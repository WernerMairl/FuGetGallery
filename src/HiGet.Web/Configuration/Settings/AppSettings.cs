using System;

namespace HiGet.Web.Configuration.Settings
{
    public class AppSettings
    {
        public Uri NuGetHostUrl { get; set; }
        public string Title { get; set; }
        public Uri Url { get; set; }

        public long PackageDataCacheTtlMinutes { get; set; } = 60 * 24 * 7; //7 days

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
