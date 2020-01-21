using System.Threading.Tasks;
using Discord.Commands;

namespace PKHeX.Discord.Axew
{
    public class EvolveModule : ModuleBase<SocketCommandContext>
    {
        [Command("evolve"), Alias("evolve")]
        [Summary("Prints Evolution data for the species.")]
        public async Task PrintDataAsync([Summary("Generation")]int gen, [Summary("Species Index")]int species)
        {
            await ReplyAsync("Sorry, I don't handle this command yet!").ConfigureAwait(false);
        }
    }
}