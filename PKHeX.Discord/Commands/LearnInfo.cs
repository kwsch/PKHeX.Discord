using System;
using System.Linq;
using System.Threading.Tasks;
using Discord.Commands;
using PKHeX.Core;

namespace PKHeX.Discord
{
    public class LearnInfo : ModuleBase<SocketCommandContext>
    {
        [Command("learn"), Alias("canlearn")]
        [Summary("Checks if the pkm can learn all of the moves asked.")]
        public async Task LearnAsync([Remainder]string context)
        {
            var args = context.Split(", ");
            var species = args[0];
            var summary = EncounterLearn.CanLearn(species, args.Skip(1));
            var msg = summary
                ? $"Yep! {species} can learn {string.Join(", ", args.Skip(1))}."
                : $"Nope, {species} can't' learn {string.Join(", ", args.Skip(1))}";
            await ReplyAsync(msg).ConfigureAwait(false);
        }

        [Command("encounter"), Alias("find")]
        [Summary("Returns a list of encounter locations where a pkm can be found, to learn all of the moves asked.")]
        public async Task EncounterAsync([Remainder]string context)
        {
            var args = context.Split(", ");
            var species = args[0];
            var summary = EncounterLearn.GetLearnSummary(species, args.Skip(1));
            var msg = string.Join(Environment.NewLine, summary);
            await ReplyAsync(msg).ConfigureAwait(false);
        }
    }
}