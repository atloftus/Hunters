using System;
using System.Collections.Generic;
using System.Text;


namespace Hunters
{
    /// <summary>
    ///     This class houses all of the properties of a Job posting from LinkedIN & Indeed which correlate to a row in the 
    ///     resulting Google Sheet.
    /// </summary>
    public class Job
    {
        #region PROPERTIES
        public string CompanyName { get; set; }
        public string Position { get; set; }
        public string Location { get; set; }
        public string DatePosted { get; set; }
        public string DateApplied { get; set; }
        public string DateAddedToSheet { get => DateTime.Now.ToString(); }
        public string Link { get; set; }
        public string Details { get; set; }
        public bool IsEasyApply { get; set; }
        public string RefID { get; set; }
        public string Website { get; set; } = "LinkedIn";
        #endregion


        #region CONSTRUCTORS
        /// <summary>
        ///     This is the most basic accessory constructor that only takes in teh required fields of company name, job position, location
        ///     and the refid of the job posting on LinkedIn.
        /// </summary>
        /// <param name="company"></param>
        /// <param name="position"></param>
        /// <param name="location"></param>
        /// <param name="refid"></param>
        public Job(string company, string position, string location, string refid)
        {
            CompanyName = company;
            Position = position;
            Location = location;
            RefID = refid;
        }


        /// <summary>
        ///     This is accessory copnstructor extends the previous accessory constructor as well as adding the link, dateposted and details fields.
        /// </summary>
        /// <param name="company"></param>
        /// <param name="position"></param>
        /// <param name="location"></param>
        /// <param name="refid"></param>
        /// <param name="link"></param>
        /// <param name="dateposted"></param>
        /// <param name="details"></param>
        public Job(string company, string position, string location, string refid, string link, string dateposted, string details) : this(company, position, location, refid)
        {
            Link = link;
            DatePosted = dateposted;
            Details = details;
            if (link.Contains("indeed")) Website = "Indeed";
            else Website = "LinkedIn";
        }


        /// <summary>
        ///     This is accessory copnstructor extends the previous accessory constructor as well as adding the iseasyapply field.
        /// </summary>
        /// <param name="company"></param>
        /// <param name="position"></param>
        /// <param name="location"></param>
        /// <param name="refid"></param>
        /// <param name="link"></param>
        /// <param name="dateposted"></param>
        /// <param name="details"></param>
        /// <param name="iseasyapply"></param>
        public Job(string company, string position, string location, string refid, string link, string dateposted, string details, bool iseasyapply) : this(company, position, location, refid, link, dateposted, details)
        {
            IsEasyApply = iseasyapply;
        }


        /// <summary>
        ///     This is the accessory constructor that takes in a Google Sheets row and serializes it to a Job object.
        /// </summary>
        /// <param name="row"></param>
        public Job(IList<object> row) : this((string)row[0], (string)row[2], (string)row[1], (string)row[8], (string)row[7], (string)row[4], (string)row[6], (bool)row[3])
        {
            if (((string)row[9] == "") || ((string)row[9] == " ")) DateApplied = "";
            else DateApplied = (string)row[9];
        }
        #endregion



        #region METHODS
        /// <summary>
        ///     This method prints out all of the properties of the given job to the console.
        /// </summary>
        public void printJob()
        {
            Console.WriteLine("----------JOB: " + RefID + "----------");
            Console.WriteLine("Position: " + Position);
            Console.WriteLine("Company: " + CompanyName);
            Console.WriteLine("Location: " + Location);
            Console.WriteLine("Date Posted To Site: " + DatePosted);
            Console.WriteLine("Date Applied: " + DateApplied);
            Console.WriteLine("Date Added To Sheet: " + DateAddedToSheet);
            Console.WriteLine("Link: " + Link);
            Console.WriteLine("IsEasyApply: " + IsEasyApply);
            Console.WriteLine("RefId: " + RefID);
            Console.WriteLine("Website: " + Website);
            Console.WriteLine("-----------------------");
        }


        /// <summary>
        ///     This method serializes the given job object to an IList<object> so it can be entered into Google Sheets.
        /// </summary>
        /// <returns></returns>
        public IList<object> toSheetsRow() { return new List<object>() { CompanyName, Location, Position, IsEasyApply, DatePosted, DateAddedToSheet, Details, Link, RefID, Website, DateApplied }; }
        #endregion
    }
}