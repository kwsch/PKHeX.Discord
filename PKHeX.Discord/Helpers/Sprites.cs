using System;
using System.Drawing;
using PKHeX.Core;
using PKHeX.Drawing;

namespace PKHeX.Discord
{
    public static class Sprites
    {
        private static readonly Font font = new Font("Microsoft Sans Serif", 8.25f);

        public static Bitmap GetFullQR(PKM pkm)
        {
            var icon = GetSprite(pkm);
            var qr = pkm is PK7 pk7 ? QREncode.GenerateQRCode7(pk7) : QREncode.GenerateQRCode(pkm);
            var lines = pkm.GetQRLines();
            var tag = $"PKHeX Discord - {DateTime.Now:yy/MM/dd} ({pkm.GetType().Name})";
            return QRImageUtil.GetQRImageExtended(font, qr, icon, Math.Max(qr.Width, 370), qr.Height + 56, lines, tag);
        }

        public static Image GetSprite(PKM pkm)
        {
            SpriteUtil.Initialize(pkm.Format >= 8 || pkm is PB7);
            return pkm.Sprite();
        }
    }
}