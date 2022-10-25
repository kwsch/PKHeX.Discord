using System.Threading;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using PKHeX.Core;
using PKHeX.Core.AutoMod;

namespace PKHeX.Discord.Axew
{
    public static class AutoLegalityExtensions
    {
        static AutoLegalityExtensions()
        {
            Task.Run(() => {
                var lang = Thread.CurrentThread.CurrentCulture.TwoLetterISOLanguageName[..2];
                LocalizationUtil.SetLocalization(typeof(LegalityCheckStrings), lang);
                LocalizationUtil.SetLocalization(typeof(MessageStrings), lang);
                RibbonStrings.ResetDictionary(GameInfo.Strings.ribbons);
            });

            // Seed the Trainer Database with enough fake save files so that we return a generation sensitive format when needed.
            foreach (var game in GameUtil.GameVersions)
            {
                const string OT = "PKHeX-D";
                var blankSAV = SaveUtil.GetBlankSAV(game, OT);
                TrainerSettings.Register(blankSAV);
            }

            var trainer = TrainerSettings.GetSavedTrainerData(7);
            RecentTrainerCache.SetRecentTrainer(trainer);

            // Legalizer.AllowBruteForce = false;

            // Update Legality Analysis strings
            ParseSettings.ChangeLocalizationStrings(GameInfo.Strings.movelist, GameInfo.Strings.specieslist);
        }

        public static async Task ReplyWithLegalizedSetAsync(this ISocketMessageChannel channel, ITrainerInfo sav, ShowdownSet set)
        {
            if (set.Species <= 0)
            {
                await channel.SendMessageAsync("Oops! I wasn't able to interpret your message! If you intended to convert something, please double check what you're pasting!").ConfigureAwait(false);
                return;
            }
            var pkm = sav.GetLegalFromSet(set, out var result);
            var la = new LegalityAnalysis(pkm);
            var spec = GameInfo.Strings.Species[set.Species];
            var msg = la.Valid
                ? $"Here's your ({result}) legalized PKM for {spec} ({la.EncounterOriginal.Name})!"
                : $"Oops! I wasn't able to create something from that. Here's my best attempt for that {spec}!";
            await channel.SendPKMAsync(pkm, msg + $"\n{ReusableActions.GetFormattedShowdownText(pkm)}").ConfigureAwait(false);
        }

        public static async Task ReplyWithLegalizedSetAsync(this ISocketMessageChannel channel, string content, int gen)
        {
            content = ReusableActions.StripCodeBlock(content);
            var set = new ShowdownSet(content);
            var sav = TrainerSettings.GetSavedTrainerData(gen);
            await channel.ReplyWithLegalizedSetAsync(sav, set).ConfigureAwait(false);
        }

        public static async Task ReplyWithLegalizedSetAsync(this ISocketMessageChannel channel, string content)
        {
            content = ReusableActions.StripCodeBlock(content);
            var set = new ShowdownSet(content);
            var sav = TrainerSettings.GetSavedTrainerData(PKX.Generation);
            await channel.ReplyWithLegalizedSetAsync(sav, set).ConfigureAwait(false);
        }

        public static async Task ReplyWithLegalizedSetAsync(this ISocketMessageChannel channel, IAttachment att)
        {
            var download = await NetUtil.DownloadPKMAsync(att).ConfigureAwait(false);
            if (!download.Success)
            {
                await channel.SendMessageAsync(download.ErrorMessage).ConfigureAwait(false);
                return;
            }

            var pkm = download.Data;
            if (new LegalityAnalysis(pkm).Valid)
            {
                await channel.SendMessageAsync($"{download.SanitizedFileName}: Already legal.").ConfigureAwait(false);
                return;
            }

            var legal = pkm.Legalize();
            if (!new LegalityAnalysis(legal).Valid)
            {
                await channel.SendMessageAsync($"{download.SanitizedFileName}: Unable to legalize.").ConfigureAwait(false);
                return;
            }

            legal.RefreshChecksum();

            var msg = $"Here's your legalized PKM for {download.SanitizedFileName}!\n{ReusableActions.GetFormattedShowdownText(legal)}";
            await channel.SendPKMAsync(legal, msg).ConfigureAwait(false);
        }
    }
}