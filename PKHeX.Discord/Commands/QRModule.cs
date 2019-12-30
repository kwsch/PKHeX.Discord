using System.Threading.Tasks;
using Discord;
using Discord.Commands;

namespace PKHeX.Discord
{
    public class QRModule : ModuleBase<SocketCommandContext>
    {
        [Command("qr")]
        [Summary("Converts the pkm(s) to QR.")]
        public async Task GenerateQRCodes()
        {
            var attachments = Context.Message.Attachments;
            foreach (var att in attachments)
                await QRPKM(att).ConfigureAwait(false);
        }

        private async Task QRPKM(IAttachment att)
        {
            var download = await NetUtil.DownloadPKMAsync(att).ConfigureAwait(false);
            if (!download.Success)
            {
                await ReplyAsync(download.ErrorMessage).ConfigureAwait(false);
                return;
            }

            var pkm = download.Data;
            var finalQR = Sprites.GetFullQR(pkm);
            var msg = $"Here's the QR for `{download.SanitizedFileName}`!";
            await Context.Channel.SendImageAsync(finalQR, msg).ConfigureAwait(false);
        }
    }
}