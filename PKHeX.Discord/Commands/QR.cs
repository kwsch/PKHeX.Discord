using System;
using System.Drawing;
using System.Net;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using PKHeX.Core;
using PKHeX.Drawing;
using ImageFormat = System.Drawing.Imaging.ImageFormat;

namespace PKHeX.Discord
{
    public class QR : ModuleBase<SocketCommandContext>
    {
        private static readonly WebClient webClient = new WebClient();
        private static readonly Font font = new Font("Microsoft Sans Serif", 8.25f);

        [Command("qr")]
        [Summary("Converts the pkm to QR.")]
        public async Task LegalityCheckVerbose()
        {
            var attachments = Context.Message.Attachments;
            foreach (var att in attachments)
                await QRPKM(att).ConfigureAwait(false);
        }

        private async Task QRPKM(IAttachment att)
        {
            if (att.Size > 0x158)
            {
                await ReplyAsync($"{att.Filename}: Invalid size.").ConfigureAwait(false);
                return;
            }

            string url = att.Url;

            // Download the resource and load the bytes into a buffer.
            byte[] buffer = await webClient.DownloadDataTaskAsync(url).ConfigureAwait(false);
            var pkm = PKMConverter.GetPKMfromBytes(buffer);
            if (pkm == null)
            {
                await ReplyAsync($"{att.Filename}: Invalid pkm attachment.").ConfigureAwait(false);
                return;
            }

            string[] r = pkm.GetQRLines();
            SpriteUtil.Initialize(pkm.Format >= 8 || pkm is PB7);
            var icon = pkm.Sprite();
            var qr = QREncode.GenerateQRCode(pkm);
            var tag = $"PKHeX Discord - {DateTime.Now:yy/MM/dd} ({pkm.GetType().Name})";
            var finalQR = QRImageUtil.GetQRImageExtended(font, qr, icon, Math.Max(qr.Width, 370), qr.Height + 50, r, tag);
            const string fn = "tmp.png";
            finalQR.Save(fn, ImageFormat.Png);
            await Context.Channel.SendFileAsync(fn, "Here's the QR for `" + att.Filename.Replace("`", "\\`") + "`!").ConfigureAwait(false);
        }
    }
}