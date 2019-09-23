using System;
using System.IO;
using System.Threading;
using System.Collections.Generic;
using System.Text;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using System.Linq;


namespace Hunters
{
    /// <summary>
    ///     This class is responsible for creating the search urls, performing the subsequent search, and serializing teh results into a list 
    ///     of job objects that can be written to Google Sheets later.
    /// </summary>
    public class LIService
    {
        #region PROPERTIES
        public string ChromeDriverRelativePath = (Directory.GetCurrentDirectory().Split(new string[] { "LIHunter" }, StringSplitOptions.None))[0] + @"\Hunters\chromedriver_win32";
        public string LIQueriesRelativePath = (Directory.GetCurrentDirectory().Split(new string[] { "LIHunter" }, StringSplitOptions.None))[0] + @"Hunters\LI_queries.txt";
        public string UserName { get; set; }
        public string Password { get; set; }
        public string BaseURL { get { return @"https://www.linkedin.com/jobs/search?keywords="; } }
        public string BaseSearchParams { get; set; }
        public string AdvancedSearchParams { get; set; }
        public string[] FULLURLS { get; set; }
        public object[] Queries { get; set; }
        public IWebDriver Driver { get; set; }
        public List<Job> JobResults { get; set; } = new List<Job>();
        public bool OnlyGetEasyAppy { get; set; } = true;
        #endregion


        #region CONSTRUCTORS
        /// <summary>
        ///     This primary accessory constructor takes in a list of queries and constructs their search url and creates a LIQuery object for each url.
        /// </summary>
        /// <param name="queries"></param>
        public LIService()
        {
            List<object> queries = createLIQueries();
            Queries = queries.ToArray();
            FULLURLS = new string[queries.Count];
            
            for (int counter = 0; counter<Queries.Length; counter++)
            {
                object query = queries[counter];
                if (query.GetType().Equals(typeof(LIQuery)))
                {
                    LIQuery q = (LIQuery)query;
                    setBaseSearchParams(q.KeyWords, q.City, q.State);
                    string url = BaseURL + BaseSearchParams;
                    FULLURLS[counter] = url;
                } else
                {
                    LIQueryAdvanced advq = (LIQueryAdvanced)query;
                    setBaseSearchParams(advq.KeyWords, advq.City, advq.State);                   
                    setAdvancedSearchParams(advq.JobTitles, advq.Experiences, advq.DatePosted);
                    string url = BaseURL + BaseSearchParams + AdvancedSearchParams;
                    FULLURLS[counter] = url;
                }
            }
        }
        #endregion


        #region METHODS 
        /// <summary>
        ///     This method reads the input text file and makes LIQueries for each line in the file.
        /// </summary>
        /// <param name="filepath"></param>
        /// <returns></returns>
        public List<object> createLIQueries()
        {
            string[] lines = File.ReadAllLines(LIQueriesRelativePath);
            List<object> queries = new List<object>();

            foreach (string line in lines)
            {
                string[] splitInput = line.Split('.');

                if (splitInput.Length == 3)
                {
                    LIQuery query = new LIQuery(splitInput[0], splitInput[1], splitInput[2]);
                    queries.Add(query);
                }

                if (splitInput.Length == 7)
                {
                    bool onlygeteasy = true;
                    if ((splitInput[6].Contains('f')) || (splitInput[6].Contains('F'))) onlygeteasy = false;
                    string[] jobtitles = splitInput[3].Split('|');
                    string[] experiences = splitInput[4].Split('|');
                    LIQueryAdvanced advquery = new LIQueryAdvanced(splitInput[0], splitInput[1], splitInput[2], jobtitles, experiences, splitInput[5], onlygeteasy);
                    queries.Add(advquery);
                }
            }
            return queries;
        }


        /// <summary>
        ///     This method takes in a string url and searches LinkedIn with it. It then scraps all of the jobs it finds and casts them
        ///     to Job objects and returns them in a list.
        /// </summary>
        /// <param name="url"></param>
        public List<Job> searchLI()
        {
            ChromeOptions options = new ChromeOptions();
            options.AddArguments("--incognito");

            string url;

            for (int counter = 0; counter<FULLURLS.Length; counter++)
            {
                url = FULLURLS[counter];
                OnlyGetEasyAppy = ((LIQuery)Queries[counter]).OnlyGetEasy;
                OnlyGetEasyAppy = ((LIQuery)Queries[counter]).OnlyGetEasy;
                Driver = new ChromeDriver(ChromeDriverRelativePath, options);
                Driver.Navigate().GoToUrl(url);
                Thread.Sleep(2000);
                IWebElement element;
                long scrollHeight = 0;

                Console.WriteLine("Searching for jobs with keyword " + ((LIQuery)Queries[counter]).KeyWords + " in the city of " + ((LIQuery)Queries[counter]).City + "...");

                do
                {
                    IJavaScriptExecutor js = (IJavaScriptExecutor) Driver;
                    var newScrollHeight = (long)js.ExecuteScript("window.scrollTo(0, document.body.scrollHeight); return document.body.scrollHeight;");
                    if (newScrollHeight == scrollHeight) break;
                    else scrollHeight = newScrollHeight;

                    try
                    {
                        Thread.Sleep(1000);
                        element = Driver.FindElement(By.XPath("/html/body/main/section[1]/button"));
                        Thread.Sleep(1000);
                        element.Click();
                        Thread.Sleep(1000);
                    }
                    catch (OpenQA.Selenium.NoSuchElementException ex) {
                        try
                        {
                            element = Driver.FindElement(By.ClassName("see-more-jobs"));
                            Thread.Sleep(1000);
                            element.Click();
                            Thread.Sleep(1000);
                        }
                        catch (OpenQA.Selenium.NoSuchElementException ex2) {
                            try
                            {
                                element = Driver.FindElement(By.CssSelector("body > main > div > section > button"));
                                Thread.Sleep(1000);
                                element.Click();
                                Thread.Sleep(1000);
                            }
                            catch (OpenQA.Selenium.NoSuchElementException ex3) { break; }
                        }
                    }
                } while (element != null);

                System.Collections.ObjectModel.ReadOnlyCollection<IWebElement> JobCards = Driver.FindElements(By.CssSelector("body > main > div > section > ul > li"));
                if (JobCards.Count == 0)
                {
                    Thread.Sleep(5000);
                    JobCards = Driver.FindElements(By.CssSelector("body > main > div > section > ul > li"));
                }

                foreach (IWebElement elm in JobCards)
                {
                    if (OnlyGetEasyAppy)
                    {
                        try
                        {
                            var easyApply = elm.FindElement(By.ClassName("job-result-card__easy-apply-label"));
                            if (easyApply != null)
                            {
                                string link = elm.FindElement(By.TagName("a")).GetAttribute("href");
                                string[] splitInfo = elm.Text.Split(new string[] { "\r\n" }, StringSplitOptions.None);
                                string[] refidSplit = ((link.Split(new string[] { "?refId=" }, StringSplitOptions.None))[0]).Split('-');
                                string refid = refidSplit[refidSplit.Length - 1];
                                if (splitInfo.Length >= 5)
                                {
                                    string dateposted = splitInfo[4].Replace("Easy Apply", "");
                                    Job holderJob = new Job(splitInfo[1], splitInfo[0], splitInfo[2], refid, link, dateposted, splitInfo[3], true);
                                    JobResults.Add(holderJob);
                                }
                            }
                        }
                        catch (OpenQA.Selenium.NoSuchElementException ex) { }
                    }
                    else
                    {
                        string link = elm.FindElement(By.TagName("a")).GetAttribute("href");
                        string[] splitInfo = elm.Text.Split(new string[] { "\r\n" }, StringSplitOptions.None);
                        string[] refidSplit = ((link.Split(new string[] { "?refId=" }, StringSplitOptions.None))[0]).Split('-');
                        string refid = refidSplit[refidSplit.Length - 1];
                        if (splitInfo.Length >= 5)
                        {
                            string dateposted = splitInfo[4].Replace("Easy Apply", "");
                            Job holderJob = new Job(splitInfo[1], splitInfo[0], splitInfo[2], refid, link, dateposted, splitInfo[3], false);
                            JobResults.Add(holderJob);
                        }
                    }
                }
                Driver.Close();
                Console.WriteLine("Completed Searching for jobs with keyword " + ((LIQuery)Queries[counter]).KeyWords + " in the city of " + ((LIQuery)Queries[counter]).City + "!");
            }
            getRidOfDuplicates();
            return JobResults;
        }


        /// <summary>
        ///     This method goes through the JobResults property of this class and gets rid of all duplicate entries.
        /// </summary>
        public void getRidOfDuplicates()
        {
            List<Job> holder = (JobResults.ToArray()).GroupBy(x => x.RefID).Select(x => x.First()).ToList();
            JobResults = holder;
        }


        /// <summary>
        ///     This method adds the required params to the search params for LinkedIn.
        /// </summary>
        /// <param name="keywords"></param>
        /// <param name="city"></param>
        /// <param name="state"></param>
        public void setBaseSearchParams(string keywords, string city, string state)
        {
            string keywordsWithoutSpaces = keywords.Replace(" ", "%20");
            BaseSearchParams = keywordsWithoutSpaces + "&location=" + city + "%2C%20" + state + "%2C%20United%20States&";
        }


        /// <summary>
        ///     This method adds the not required search params to the LinkedIn search url.
        /// </summary>
        /// <param name="jobtitles"></param>
        /// <param name="experiences"></param>
        /// <param name="timesposted"></param>
        public void setAdvancedSearchParams(string[] jobtitles, string[] experiences, string timesposted)
        {
            int counter = 0;
            foreach (string job in jobtitles)
            {
                if (counter == 0) AdvancedSearchParams += "&f_JT=";
                else AdvancedSearchParams += "%2";

                if (job.Contains("full")) AdvancedSearchParams += "F";
                else if (job.Contains("contract")) AdvancedSearchParams += "CC";
                else if (job.Contains("part")) AdvancedSearchParams += "P";

                counter++;
            }

            counter = 0;

            foreach (string exp in experiences)
            {
                if (counter == 0) AdvancedSearchParams += "&f_E=";
                else AdvancedSearchParams += "%2";

                if (exp.Contains("entry")) AdvancedSearchParams += "2";
                else if (exp.Contains("associate")) AdvancedSearchParams += "C3";

                counter++;
            }

            if (timesposted.Contains("day")) AdvancedSearchParams += "&f_TP=1";
            else if (timesposted.Contains("month")) AdvancedSearchParams += "&f_TP=1%2C2";
            else if (timesposted.Contains("week")) AdvancedSearchParams += "&f_TP=1%2C2%2C3%2C4";
        }
        #endregion
    }
}