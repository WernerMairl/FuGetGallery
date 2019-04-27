using System;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
namespace HiGet.Web
{
    internal sealed class PackageDataCache : DataCache<string, PackageVersion, PackageData>
    {
        public PackageDataCache() : base()
        {
        }

        protected override async Task<PackageData> GetValueAsync(string packageId, PackageVersion packageVersion, HttpClient httpClient, CancellationToken token)
        {
            var id = packageId;
            var version = packageVersion;
            var package = new PackageData
            {
                Id = id,
                IndexId = id,
                Version = version,
                SizeInBytes = 0,
                DownloadUrl = Tools.GetPackageDownloadUrl(id,version)
            };
            try
            {
                var r = await httpClient.GetAsync(package.DownloadUrl, token).ConfigureAwait(false);
                var data = new MemoryStream();
                using (var s = await r.Content.ReadAsStreamAsync().ConfigureAwait(false))
                {
                    await s.CopyToAsync(data, 16 * 1024, token).ConfigureAwait(false);
                }
                data.Position = 0;
                await Task.Run(() => package.Read(data, httpClient), token).ConfigureAwait(false);
                await package.MatchLicenseAsync(httpClient).ConfigureAwait(false);
                await package.SaveDependenciesAsync();
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception ex)
            {
                package.Error = ex;
            }

            return package;
        }
    }
}

