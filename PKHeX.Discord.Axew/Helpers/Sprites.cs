using System;
using System.Drawing;
using PKHeX.Core;
using PKHeX.Drawing.Misc;
using PKHeX.Drawing.PokeSprite;

namespace PKHeX.Discord.Axew
{
    public static class Sprites
    {
        private static readonly Font font = new("Microsoft Sans Serif", 8.25f);

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
            return pkm.Sprite();
        }
    }
}