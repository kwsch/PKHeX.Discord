using System.Net;
using System.Threading.Tasks;

namespace PKHeX.Discord
{
    public static class NetUtil
    {
        private static readonly WebClient webClient = new WebClient();

        public static async Task<byte[]> DownloadFromUrlAsync(string url)
        {
            return await webClient.DownloadDataTaskAsync(url).ConfigureAwait(false);
        }
    }
}