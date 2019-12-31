using System.Threading;
using System.Threading.Tasks;

using Discord;
using Discord.Commands;
using PKHeX.Core;
using PKHeX.Core.AutoMod;

namespace PKHeX.Discord
{
    public class LegalizerModule : ModuleBase<SocketCommandContext>
    {
        static LegalizerModule()
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
        [Summary("Tries to legalize the attached pkm data.")]
        public async Task LegalizeAsync()
        {
            var attachments = Context.Message.Attachments;
            foreach (var att in attachments)
                await Legalize(att).ConfigureAwait(false);
        }

        private async Task Legalize(IAttachment att)
        {
            var download = await NetUtil.DownloadPKMAsync(att).ConfigureAwait(false);
            if (!download.Success)
            {
                await ReplyAsync(download.ErrorMessage).ConfigureAwait(false);
                return;
            }

            var pkm = download.Data;
            if (new LegalityAnalysis(pkm).Valid)
            {
                await ReplyAsync($"{download.SanitizedFileName}: Already legal.").ConfigureAwait(false);
                return;
            }

            var legal = pkm.Legalize();
            if (legal == null || !new LegalityAnalysis(legal).Valid)
            {
                await ReplyAsync($"{download.SanitizedFileName}: Unable to legalize.").ConfigureAwait(false);
                return;
            }

            legal.RefreshChecksum();

            var msg = $"Here's your legalized PKM for {download.SanitizedFileName}!\n{ReusableActions.GetFormattedShowdownText(legal)}";
            await Context.Channel.SendPKMAsync(legal, msg).ConfigureAwait(false);
        }
    }
}