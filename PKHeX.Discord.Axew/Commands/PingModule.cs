using System.Threading.Tasks;
using Discord.Interactions;

namespace PKHeX.Discord.Axew;

[RequireContext(ContextType.Guild)]
public class HelloModule : SlashModuleBase
{
    [SlashCommand("hello", "Say hello to the bot and get a response.")]
    public Task HelloAsync() => RespondAsync($"Hello {Context.User.Mention}!");

    [SlashCommand("ping", "Makes the bot respond, indicating that it is running.")]
    public Task PingAsync() => RespondAsync("Pong!", ephemeral: true);
}
