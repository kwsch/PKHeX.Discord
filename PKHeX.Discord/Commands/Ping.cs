using System.Threading.Tasks;
using Discord.Commands;

namespace PKHeX.Discord
{
    public class Ping : ModuleBase<SocketCommandContext>
    {
        [Command("ping")]
        public async Task PingAsync()
        {
            await ReplyAsync("Pong!").ConfigureAwait(false);
        }
    }
}