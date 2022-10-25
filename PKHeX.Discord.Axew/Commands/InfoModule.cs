﻿using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;

namespace PKHeX.Discord.Axew
{
    // src: https://github.com/foxbot/patek/blob/master/src/Patek/Modules/InfoModule.cs
    // ISC License (ISC)
    // Copyright 2017, Christopher F. <foxbot@protonmail.com>
    public class InfoModule : ModuleBase<SocketCommandContext>
    {
        private const string detail = "I am an open source Discord bot powered by PKHeX.Core and other open source software.";
        private const string repo = "https://github.com/kwsch/PKHeX.Discord";

        [Command("info")]
        [Alias("about", "whoami", "owner")]
        public async Task InfoAsync()
        {
            var app = await Context.Client.GetApplicationInfoAsync().ConfigureAwait(false);

            var builder = new EmbedBuilder
            {
                Color = new Color(114, 137, 218),
                Description = detail,
            };

            var invite = $"https://discordapp.com/oauth2/authorize?client_id={app.Id}&permissions=0&scope=bot";
            builder.AddField("Info",
                $"- [Source Code]({repo}) | [Invite URL]({invite})\n" +
                $"- {Format.Bold("Author")}: {app.Owner} ({app.Owner.Id})\n" +
                $"- {Format.Bold("Library")}: Discord.Net ({DiscordConfig.Version})\n" +
                $"- {Format.Bold("Uptime")}: {GetUptime()}\n" +
                $"- {Format.Bold("Runtime")}: {RuntimeInformation.FrameworkDescription} {RuntimeInformation.ProcessArchitecture} " +
                $"({RuntimeInformation.OSDescription} {RuntimeInformation.OSArchitecture})\n" +
                $"- {Format.Bold("Buildtime")}: {GetBuildTime()}\n" +
                $"- {Format.Bold("Core")}: {GetCoreDate()}\n" +
                $"- {Format.Bold("AutoLegality")}: {GetALMDate()}\n"
                );

            builder.AddField("Stats",
                $"- {Format.Bold("Heap Size")}: {GetHeapSize()}MiB\n" +
                $"- {Format.Bold("Guilds")}: {Context.Client.Guilds.Count}\n" +
                $"- {Format.Bold("Channels")}: {Context.Client.Guilds.Sum(g => g.Channels.Count)}\n" +
                $"- {Format.Bold("Users")}: {Context.Client.Guilds.Sum(g => g.Users.Count)}\n"
                );

            await ReplyAsync("Here's a bit about me!", embed:builder.Build()).ConfigureAwait(false);
        }

        private static string GetUptime() => (DateTime.Now - Process.GetCurrentProcess().StartTime).ToString(@"dd\.hh\:mm\:ss");
        private static string GetHeapSize() => Math.Round(GC.GetTotalMemory(true) / (1024.0 * 1024.0), 2).ToString();

        private static string GetBuildTime()
        {
            var assembly = Assembly.GetExecutingAssembly();
            return File.GetLastWriteTime(assembly.Location).ToString(@"yy-MM-dd\.hh\:mm");
        }

        public static string GetCoreDate() => GetDateOfDll("PKHeX.Core.dll");
        public static string GetALMDate() => GetDateOfDll("PKHeX.Core.AutoMod.dll");

        private static string GetDateOfDll(string dll)
        {
            var folder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            if (folder == null)
                return "Unknown";
            var path = Path.Combine(folder, dll);
            var date = File.GetLastWriteTime(path);
            return date.ToString(@"yy-MM-dd\.hh\:mm");
        }
    }
}