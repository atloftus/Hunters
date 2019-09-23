using System;
using System.Collections.Generic;
using System.Text;


namespace Hunters
{
    /// <summary>
    ///     This class houses all of the properties of a Job posting from LinkedIN which correlate to a row in the 
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
        #endregion
    }
}