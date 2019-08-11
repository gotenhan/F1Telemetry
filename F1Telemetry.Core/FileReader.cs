using System.IO;
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
    }
}