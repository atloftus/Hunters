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
    public class INService
    {
        #region PROPERTIES
        public string ChromeDriverRelativePath = (Directory.GetCurrentDirectory().Split(new string[] { "repos" }, StringSplitOptions.None))[0] + @"repos\Hunters\chromedriver_win32";
        public string INQueriesRelativePath = (Directory.GetCurrentDirectory().Split(new string[] { "repos" }, StringSplitOptions.None))[0] + @"repos\Hunters\IN_queries.txt";
        public string BaseURL { get { return @"https://www.indeed.com/jobs?"; } }
        public string BaseSearchParams { get; set; }
        public string AdvancedSearchParams { get; set; }
        public string[] FULLURLS { get; set; }
        public object[] Queries { get; set; }
        public IWebDriver Driver { get; set; }
        public List<Job> JobResults { get; set; } = new List<Job>();
        public bool OnlyGetEasyAppy { get; set; } = true;
        #endregion



        #region CONSTRUCTORS
        public INService()
        {
            //TODO: Possibly add some sort of location resolution (maybe a library)
            //TODO: Possibly allow different search radius options
            List<object> queries = createINQueries();
            Queries = queries.ToArray();
            FULLURLS = new string[queries.Count];

            for (int counter = 0; counter < Queries.Length; counter++)
            {
                object query = queries[counter];
                if (query.GetType().Equals(typeof(INQuery)))
                {
                    INQuery q = (INQuery)query;
                    setBaseSearchParams(q.KeyWords, q.City, q.State);
                    string url = BaseURL + BaseSearchParams;
                    FULLURLS[counter] = url;
                }
                else
                {
                    INQueryAdvanced advq = (INQueryAdvanced)query;
                    setBaseSearchParams(advq.KeyWords, advq.City, advq.State);
                    setAdvancedSearchParams(advq.JobTitle, advq.Experience);
                    string url = BaseURL + BaseSearchParams + AdvancedSearchParams;
                    FULLURLS[counter] = url;
                }
            }
        }
        #endregion



        #region METHODS 
        /// <summary>
        ///     This method reads the input text file and makes INQueries for each line in the file.
        /// </summary>
        /// <param name="filepath"></param>
        /// <returns></returns>
        public List<object> createINQueries()
        {
            string[] lines = File.ReadAllLines(INQueriesRelativePath);
            List<object> queries = new List<object>();

            foreach (string line in lines)
            {
                string[] splitInput = line.Split('.');

                if (splitInput.Length == 3)
                {
                    INQuery query = new INQuery(splitInput[0], splitInput[1], splitInput[2]);
                    queries.Add(query);
                }

                if (splitInput.Length == 6)
                {
                    bool onlygeteasy = true;
                    if ((splitInput[5].Contains('f')) || (splitInput[5].Contains('F'))) onlygeteasy = false;
                    INQueryAdvanced advquery = new INQueryAdvanced(splitInput[0], splitInput[1], splitInput[2], splitInput[3], splitInput[4], onlygeteasy);
                    queries.Add(advquery);
                }
            }
            return queries;
        }


        /// <summary>
        ///     This method takes in a string url and searches Indeed with it. It then scraps all of the jobs it finds and casts them
        ///     to Job objects and returns them in a list.
        /// </summary>
        /// <param name="url"></param>
        public List<Job> searchIN()
        {
            ChromeOptions options = new ChromeOptions();
            options.AddArguments("--incognito");
            string url;
            List<string> JobCards = new List<string>();

            for (int counter = 0; counter < FULLURLS.Length; counter++)
            {
                url = FULLURLS[counter];
                OnlyGetEasyAppy = ((INQuery)Queries[counter]).OnlyGetEasy;
                Driver = new ChromeDriver(ChromeDriverRelativePath, options);
                Driver.Navigate().GoToUrl(url);
                Thread.Sleep(4000);
                IWebElement element;
                long scrollHeight = 0;

                Console.WriteLine("Searching for jobs with keyword " + ((INQuery)Queries[counter]).KeyWords + " in the city of " + ((INQuery)Queries[counter]).City + "...");

                int resultCount = getNumberOfSearchResults();
                Console.WriteLine("The search yielded " + resultCount + " results.");
                int numberOfPages = (int) Math.Floor(resultCount/10.00);
                List<string> nextPages = new List<string>();

                for (int a = 1; a <= numberOfPages; a++)
                {
                    string tempURL = url + "&start=" + (a*10).ToString();
                    nextPages.Add(tempURL);
                }

                Thread.Sleep(2000);
                System.Collections.ObjectModel.ReadOnlyCollection<IWebElement> holderJobCards = Driver.FindElement(By.XPath("/ html / body / table[2] / tbody / tr / td / table / tbody / tr / td[2]")).FindElements(By.TagName("div"));
                foreach(IWebElement elm in holderJobCards)
                {
                    if (elm.GetAttribute("class") == "jobsearch-SerpJobCard unifiedRow row result clickcard") createJobFromWebElement(elm);
                }


                foreach(string nextUrl in nextPages)
                {
                    Driver.Navigate().GoToUrl(nextUrl);
                    Thread.Sleep(4000);
                    holderJobCards = Driver.FindElement(By.XPath("/ html / body / table[2] / tbody / tr / td / table / tbody / tr / td[2]")).FindElements(By.TagName("div"));
                    foreach (IWebElement elm in holderJobCards)
                    {
                        if (elm.GetAttribute("class") == "jobsearch-SerpJobCard unifiedRow row result clickcard") createJobFromWebElement(elm);
                    }
                }
                //TODO: Need to add some error checking to make sure that I'm getting exactly the right set of jobs
            }

            getRidOfDuplicates();
            return JobResults;
        }


        /// <summary>
        ///     This method scrapes the number of job results from a Indeed search/the page the Driver property is on.
        /// </summary>
        /// <returns></returns>
        public int getNumberOfSearchResults()
        {
            var resultElm = Driver.FindElement(By.Id("searchCountPages"));
            string resultStringFull = resultElm.Text;
            string[] resultSplit = resultStringFull.Split(' ');
            string resultString = resultSplit[3];
            int resultCount = Int32.Parse(resultString);
            return resultCount;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="elm"></param>
        public void createJobFromWebElement(IWebElement elm)
        {
            try
            {
                string refid = elm.GetAttribute("data-jk");
                string link = "https://www.indeed.com/viewjob?jk=" + refid;
                string[] splitInfo = elm.Text.Split(new string[] { "\r\n" }, StringSplitOptions.None);
                bool isEasyApply = false;
                string[] locationSplit = splitInfo[2].Split(' ');
                string location = locationSplit[0] + ' ' + locationSplit[1];
                string company = splitInfo[1];
                if (company.Contains("review"))
                {
                    string[] companySplit = company.Split(' ');
                    int length = companySplit.Length - 2;
                    company = companySplit[0] + ' ';
                    for (int a = 1; a < length; a++) company += companySplit[a] + ' ';
                }

                string datePosted = splitInfo[4];
                if (datePosted.Contains("day"))
                {
                    string[] datePostedSplit = datePosted.Split(new string[] { "day" }, StringSplitOptions.None);
                    datePosted = datePostedSplit[0] + "days";
                } else
                {
                    datePosted = "";
                }
                try
                {
                    var easyApplyButton = elm.FindElement(By.ClassName("iaLabel"));                   
                    if (easyApplyButton != null) isEasyApply = true;
                }
                catch (Exception exp) { }

                Job holderJob = new Job(company, splitInfo[0], location, refid, link, datePosted, splitInfo[3], isEasyApply);
                JobResults.Add(holderJob);
            }
            catch (Exception ex) { }
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
        ///     This method adds the required params to the search params for Indeed.
        /// </summary>
        /// <param name="keywords"></param>
        /// <param name="city"></param>
        /// <param name="state"></param>
        public void setBaseSearchParams(string keywords, string city, string state)
        {
            string keywordsWithoutSpaces = keywords.Replace(" ", "+");
            BaseSearchParams = "q=" + keywordsWithoutSpaces + "&l=" + city + ",+" + state;
        }


        /// <summary>
        ///     This method adds the not required search params to the Indeed search url.
        /// </summary>
        /// <param name="jobtitles"></param>
        /// <param name="experiences"></param>
        /// <param name="timesposted"></param>
        public void setAdvancedSearchParams(string jobtitle, string experience)
        {
            if (jobtitle != "")
            {
                AdvancedSearchParams += "&jt=";
                if (jobtitle.Contains("full")) AdvancedSearchParams += "fulltime";
                else if (jobtitle.Contains("contract")) AdvancedSearchParams += "contract";
                else if (jobtitle.Contains("part")) AdvancedSearchParams += "parttime";
            }

            if (experience != "")
            {
                AdvancedSearchParams += "&explvl=";
                if (experience.Contains("senior")) AdvancedSearchParams += "senior_level";
                else if (experience.Contains("mid")) AdvancedSearchParams += "mid_level";
                else AdvancedSearchParams += "entry_level";
            }
        }
        #endregion
    }
}
