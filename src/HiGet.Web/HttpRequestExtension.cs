using System.Text;
using Microsoft.AspNetCore.Http;

namespace HiGet.Web
{

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
