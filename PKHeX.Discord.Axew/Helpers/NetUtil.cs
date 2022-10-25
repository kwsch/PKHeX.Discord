using System.Net.Http;
using System.Threading.Tasks;
using Discord;
using PKHeX.Core;

namespace PKHeX.Discord.Axew
{
    public static class NetUtil
    {
        private static readonly HttpClient httpClient = new();

        public static async Task<byte[]> DownloadFromUrlAsync(string url)
        {
            return await httpClient.GetByteArrayAsync(url).ConfigureAwait(false);
        }

        public static async Task<Download<PKM>> DownloadPKMAsync(IAttachment att)
        {
            var result = new Download<PKM> { SanitizedFileName = Format.Sanitize(att.Filename) };
            if (!EntityDetection.IsSizePlausible(att.Size))
            {
                result.ErrorMessage = $"{result.SanitizedFileName}: Invalid size.";
                return result;
            }

            string url = att.Url;

            // Download the resource and load the bytes into a buffer.
            var buffer = await DownloadFromUrlAsync(url).ConfigureAwait(false);
            var prefer = EntityFileExtension.GetContextFromExtension(result.SanitizedFileName);
            var pkm = EntityFormat.GetFromBytes(buffer, prefer);
            if (pkm == null)
            {
                result.ErrorMessage = $"{result.SanitizedFileName}: Invalid pkm attachment.";
                return result;
            }

            result.Data = pkm;
            result.Success = true;
            return result;
        }
    }

    public sealed class Download<T>
    {
        public bool Success;
        public T Data = default!;
        public string SanitizedFileName = string.Empty;
        public string ErrorMessage = string.Empty;
    }
}