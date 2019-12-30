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
            var sanitized = $"`{att.Filename.Replace("`", "\\`")}`";
            if (!PKX.IsPKM(att.Size))
            {
                await ReplyAsync($"{sanitized}: Invalid size.").ConfigureAwait(false);
                return;
            }

            string url = att.Url;

            // Download the resource and load the bytes into a buffer.
            var buffer = await NetUtil.DownloadFromUrlAsync(url).ConfigureAwait(false);
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

            var msg = $"Here's your legalized PKM for {sanitized}!";
            var channel = Context.Channel;
            await ReusableActions.SendPKMToChannelAsync(channel, legal, msg).ConfigureAwait(false);
        }
    }
}