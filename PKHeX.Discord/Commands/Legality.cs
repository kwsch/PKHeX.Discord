using System.Net;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using PKHeX.Core;

namespace PKHeX.Discord
{
    public class Legality : ModuleBase<SocketCommandContext>
    {
        private static readonly WebClient webClient = new WebClient();

        [Command("lc")]
        [Summary("Verifies the attachment for legality.")]
        public async Task LegalityCheck()
        {
            var attachments = Context.Message.Attachments;
            foreach (var att in attachments)
                await LegalityCheck(att, false).ConfigureAwait(false);
        }

        [Command("lcv")]
        [Summary("Verifies the attachment for legality with a verbose output.")]
        public async Task LegalityCheckVerbose()
        {
            var attachments = Context.Message.Attachments;
            foreach (var att in attachments)
                await LegalityCheck(att, true).ConfigureAwait(false);
        }

        private async Task LegalityCheck(IAttachment att, bool verbose)
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

            var la = new LegalityAnalysis(pkm);
            await ReplyAsync($"Legality Report for {att.Filename}:").ConfigureAwait(false);
            await ReplyAsync($"{la.Report(verbose)}").ConfigureAwait(false);
        }
    }
}