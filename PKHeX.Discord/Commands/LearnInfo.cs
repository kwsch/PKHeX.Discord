using System;
using System.Linq;
using System.Threading.Tasks;
using Discord.Commands;
using PKHeX.Core;

namespace PKHeX.Discord
{
    public class LearnInfo : ModuleBase<SocketCommandContext>
    {
        [Command("learn")]
        public async Task LearnAsync([Remainder]string context)
        {
            var args = context.Split(", ");
            var species = args[0];
            var summary = EncounterLearn.GetLearnSummary(species, args.Skip(1));
            var msg = string.Join(Environment.NewLine, summary);
            await ReplyAsync(msg).ConfigureAwait(false);
        }
    }
}