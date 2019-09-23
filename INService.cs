using System;
using System.Collections.Generic;
using System.Text;

namespace Hunters
{
    public class INService
    {
        public string BaseURL { get { return @"https://www.linkedin.com/jobs/search?keywords="; } }
        public string BaseSearchParams { get; set; }
        public string AdvancedSearchParams { get; set; }
        public string[] FULLURLS { get; set; }
        public object[] Queries { get; set; }

        public INService()
        {
            BaseSearchParams = "testing";
        }

        public string testmethod()
        {
            return "It works!";
        }
    }
}
