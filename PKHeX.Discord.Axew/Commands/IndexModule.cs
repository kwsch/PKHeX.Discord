using System.Threading.Tasks;
using Discord.Interactions;
using PKHeX.Core;

namespace PKHeX.Discord.Axew;

[Group("index", "Fetches information about specific Pokémon attributes by index.")]
public class IndexModule : SlashModuleBase
{
    [SlashCommand("move", "Gets information about a move.")]
    public async Task MoveAsync(
        [Summary("index", "Move Index")] int index)
    {
        var str = GameInfo.Strings.Move;
        if ((uint)index >= str.Count)
        {
            await RespondAsync($"Move {index} is out of range.").ConfigureAwait(false);
            return;
        }
        await RespondAsync(str[index]).ConfigureAwait(false);
    }

    [SlashCommand("move-name", "Gets the index of a move by name.")]
    public async Task MoveNameAsync(
        [Summary("name", "Move Name")] string name)
    {
        var str = GameInfo.Strings.movelist;
        int index = StringUtil.FindIndexIgnoreCase(str, name);
        await RespondAsync(index.ToString()).ConfigureAwait(false);
    }

    [SlashCommand("item", "Gets information about an item.")]
    public async Task ItemAsync(
        [Summary("index", "Item Index")] int index)
    {
        var str = GameInfo.Strings.Item;
        if ((uint)index >= str.Count)
        {
            await RespondAsync($"Item {index} is out of range.").ConfigureAwait(false);
            return;
        }
        await RespondAsync(str[index]).ConfigureAwait(false);
    }

    [SlashCommand("item-name", "Gets the index of an item by name.")]
    public async Task ItemNameAsync(
        [Summary("name", "Item Name")] string name)
    {
        var str = GameInfo.Strings.itemlist;
        int index = StringUtil.FindIndexIgnoreCase(str, name);
        await RespondAsync(index.ToString()).ConfigureAwait(false);
    }

    [SlashCommand("species", "Gets information about a species.")]
    public async Task SpeciesAsync(
        [Summary("index", "Species Index")] int index)
    {
        var str = GameInfo.Strings.Species;
        if ((uint)index >= str.Count)
        {
            await RespondAsync($"Species {index} is out of range.").ConfigureAwait(false);
            return;
        }
        await RespondAsync(str[index]).ConfigureAwait(false);
    }

    [SlashCommand("species-name", "Gets the index of a species by name.")]
    public async Task SpeciesNameAsync(
        [Summary("name", "Species Name")] string name)
    {
        var str = GameInfo.Strings.specieslist;
        int index = StringUtil.FindIndexIgnoreCase(str, name);
        await RespondAsync(index.ToString()).ConfigureAwait(false);
    }

    [SlashCommand("ability", "Gets information about an ability.")]
    public async Task AbilityAsync(
        [Summary("index", "Ability Index")] int index)
    {
        var str = GameInfo.Strings.Ability;
        if ((uint)index >= str.Count)
        {
            await RespondAsync($"Ability {index} is out of range.").ConfigureAwait(false);
            return;
        }
        await RespondAsync(str[index]).ConfigureAwait(false);
    }

    [SlashCommand("ability-name", "Gets the index of an ability by name.")]
    public async Task AbilityNameAsync(
        [Summary("name", "Ability Name")] string name)
    {
        var str = GameInfo.Strings.abilitylist;
        int index = StringUtil.FindIndexIgnoreCase(str, name);
        await RespondAsync(index.ToString()).ConfigureAwait(false);
    }
}