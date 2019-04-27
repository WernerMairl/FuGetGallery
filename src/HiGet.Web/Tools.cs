using HiGet.Web.Configuration.Settings;
using System;
using NuGet.Versioning;

namespace HiGet.Web
{
    public static class Tools
    {
        public static string GetRegistrationUrl(string lowerId)
        {
            string hostUrl = Environment.GetEnvironmentVariable(InternalEnvName);
            return InternalGetRegistrationUrl(hostUrl, lowerId);
        }

        internal static string InternalGetRegistrationUrl(string hostUrl, string lowerId)
        {
            string defaultTemplate = "https://api.nuget.org/v3/registration3/{0}/index.json";
            string customTemplate = string.Empty;
            string usedTemplate = defaultTemplate;
            if (string.IsNullOrEmpty(hostUrl) == false)
            {

                if (hostUrl.EndsWith('/') == false)
                {
                    hostUrl += '/';
                }
                customTemplate = hostUrl + "v3/registration/{0}/index.json";
            }

            if (string.IsNullOrEmpty(customTemplate) == false)
            {
                usedTemplate = customTemplate;
            }

            return string.Format(usedTemplate, Uri.EscapeDataString(lowerId));
        }
        public static string GetQueryUrl(string filterString)
        {
            string hostUrl = Environment.GetEnvironmentVariable(InternalEnvName);
            return InternalGetQueryUrl(hostUrl,filterString);
        }

        internal static string InternalGetQueryUrl(string hosttUrl, string filterString)
        {
            string defaultTemplate = "https://api-v2v3search-0.nuget.org/query?prerelease=true&q={0}";
            string customTemplate = string.Empty;
            if (string.IsNullOrEmpty(hosttUrl) == false)
            {
                if (hosttUrl.EndsWith('/') == false)
                {
                    hosttUrl += '/';
                }
                customTemplate = hosttUrl + "v3/search?prerelease=true&q={0}";
            }

            string usedTemplate = defaultTemplate;
            if (string.IsNullOrEmpty(customTemplate) == false)
            {
                usedTemplate = customTemplate;
            }

            return string.Format(usedTemplate, Uri.EscapeDataString(filterString));
        }

        private static readonly string InternalEnvName = "HIGET_NUGET_HOST";

        public static void ConfigureNuGetHostUrl(AppSettings settings)
        {
            if (settings == null) throw new ArgumentNullException(nameof(settings));
            string v = string.Empty;
            if (settings.NuGetHostUrl != null)
            {
                v = settings.NuGetHostUrl.ToString();
            }
            Environment.SetEnvironmentVariable(InternalEnvName, v);
        }
        public static string GetPackageDownloadUrl(string id, PackageVersion version)
        {
           string hostUrl = Environment.GetEnvironmentVariable(InternalEnvName);
           return InternalGetPackageDownloadUrl(hostUrl, id, version);
        }

        internal static string InternalGetPackageDownloadUrl(string hostUrl, string id, PackageVersion version)
        {
            string defaultTemplate = "https://www.nuget.org/api/v2/package/{0}/{1}";
            string customTemplate = string.Empty;

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
}
