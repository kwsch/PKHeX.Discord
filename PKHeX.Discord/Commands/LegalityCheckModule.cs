using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using PKHeX.Core;

namespace PKHeX.Discord
{
    public class LegalityCheckModule : ModuleBase<SocketCommandContext>
    {
        [Command("lc"), Alias("check", "validate", "verify")]
        [Summary("Verifies the attachment for legality.")]
        public async Task LegalityCheck()
        {
            var attachments = Context.Message.Attachments;
            foreach (var att in attachments)
                await LegalityCheck(att, false).ConfigureAwait(false);
        }

        [Command("lcv"), Alias("verbose")]
        [Summary("Verifies the attachment for legality with a verbose output.")]
        public async Task LegalityCheckVerbose()
        {
            var attachments = Context.Message.Attachments;
            foreach (var att in attachments)
                await LegalityCheck(att, true).ConfigureAwait(false);
        }

        private async Task LegalityCheck(IAttachment att, bool verbose)
        {
            var sanitized = $"`{att.Filename.Replace("`", "\\`")}`";
            if (!PKX.IsPKM(att.Size))
            {
                await ReplyAsync($"{sanitized}: Invalid size.").ConfigureAwait(false);
                return;
            }

            string url = att.Url;

            // Download the resource and load the bytes into a buffer.
            byte[] data = await NetUtil.DownloadFromUrlAsync(url).ConfigureAwait(false);
            var pkm = PKMConverter.GetPKMfromBytes(data, sanitized.Contains("pk6") ? 6 : 7);
            if (pkm == null)
            {
                await ReplyAsync($"{sanitized}: Invalid pkm attachment.").ConfigureAwait(false);
                return;
            }

            var la = new LegalityAnalysis(pkm);
            var builder = new EmbedBuilder
            {
                Color = la.Valid ? Color.Green : Color.Red,
                Description = $"Legality Report for {sanitized}:"
            };

            builder.AddField(x =>
            {
                x.Name = la.Valid ? "Valid" : "Invalid";
                x.Value = la.Report(verbose);
                x.IsInline = false;
            });

            await ReplyAsync("Here's the legality report!", false, builder.Build()).ConfigureAwait(false);
        }
    }
}