using System.IO;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using PKHeX.Core;
using PKHeX.Core.AutoMod;

namespace PKHeX.Discord
{
    public class Legalizer : ModuleBase<SocketCommandContext>
    {
        private static readonly WebClient webClient = new WebClient();

        static Legalizer()
        {
            Task.Run(() => {
                var lang = Thread.CurrentThread.CurrentCulture.TwoLetterISOLanguageName.Substring(0, 2);
                Util.SetLocalization(typeof(LegalityCheckStrings), lang);
                Util.SetLocalization(typeof(MessageStrings), lang);
                RibbonStrings.ResetDictionary(GameInfo.Strings.ribbons);
            });

            // Update Legality Analysis strings
            LegalityAnalysis.MoveStrings = GameInfo.Strings.movelist;
            LegalityAnalysis.SpeciesStrings = GameInfo.Strings.specieslist;
        }

        [Command("legalize"), Alias("alm")]
        [Summary("Tries to legalize the attached pkm.")]
        public async Task LegalizeAsync()
        {
            var attachments = Context.Message.Attachments;
            foreach (var att in attachments)
                await Legalize(att).ConfigureAwait(false);
        }

        private async Task Legalize(IAttachment att)
        {
            var sanitized = $"`{att.Filename.Replace("`", "\\`")}`";
            if (!PKX.IsPKM(att.Size))
            {
                await ReplyAsync($"{sanitized}: Invalid size.").ConfigureAwait(false);
                return;
            }

            string url = att.Url;

            // Download the resource and load the bytes into a buffer.
            byte[] buffer = await webClient.DownloadDataTaskAsync(url).ConfigureAwait(false);
            var pkm = PKMConverter.GetPKMfromBytes(buffer, sanitized.Contains("pk6") ? 6 : 7);
            if (pkm == null)
            {
                await ReplyAsync($"{sanitized}: Invalid pkm attachment.").ConfigureAwait(false);
                return;
            }

            if (new LegalityAnalysis(pkm).Valid)
            {
                await ReplyAsync($"{sanitized}: Already legal.").ConfigureAwait(false);
                return;
            }

            var legal = pkm.Legalize();
            if (legal == null || !new LegalityAnalysis(legal).Valid)
            {
                await ReplyAsync($"{sanitized}: Unable to legalize.").ConfigureAwait(false);
                return;
            }

            legal.RefreshChecksum();

            var tmp = Path.Combine(Path.GetTempPath(), Util.CleanFileName(legal.FileName));
            File.WriteAllBytes(tmp, legal.DecryptedPartyData);
            var msg = $"Here's your legalized PKM for {sanitized}!";
            await Context.Channel.SendFileAsync(tmp, msg).ConfigureAwait(false);
            File.Delete(tmp);
        }
    }
}