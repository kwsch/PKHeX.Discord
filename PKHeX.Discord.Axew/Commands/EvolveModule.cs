using System.Threading.Tasks;
using Discord.Interactions;

namespace PKHeX.Discord.Axew;

public class EvolveModule : SlashModuleBase
{
    [SlashCommand("evolve", "Prints Evolution data for the species.")]
    public async Task PrintDataAsync(
        [Summary("gen", "Generation")] int gen,
        [Summary("species", "Species Index")] int species)
    {
        await RespondAsync("Sorry, I don't handle this command yet!").ConfigureAwait(false);
    }
}