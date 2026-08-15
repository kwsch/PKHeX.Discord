using System;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;

namespace PKHeX.Discord.Axew;

public sealed class AxewBot
{
    private readonly DiscordSocketClient _client;

    // Keep the CommandService and DI container around for use with commands.
    // These two types require you install the Discord.Net.Commands package.
    private readonly InteractionService _interactions;
    private readonly IServiceProvider _services;
    public static IServiceProvider Provider { get; private set; } = null!; // late-bound via singleton ctor

    // Bot listens to channel messages to reply with a ShowdownSet whenever a PKM file is attached (not with a command).
    public bool ConvertAttachedEntityToShowdownSet { get; set; } = true;

    public AxewBot()
    {
        _client = new DiscordSocketClient(new DiscordSocketConfig
        {
            // How much logging do you want to see?
            LogLevel = LogSeverity.Info,

            // If you or another service needs to do anything with messages
            // (eg. checking Reactions, checking the content of edited/deleted messages),
            // you must set the MessageCacheSize. You may adjust the number as needed.
            //MessageCacheSize = 50,
        });

        _interactions = new InteractionService(_client.Rest, new InteractionServiceConfig
        {
            LogLevel = LogSeverity.Info,
            DefaultRunMode = RunMode.Async,
        });

        // Subscribe the logging handler to both the client and the CommandService.
        _client.Log += Log;
        _interactions.Log += Log;

        // Setup your DI container.
        Provider = _services = ConfigureServices();
    }

    // If any services require the client, or the CommandService, or something else you keep on hand,
    // pass them as parameters into this method as needed.
    // If this method is getting pretty long, you can seperate it out into another file using partials.
    private static ServiceProvider ConfigureServices()
    {
        var map = new ServiceCollection();//.AddSingleton(new SomeServiceClass());

        // When all your required services are in the collection, build the container.
        // Tip: There's an overload taking in a 'validateScopes' bool to make sure
        // you haven't made any mistakes in your dependency graph.
        return map.BuildServiceProvider();
    }

    // Example of a logging handler. This can be re-used by addons
    // that ask for a Func<LogMessage, Task>.
    private static Task Log(LogMessage msg)
    {
        Console.ForegroundColor = msg.Severity switch
        {
            LogSeverity.Critical => ConsoleColor.Red,
            LogSeverity.Error => ConsoleColor.Red,

            LogSeverity.Warning => ConsoleColor.Yellow,
            LogSeverity.Info => ConsoleColor.White,

            LogSeverity.Verbose => ConsoleColor.DarkGray,
            LogSeverity.Debug => ConsoleColor.DarkGray,
            _ => Console.ForegroundColor
        };

        Console.WriteLine($"{DateTime.Now,-19} [{msg.Severity,8}] {msg.Source}: {msg.Message} {msg.Exception}");
        Console.ResetColor();

        return Task.CompletedTask;
    }

    public async Task MainAsync(string token)
    {
        // Centralize the logic for commands into a separate method.
        await InitCommands().ConfigureAwait(false);

        // Login and connect.
        await _client.LoginAsync(TokenType.Bot, token).ConfigureAwait(false);
        await _client.StartAsync().ConfigureAwait(false);

        // Wait infinitely so your bot actually stays connected.
        await Task.Delay(Timeout.Infinite).ConfigureAwait(false);
    }

    private async Task InitCommands()
    {
        var assembly = Assembly.GetExecutingAssembly();
        await LoadModulesFromAssembly(assembly).ConfigureAwait(false);

        _client.InteractionCreated += HandleInteractionAsync;

        // Subscribe a handler to see if a message invokes a command.
        _client.MessageReceived += HandleMessageAsync;
    }

    private async Task HandleMessageAsync(SocketMessage arg)
    {
        // Bail out if it's a System Message.
        if (arg is not SocketUserMessage msg)
            return;

        // We don't want the bot to respond to itself or other bots.
        if (msg.Author.Id == _client.CurrentUser.Id || msg.Author.IsBot)
            return;

        await TryHandleMessageAsync(msg).ConfigureAwait(false);
    }

    private async Task LoadModulesFromAssembly(Assembly assembly)
    {
        var moduleTypes = assembly.DefinedTypes
            .Where(z => z is { IsAbstract: false, IsGenericTypeDefinition: false } && typeof(InteractionModuleBase<SocketInteractionContext>).IsAssignableFrom(z.AsType()))
            .Select(z => z.AsType());

        foreach (var module in moduleTypes)
        {
            await Log(GetLog(LogSeverity.Info, $"Adding module: {ReusableActions.GetModuleName(module.Name)}")).ConfigureAwait(false);
            await _interactions.AddModuleAsync(module, _services).ConfigureAwait(false);
        }

        await Log(GetLog(LogSeverity.Info, "Module load complete.")).ConfigureAwait(false);
    }

    private async Task TryHandleMessageAsync(SocketMessage msg)
    {
        // should this be a service?
        if (msg.Attachments.Count > 0 && ConvertAttachedEntityToShowdownSet)
        {
            foreach (var att in msg.Attachments)
                await msg.Channel.InterpretAttachmentAsSetAndReply(att).ConfigureAwait(false);
        }
    }

    private async Task HandleInteractionAsync(SocketInteraction interaction)
    {
        var context = new SocketInteractionContext(_client, interaction);
        if (context.Interaction.ChannelId is not { } channel)
        {
            await interaction.RespondAsync("You can't use that here.", ephemeral: true).ConfigureAwait(false);
            return;
        }

        await LogInteractionStart(context, channel).ConfigureAwait(false);

        var result = await _interactions.ExecuteCommandAsync(context, _services).ConfigureAwait(false);
        if (!result.IsSuccess && !interaction.HasResponded)
            await RespondErrorAsync(interaction, result.ErrorReason).ConfigureAwait(false);
    }

    private static Task RespondErrorAsync(SocketInteraction interaction, string message, bool ephemeral = true) => interaction.HasResponded
        ? interaction.FollowupAsync(message, ephemeral: ephemeral)
        : interaction.RespondAsync(message, ephemeral: ephemeral);

    private static async Task LogInteractionStart(SocketInteractionContext context, ulong channelId)
    {
        var interaction = context.Interaction;
        var (type, identity) = interaction switch
        {
            SocketSlashCommand cmd => ("slash command", $"Command: {cmd.CommandName}"),
            SocketModal modal => ("modal", $"Modal: {modal.Id}"),
            SocketMessageComponent c => ("component", $"Component: {c.Id}"),
            _ => (interaction.Type.ToString(), "Unknown"),
        };
        var channel = interaction.Channel?.Name ?? $"Unknown Channel: {channelId}";
        var location = interaction.IsDMInteraction ? "Direct Messages" : context.Guild?.Name ?? "Unknown Guild";
        var status = $"Executing {type} from {location}:{channel}:@{context.User.Username}. {identity}";

        await Log(GetLog(LogSeverity.Info, status)).ConfigureAwait(false);
    }

    private static LogMessage GetLog(LogSeverity severity, string message, [CallerMemberName] string identity = "")
        => new(severity, identity, message);
}