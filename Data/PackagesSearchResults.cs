using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Mono.Cecil;

namespace FuGetGallery
{
    public class PackagesSearchResults
    {
        public string Query { get; set; } = "";
        public Exception Error { get; set; }
        public List<PackagesSearchResult> Results { get; set; } = new List<PackagesSearchResult> ();

        public PackagesSearchResults ()
        {
        }

        static readonly ResultsCache cache = new ResultsCache ();

        public static Task<PackagesSearchResults> GetAsync (string inputId, HttpClient httpClient)
        {
            var cleanId = (inputId ?? "").ToString().Trim().ToLowerInvariant();

            return cache.GetAsync (cleanId, httpClient);
            
            // "https://api-v2v3search-0.nuget.org/query?q=%s" (Uri.EscapeDataString (searchTerm))
            // return Task.FromResult<PackageSearchResults> (null);

        }

        void Read (string json)
        {
            var j = Newtonsoft.Json.Linq.JObject.Parse (json);
            var d = (Newtonsoft.Json.Linq.JArray)j["data"];
            foreach (var x in d) {
                var r = new PackagesSearchResult {
                    PackageId = (string)x["id"],
                    Version = (string)x["version"],
                    Description = (string)x["description"],
                    IconUrl = (string)x["iconUrl"],
                    TotalDownloads = (int)x["totalDownloads"],
                    Authors = string.Join (", ", x["authors"]),
                };
                // System.Console.WriteLine(r);
                Results.Add (r);
            }
            // Results.Sort ((a, b) => -a.TotalDownloads.CompareTo (b.TotalDownloads));
        }

        class ResultsCache : DataCache<string, PackagesSearchResults>
        {
            public ResultsCache () : base (TimeSpan.FromMinutes (15)) { }

            public static string GetQueryUrl (string filterString)
            {
                string defaultTemplate = "https://api-v2v3search-0.nuget.org/query?prerelease=true&q={0}";
                string customTemplate = string.Empty;
                string BaGetHostUrl = Environment.GetEnvironmentVariable ("BaGetHost");
                if (string.IsNullOrEmpty (BaGetHostUrl) == false) {
                    if (BaGetHostUrl.EndsWith ('/') == false) {
                        BaGetHostUrl += '/';
                    }
                    customTemplate = BaGetHostUrl + "v3/search?prerelease=true&q={0}";
                }

                string usedTemplate = defaultTemplate;
                if (string.IsNullOrEmpty (customTemplate) == false) 
                {
                    usedTemplate = customTemplate;
                }

                return string.Format (usedTemplate, Uri.EscapeDataString (filterString));
            }

            protected override async Task<PackagesSearchResults> GetValueAsync (string q, HttpClient httpClient, CancellationToken token)
            {
                var results = new PackagesSearchResults {
                    Query = q,
                };
                try {
                    // System.Console.WriteLine($"DOWNLOADING {package.DownloadUrl}");
                    var queryUrl = GetQueryUrl (q);
                    var data = await httpClient.GetStringAsync (queryUrl).ConfigureAwait (false);
                    results.Read (data);
                }
                catch (Exception ex) {
                    results.Error = ex;
                }

                return results;
            }
        }
    }

    public class PackagesSearchResult
    {
        public string PackageId { get; set; } = "";
        public string Version { get; set; } = "";
        public string Description { get; set; } = "";
        public string IconUrl { get; set; } = "";
        public int TotalDownloads { get; set; } = 0;
        public string Authors { get; set; } = "";

        public override string ToString() => PackageId;

        public string SafeIconUrl => string.IsNullOrEmpty (IconUrl) ? "/images/no-icon.png" : IconUrl;
    }
}
