using System.Threading;
using System.Threading.Tasks;

using Discord;
using Discord.Commands;
using PKHeX.Core;
using PKHeX.Core.AutoMod;

namespace PKHeX.Discord.Axew
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

            // Seed the Trainer Database with enough fake save files so that we return a generation sensitive format when needed.
            for (int i = 1; i < PKX.Generation; i++)
            {
                const string OT = "PKHeX-D";
                var blankSAV = SaveUtil.GetBlankSAV(i, OT);
                TrainerSettings.Register(blankSAV);
            }

            PKMConverter.Trainer = TrainerSettings.GetSavedTrainerData(7);

            // Legalizer.AllowBruteForce = false;

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
        [Priority(1)]
        public async Task ConvertShowdown([Summary("Generation/Format")]int gen, [Remainder][Summary("Showdown Set")]string content)
        {
            content = ReusableActions.StripCodeBlock(content);
            var set = new ShowdownSet(content);
            var sav = TrainerSettings.GetSavedTrainerData(gen);
            await GenerateFromSet(sav, set).ConfigureAwait(false);
        }

        [Command("convert"), Alias("showdown")]
        [Summary("Tries to convert the Showdown Set to pkm data.")]
        [Priority(0)]
        public async Task ConvertShowdown([Remainder][Summary("Showdown Set")]string content)
        {
            content = ReusableActions.StripCodeBlock(content);
            var set = new ShowdownSet(content);
            var sav = TrainerSettings.GetSavedTrainerData(set.Format);
            await GenerateFromSet(sav, set).ConfigureAwait(false);
        }

        private async Task GenerateFromSet(ITrainerInfo sav, ShowdownSet set)
        {
            if (set.Species <= 0)
            {
                await ReplyAsync("Oops! I wasn't able to interpret your message! If you intended to convert something, please double check what you're pasting!").ConfigureAwait(false);
                return;
            }
            var pkm = sav.GetLegalFromSet(set, out var result);
            var la = new LegalityAnalysis(pkm);
            var spec = GameInfo.Strings.Species[set.Species];
            var msg = la.Valid
                ? $"Here's your ({result}) legalized PKM for {spec}!"
                : $"Oops! I wasn't able to create something from that. Here's my best attempt for that {spec}!";
            await Context.Channel.SendPKMAsync(pkm, msg + $"\n{ReusableActions.GetFormattedShowdownText(pkm)}").ConfigureAwait(false);
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