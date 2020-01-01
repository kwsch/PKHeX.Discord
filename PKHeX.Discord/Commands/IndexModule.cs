using System.Threading.Tasks;
using Discord.Commands;
using PKHeX.Core;

namespace PKHeX.Discord
{
    public class IndexModule : ModuleBase<SocketCommandContext>
    {
        [Command("move")]
        [Summary("Gets the name of the requested move integer.")]
        public async Task GetMoveAsync([Summary("Move Index")]int move)
        {
            var str = GameInfo.Strings.Move;
            if ((uint)move >= str.Count)
                await ReplyAsync($"Move {move} is out of range.").ConfigureAwait(false);
            else
                await ReplyAsync(str[move]).ConfigureAwait(false);
        }

        [Command("item")]
        [Summary("Gets the name of the requested item integer.")]
        public async Task GetItemAsync([Summary("Item Index")]int item)
        {
            var str = GameInfo.Strings.Item;
            if ((uint)item >= str.Count)
                await ReplyAsync($"Item {item} is out of range.").ConfigureAwait(false);
            else
                await ReplyAsync(str[item]).ConfigureAwait(false);
        }

        [Command("species")]
        [Summary("Gets the name of the requested species integer.")]
        public async Task GetSpeciesAsync([Summary("Species Index")]int species)
        {
            var str = GameInfo.Strings.Species;
            if ((uint)species >= str.Count)
                await ReplyAsync($"Species {species} is out of range.").ConfigureAwait(false);
            else
                await ReplyAsync(str[species]).ConfigureAwait(false);
        }

        [Command("ability")]
        [Summary("Gets the name of the requested species integer.")]
        public async Task GetAbilityAsync([Summary("Ability Index")]int ability)
        {
            var str = GameInfo.Strings.Ability;
            if ((uint)ability >= str.Count)
                await ReplyAsync($"Ability {ability} is out of range.").ConfigureAwait(false);
            else
                await ReplyAsync(str[ability]).ConfigureAwait(false);
        }
    }
}