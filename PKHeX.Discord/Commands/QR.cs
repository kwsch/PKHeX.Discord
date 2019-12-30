using System;
using System.Drawing;
using System.Net;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using PKHeX.Core;
using PKHeX.Drawing;
using Image = System.Drawing.Image;
using ImageFormat = System.Drawing.Imaging.ImageFormat;

namespace PKHeX.Discord
{
    public class QR : ModuleBase<SocketCommandContext>
    {
        private static readonly WebClient webClient = new WebClient();
        private static readonly Font font = new Font("Microsoft Sans Serif", 8.25f);

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
            if (!PKX.IsPKM(att.Size))
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

            var finalQR = GetQR(pkm);
            const string fn = "tmp.png";
            finalQR.Save(fn, ImageFormat.Png);
            var msg = $"Here's the QR for `{att.Filename.Replace("`", "\\`")}`!";
            await Context.Channel.SendFileAsync(fn, msg).ConfigureAwait(false);
        }

        private static Bitmap GetQR(PKM pkm)
        {
            var icon = GetSprite(pkm);
            var qr = pkm is PK7 pk7 ? QREncode.GenerateQRCode7(pk7) : QREncode.GenerateQRCode(pkm);
            var lines = pkm.GetQRLines();
            var tag = $"PKHeX Discord - {DateTime.Now:yy/MM/dd} ({pkm.GetType().Name})";
            return QRImageUtil.GetQRImageExtended(font, qr, icon, Math.Max(qr.Width, 370), qr.Height + 56, lines, tag);
        }

        private static Image GetSprite(PKM pkm)
        {
            SpriteUtil.Initialize(pkm.Format >= 8 || pkm is PB7);
            return pkm.Sprite();
        }
    }
}