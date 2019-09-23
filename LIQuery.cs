using System;
using System.Collections.Generic;
using System.Text;


namespace Hunters
{
    /// <summary>
    ///     This class contains all of the properties of a basic LinkedIN Query.
    /// </summary>
    public class LIQuery
    {
        #region PROPERTIES
        public string KeyWords { get; set; } = "Software Engineering";
        public string City { get; set; } = "Chicago";
        public string State { get; set; } = "Illinois";
        public bool OnlyGetEasy { get; set; } = true;
        #endregion



        #region CONSTRUCTORS
        /// <summary>
        ///     This is the default constructor that doesn't do anyhting.
        /// </summary>
        public LIQuery() { }


        /// <summary>
        ///     This is the accessory constructor that sets the keywords, city and state correctly. 
        /// </summary>
        /// <param name="keywords"></param>
        /// <param name="city"></param>
        /// <param name="state"></param>
        public LIQuery(string keywords, string city, string state)
        {
            KeyWords = keywords;
            City = city;
            State = state;
        }
        #endregion
    }



    /// <summary>
    ///     This class contains all of the properties of an Advanced LinkedIN Query.
    /// </summary>
    public class LIQueryAdvanced : LIQuery
    {
        #region PROPERTIES
        public string[] JobTitles { get; set; } = new string[1] { "fulltime" };
        public string[] Experiences { get; set; } = new string[1] { "entry" };
        public string DatePosted { get; set; } = "week";
        #endregion



        #region CONSTRUCTORS
        /// <summary>
        ///     This is the default constructor that doesn't do anyhting.
        /// </summary>
        public LIQueryAdvanced() { }


        /// <summary>
        ///     This is the advanced construtor that extends the base LIQuery constructor as well as takingi n and setting the advanced search parameters that
        ///     are the differentiating factors between LIQuery and LIQueryAdvanced.
        /// </summary>
        /// <param name="keywords"></param>
        /// <param name="city"></param>
        /// <param name="state"></param>
        /// <param name="jobtitles"></param>
        /// <param name="experiences"></param>
        /// <param name="dateposted"></param>
        /// <param name="onlygeteasy"></param>
        public LIQueryAdvanced(string keywords, string city, string state, string[] jobtitles, string[] experiences, string dateposted, bool onlygeteasy) : base(keywords, city, state)
        {
            JobTitles = jobtitles;
            Experiences = experiences;
            DatePosted = dateposted;
            OnlyGetEasy = onlygeteasy;
        }
        #endregion
    }
}