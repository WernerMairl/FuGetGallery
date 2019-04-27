using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace HiGet.Web
{
    public class PackageVersions
    {
        public string LowerId { get; set; } = "";
        public List<PackageVersion> Versions { get; set; } = new List<PackageVersion> ();
        public Exception Error { get; set; }

        static readonly PackageVersionsCache cache = new PackageVersionsCache ();

        public static Task<PackageVersions> GetAsync (object inputId, HttpClient client, CancellationToken token)
        {
            var cleanId = (inputId ?? "").ToString().Trim().ToLowerInvariant();
            return cache.GetAsync (cleanId,client, token);
        }

        public PackageVersion GetVersion (object inputVersion)
        {
            var version = (inputVersion ?? "").ToString().Trim().ToLowerInvariant();
            var v = Versions.FirstOrDefault (x => x.VersionString == version);
            if (v == null) {
                v = Versions.LastOrDefault ();
            }
            if (v == null) {
                v = new PackageVersion {
                    VersionString = version.Length > 0 ? version : "0",
                };
            }
            return v;
        }

        void Read (JArray items)
        {
            foreach (var v in items) {
                DateTime? time = null;
                var times = v["catalogEntry"]?["published"];
                if (times != null) {
                    time = (DateTime)times;
                }
                var version = v["version"]?.ToString();
                if (version == null) {
                    version = v["catalogEntry"]?["version"]?.ToString();
                }
                if (version != null) {
                    Versions.Add (new PackageVersion { VersionString = version, PublishTime = time });
                }
            }
        }

        class PackageVersionsCache : DataCache<string, PackageVersions>
        {
            public PackageVersionsCache () : base (TimeSpan.FromMinutes (20)) { }

            protected override async Task<PackageVersions> GetValueAsync(string lowerId, HttpClient httpClient, CancellationToken token)
            {
                var package = new PackageVersions {
                    LowerId = lowerId,
                };
                try {
                    var url = Tools.GetRegistrationUrl (lowerId);
                    var rootJson = await httpClient.GetStringAsync (url).ConfigureAwait (false);
                    // System.Console.WriteLine(rootJson + "\n\n\n\n");
                    var root = JObject.Parse(rootJson);
                    var pages = (JArray)root["items"];
                    if (pages.Count > 0) {
                        var lastPage = pages.Last();
                        var lastPageItems = lastPage["items"] as JArray;
                        if (lastPageItems != null) {
                            package.Read (lastPageItems);
                        }
                        else {
                            foreach (var p in pages.Reverse ()) {
                                var pageUrl = p["@id"].ToString ();
                                var pageRootJson = await httpClient.GetStringAsync (pageUrl).ConfigureAwait (false);
                                var pageRoot = JObject.Parse (pageRootJson);
                                package.Read ((JArray)pageRoot["items"]);
                            }
                        }
                    }
                    package.Versions.Sort ((x, y) => x.CompareTo (y));
                }
                catch (Exception ex) {
                    package.Error = ex;
                }

                return package;
            }
        }
    }

    public class PackageVersion : IComparable<PackageVersion>
    {
        string versionString = "";

        public int Major { get; private set; }
        public int Minor { get; private set; }
        public int Patch { get; private set; }
        public int Build { get; private set; }
        public string Rest { get; private set; } = "";
        public DateTime? PublishTime { get; set; }

        public bool IsPublished => PublishTime.HasValue && PublishTime.Value.Year > 1970;

        public string VersionString { 
            get => versionString; 
            set {
                if (versionString == value || string.IsNullOrEmpty (value))
                    return;
                versionString = value;
                var di = value.IndexOf ('-');
                var vpart = di > 0 ? value.Substring (0, di) : value;
                var rest = di > 0 ? value.Substring (di) : "";
                var parts = vpart.Split ('.');
                var maj = 0;
                var min = 0;
                var patch = 0;
                var build = 0;
                if (parts.Length > 0) int.TryParse(parts[0], out maj);
                if (parts.Length > 1) int.TryParse(parts[1], out min);
                if (parts.Length > 2) int.TryParse(parts[2], out patch);
                if (parts.Length > 3) int.TryParse (parts[3], out build);
                Major = maj;
                Minor = min;
                Patch = patch;
                Build = build;
                Rest = rest;
            }
        }

        public int CompareTo(PackageVersion other)
        {
            var c = Major.CompareTo(other.Major);
            if (c != 0) return c;
            c = Minor.CompareTo(other.Minor);
            if (c != 0) return c;
            c = Patch.CompareTo(other.Patch);
            if (c != 0) return c;
            c = Build.CompareTo (other.Build);
            if (c != 0) return c;
            return string.Compare(Rest, other.Rest, StringComparison.Ordinal);
        }

        public override bool Equals(object obj)
        {
            return obj is PackageVersion v && VersionString == v.VersionString;
        }

        public override int GetHashCode()
        {
            return VersionString.GetHashCode ();
        }

        public override string ToString() => VersionString;
    }
}
