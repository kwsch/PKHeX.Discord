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

        [Command("convert"), Alias("showdown")]
        [Summary("Tries to convert the Showdown Set to pkm data.")]
        public async Task ConvertShowdown([Remainder][Summary("Showdown Set")]string content)
        {
            content = ReusableActions.StripCodeBlock(content);
            var set = new ShowdownSet(content);
            if (set.Species <= 0)
            {
                await ReplyAsync("Oops! I wasn't able to interpret your message! If you intended to convert something, please double check what you're pasting!").ConfigureAwait(false);
                return;
            }

            var sav = TrainerSettings.GetSavedTrainerData(set.Format);
            var legal = sav.GetLegalFromSet(set, out var result);
            if (new LegalityAnalysis(legal).Valid)
            {
                var msg = $"Here's your ({result}) legalized PKM for {GameInfo.Strings.Species[set.Species]}!\n{ReusableActions.GetFormattedShowdownText(legal)}";
                await Context.Channel.SendPKMAsync(legal, msg).ConfigureAwait(false);
            }
            else // Invalid
            {
                var msg = $"Oops! I wasn't able to create something from that. Here's my best attempt for that {GameInfo.Strings.Species[set.Species]}!\n{ReusableActions.GetFormattedShowdownText(legal)}";
                await Context.Channel.SendPKMAsync(legal, msg).ConfigureAwait(false);
            }
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