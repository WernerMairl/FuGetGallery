using System;
//using CoreWiki.Notifications.Abstractions.Configuration;

namespace HiGet.Web.Configuration.Settings
{
    public class AppSettings
    {
        public string Title { get; set; }
        public Uri Url { get; set; }
        //public Connectionstrings ConnectionStrings { get; set; }
        //public Comments Comments { get; set; }
        //public EmailNotifications EmailNotifications { get; set; }
        //public CspSettings CspSettings { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <returns>base Url without Trailing slash</returns>
        public string GetRootUrl(string defaultValue)
        {
            if (this.Url == null)
            {
                return defaultValue;
            }
            else return this.Url.ToString();
        }

        public string TitleLine1 { get; set; } = "HIGET.org.Title";

        public string TitleLine2 { get; set; } = "HIGET package browsing small";


    }
}
