using System.IO;
using System.Threading.Tasks;

using Discord.WebSocket;
using PKHeX.Core;
using Image = System.Drawing.Image;
using ImageFormat = System.Drawing.Imaging.ImageFormat;

namespace PKHeX.Discord
{
    public static class ReusableActions
    {
        public static async Task SendPKMAsync(this ISocketMessageChannel channel, PKM pkm, string msg = "")
        {
            var tmp = Path.Combine(Path.GetTempPath(), Util.CleanFileName(pkm.FileName));
            File.WriteAllBytes(tmp, pkm.DecryptedPartyData);
            await channel.SendFileAsync(tmp, msg).ConfigureAwait(false);
            File.Delete(tmp);
        }

        public static async Task SendImageAsync(this ISocketMessageChannel channel, Image finalQR, string msg = "")
        {
            const string fn = "tmp.png";
            finalQR.Save(fn, ImageFormat.Png);
            await channel.SendFileAsync(fn, msg).ConfigureAwait(false);
        }
    }
}