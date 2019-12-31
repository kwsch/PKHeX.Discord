using System;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;

namespace PKHeX.Discord
{
    internal sealed class Program
    {
        private readonly DiscordSocketClient _client;

        // Keep the CommandService and DI container around for use with commands.
        // These two types require you install the Discord.Net.Commands package.
        private const char _prefix = '.';
        private readonly CommandService _commands;
        private readonly IServiceProvider _services;

        public static void Main(string[] args)
        {
            foreach (var line in args)
                Console.WriteLine(line);
            // Call the Program constructor, followed by the 
            // MainAsync method and wait until it finishes (which should be never).
            new Program().MainAsync().GetAwaiter().GetResult();
        }

        private Program()
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

            _commands = new CommandService(new CommandServiceConfig
            {
                // Again, log level:
                LogLevel = LogSeverity.Info,

                // There's a few more properties you can set,
                // for example, case-insensitive commands.
                CaseSensitiveCommands = false,
            });

            // Subscribe the logging handler to both the client and the CommandService.
            _client.Log += Log;
            _commands.Log += Log;

            // Setup your DI container.
            _services = ConfigureServices();
        }

        // If any services require the client, or the CommandService, or something else you keep on hand,
        // pass them as parameters into this method as needed.
        // If this method is getting pretty long, you can seperate it out into another file using partials.
        private static IServiceProvider ConfigureServices()
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

        private async Task MainAsync()
        {
            // Centralize the logic for commands into a separate method.
            await InitCommands().ConfigureAwait(false);

            // Login and connect.
            await _client.LoginAsync(TokenType.Bot, GetToken()).ConfigureAwait(false);
            await _client.StartAsync().ConfigureAwait(false);

            // Wait infinitely so your bot actually stays connected.
            await Task.Delay(Timeout.Infinite).ConfigureAwait(false);
        }

        private static string GetToken()
        {
            return File.ReadAllText("token.txt");
        }

        private async Task InitCommands()
        {
            // Either search the program and add all Module classes that can be found.
            // Module classes MUST be marked 'public' or they will be ignored.
            // You also need to pass your 'IServiceProvider' instance now,
            // so make sure that's done before you get here.
            await _commands.AddModulesAsync(Assembly.GetEntryAssembly(), _services).ConfigureAwait(false);
            // Or add Modules manually if you prefer to be a little more explicit:
            //await _commands.AddModuleAsync<SomeModule>(_services);
            // Note that the first one is 'Modules' (plural) and the second is 'Module' (singular).

            // Subscribe a handler to see if a message invokes a command.
            _client.MessageReceived += HandleMessageAsync;
        }

        private async Task HandleMessageAsync(SocketMessage arg)
        {
            // Bail out if it's a System Message.
            if (!(arg is SocketUserMessage msg))
                return;

            // We don't want the bot to respond to itself or other bots.
            if (msg.Author.Id == _client.CurrentUser.Id || msg.Author.IsBot)
                return;

            // Create a number to track where the prefix ends and the command begins
            int pos = 0;
            if (msg.HasCharPrefix(_prefix, ref pos))
            {
                bool handled = await TryHandleCommandAsync(msg, pos).ConfigureAwait(false);
                if (handled)
                    return;
            }

            await TryHandleMessageAsync(msg).ConfigureAwait(false);
        }

        private bool ConvertPKMToShowdownSet { get; } = true;

        private async Task TryHandleMessageAsync(SocketMessage msg)
        {
            // should this be a service?
            if (msg.Attachments.Count > 0 && ConvertPKMToShowdownSet)
            {
                foreach (var att in msg.Attachments)
                    await msg.Channel.RepostPKMAsShowdownAsync(att).ConfigureAwait(false);
            }
        }

        private async Task<bool> TryHandleCommandAsync(SocketUserMessage msg, int pos)
        {
            // Create a Command Context.
            var context = new SocketCommandContext(_client, msg);

            // Execute the command. (result does not indicate a return value, 
            // rather an object stating if the command executed successfully).
            await Log(new LogMessage(LogSeverity.Info, "Command", $"Executing command from {msg.Author.Username}. Content: {msg}")).ConfigureAwait(false);
            var result = await _commands.ExecuteAsync(context, pos, _services).ConfigureAwait(false);

            if (result.Error == CommandError.UnknownCommand)
                return false;

            // Uncomment the following lines if you want the bot
            // to send a message if it failed.
            // This does not catch errors from commands with 'RunMode.Async',
            // subscribe a handler for '_commands.CommandExecuted' to see those.
            if (!result.IsSuccess)
                await msg.Channel.SendMessageAsync(result.ErrorReason).ConfigureAwait(false);
            return true;
        }
    }
}
