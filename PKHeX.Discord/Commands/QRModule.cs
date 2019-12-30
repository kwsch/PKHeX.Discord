using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using PKHeX.Core;

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
            var sanitized = $"`{att.Filename.Replace("`", "\\`")}`";
            if (!PKX.IsPKM(att.Size))
            {
                await ReplyAsync($"{att.Filename}: Invalid size.").ConfigureAwait(false);
                return;
            }

            string url = att.Url;

            byte[] data = await NetUtil.DownloadFromUrlAsync(url).ConfigureAwait(false);
            var pkm = PKMConverter.GetPKMfromBytes(data, sanitized.Contains("pk6") ? 6 : 7);
            if (pkm == null)
            {
                await ReplyAsync($"{sanitized}: Invalid pkm attachment.").ConfigureAwait(false);
                return;
            }

            var channel = Context.Channel;
            var finalQR = Sprites.GetFullQR(pkm);
            var msg = $"Here's the QR for `{sanitized.Replace("`", "\\`")}`!";
            await ReusableActions.SendImageToChannelAsync(channel, finalQR, msg).ConfigureAwait(false);
        }
    }
}