using System.IO;
using System.IO.Compression;
using System.IO.Pipelines;
using System.Threading;
using System.Threading.Tasks;

namespace F1Telemetry.Core
{
    public static class FileReader
    {
        public static async Task ReadFromFile(string path, PipeWriter writer, CancellationToken ct = default)
        {
            using (var fileStream = File.Open(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                await fileStream.CopyToAsync(writer, ct).ConfigureAwait(false);
                writer.Complete();
            }
        }

        public static async Task ReadFromGzippedFile(string path, PipeWriter writer, CancellationToken ct = default)
        {
            using (var fileStream = File.Open(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                using (var ungzipStream = new GZipStream(fileStream, CompressionMode.Decompress))
                {
                    await ungzipStream.CopyToAsync(writer, ct).ConfigureAwait(false);
                    writer.Complete();
                }
            }
        }
    }
}