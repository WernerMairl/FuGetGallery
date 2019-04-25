using System.IO.Compression;

namespace HiGet.Web
{
    public class PackageFile
    {
        public ZipArchiveEntry ArchiveEntry { get; }
        public string FileName => ArchiveEntry?.Name;
        public long SizeInBytes => ArchiveEntry != null ? ArchiveEntry.Length : 0;

        public PackageFile (ZipArchiveEntry entry)
        {
            ArchiveEntry = entry;
        }
    }
}
