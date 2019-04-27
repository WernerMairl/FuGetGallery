using System.Text;
using Microsoft.AspNetCore.Http;
using HiGet.Web.Configuration.Settings;
using System;
using NuGet.Versioning;

namespace HiGet.Web
{

    public static class Tools
    {
        public static string GetPackageDownloadUrl(AppSettings settings, string id, PackageVersion version)
        {
            string defaultTemplate = "https://www.nuget.org/api/v2/package/{0}/{1}";
            string customTemplate = string.Empty;
            //string BaGetHostUrl = Environment.GetEnvironmentVariable("BaGetHost");
            string hostUrl = string.Empty;
            if (settings.NuGetHostUrl != null)
            {
                hostUrl = settings.NuGetHostUrl.ToString();
            }
            if (string.IsNullOrEmpty(hostUrl) == false)
            {
                if (hostUrl.EndsWith('/') == false)
                {
                    hostUrl += '/';
                }
                customTemplate = hostUrl + "v3/package/{0}/{1}/{2}.nupkg";
            }

            string usedTemplate = defaultTemplate;

            if (string.IsNullOrEmpty(customTemplate) == false)
            {
                usedTemplate = customTemplate;
            }

            var nugetVersion = new NuGetVersion(version.VersionString);
            var idVersion = $"{id}.{nugetVersion.ToNormalizedString().ToLowerInvariant()}";

            string id_escaped = Uri.EscapeDataString(id.ToLowerInvariant());
            string versionString_escaped = Uri.EscapeDataString(version.VersionString);
            string idversion_escaped = Uri.EscapeDataString(idVersion);

            string finalUrl = string.Format(usedTemplate, id_escaped, versionString_escaped, idversion_escaped);
            return finalUrl;
        }
    }

    public static class HttpRequestExtension
    {
        //private const char ForwardSlash = '/';
        //private const char Hash = '#';
        //private const char QuestionMark = '?';
        private const string SchemeDelimiter = "://";

        public static string GetRootUrl(this HttpRequest request)
        {
            var scheme = request.Scheme ?? string.Empty;
            var host = request.Host.Value ?? string.Empty;
            var pathBase = request.PathBase.Value ?? string.Empty;
            //var path = request.Path.Value ?? string.Empty;
            //var queryString = request.QueryString.Value ?? string.Empty;

            // PERF: Calculate string length to allocate correct buffer size for StringBuilder.
            var length = scheme.Length + SchemeDelimiter.Length + host.Length
                + pathBase.Length;// + path.Length + queryString.Length;

            return new StringBuilder(length)
                .Append(scheme)
                .Append(SchemeDelimiter)
                .Append(host)
                .Append(pathBase)
                //.Append(path)
                //.Append(queryString)
                .ToString();
        }
    }
}
