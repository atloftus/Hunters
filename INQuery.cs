using System;
using System.Collections.Generic;
using System.Text;


namespace Hunters
{
    /// <summary>
    ///     This class contains all of the properties of a basic LinkedIN Query.
    /// </summary>
    public class INQuery
    {
        #region PROPERTIES
        public string KeyWords { get; set; } = "Software Engineering";
        public string City { get; set; } = "Chicago";
        public string State { get; set; } = "IL";
        public bool OnlyGetEasy { get; set; } = true;
        #endregion



        #region CONSTRUCTORS
        /// <summary>
        ///     This is the default constructor that doesn't do anyhting.
        /// </summary>
        public INQuery() { }


        /// <summary>
        ///     This is the accessory constructor that sets the keywords, city and state correctly. 
        /// </summary>
        /// <param name="keywords"></param>
        /// <param name="city"></param>
        /// <param name="state"></param>
        public INQuery(string keywords, string city, string state)
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
    public class INQueryAdvanced : INQuery
    {
        #region PROPERTIES
        public string JobTitle { get; set; } = "fulltime";
        public string Experience { get; set; } = "entry";
        #endregion



        #region CONSTRUCTORS
        /// <summary>
        ///     This is the default constructor that doesn't do anyhting.
        /// </summary>
        public INQueryAdvanced() { }


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
        public INQueryAdvanced(string keywords, string city, string state, string jobtitle, string experience, bool onlygeteasy) : base(keywords, city, state)
        {
            JobTitle = jobtitle;
            Experience = experience;
            OnlyGetEasy = onlygeteasy;
        }
        #endregion
    }
}