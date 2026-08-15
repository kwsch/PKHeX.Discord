using System;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Discord;
using Discord.Interactions;

namespace PKHeX.Discord.Axew;

// src: https://github.com/foxbot/patek/blob/169a4d93b099a843bc0ddce1a30f14132c073fe4/src/Patek/Modules/InfoModule.cs
// ISC License (ISC)
// Copyright 2017, Christopher F. <foxbot@protonmail.com>
// Adapted by kwsch 2020; updated for Slash commands 2026.

[RequireContext(ContextType.Guild)]
public class InfoModule : SlashModuleBase
{
    private const string Detail = "I am an open source Discord bot powered by PKHeX.Core and other open source software.";
    private const string Repo = "https://github.com/kwsch/PKHeX.Discord";

    [SlashCommand("info", "Displays information about the bot.")]
    public async Task InfoAsync()
    {
        var builder = new EmbedBuilder
        {
            Color = Color.Blue,
            Description = Detail
        };

        builder.AddField("Info",
$"""
- [Source Code]({Repo})
- {Format.Bold("Library")}: Discord.Net ({DiscordConfig.Version})
- {Format.Bold("Started")}: {GetStartTimeRelative()}
- {Format.Bold("Runtime")}: {RuntimeInformation.FrameworkDescription} {RuntimeInformation.ProcessArchitecture} ({RuntimeInformation.OSDescription} {RuntimeInformation.OSArchitecture})
- {Format.Bold("Buildtime")}: {GetVersionInfo("SysBot.Pokemon.Discord", false)}
- {Format.Bold("Core Version")}: {GetVersionInfo("PKHeX.Core")}
- {Format.Bold("AutoLegality Version")}: {GetVersionInfo("PKHeX.Core.AutoMod")}
"""
            );
        builder.AddField("Stats",
$"""
- {Format.Bold("Heap Size")}: {GetHeapSize()}MiB
- {Format.Bold("Guilds")}: {Context.Client.Guilds.Count}
- {Format.Bold("Channels")}: {Context.Client.Guilds.Sum(g => g.Channels.Count)}
- {Format.Bold("Users")}: {Context.Client.Guilds.Sum(g => g.MemberCount)}
""");
        await RespondAsync("Here's a bit about me!", embed: builder.Build()).ConfigureAwait(false);
    }

    private static string GetStartTimeRelative() => TimestampTag.FromDateTime(Process.GetCurrentProcess().StartTime.ToUniversalTime(), TimestampTagStyles.Relative).ToString();
    private static string GetHeapSize() => Math.Round(GC.GetTotalMemory(true) / (1024.0 * 1024.0), 2).ToString(CultureInfo.CurrentCulture);

    private static string GetVersionInfo(string assemblyName, bool inclVersion = true)
    {
        const string unknownVersion = "Unknown";

        var assemblies = AppDomain.CurrentDomain.GetAssemblies();
        var assembly = Array.Find(assemblies, x => x.GetName().Name == assemblyName);

        var attribute = assembly?.GetCustomAttribute<AssemblyInformationalVersionAttribute>();
        if (attribute is null)
            return unknownVersion;

        var info = attribute.InformationalVersion;
        var split = info.Split('+');
        if (split.Length < 2)
            return unknownVersion;

        var version = split[0];
        var revision = split[1];
        revision = revision.Split('.')[^1]; // sometimes builds have extra metadata prepended, followed by .timestamp -- just keep the ending.
        if (DateTime.TryParseExact(revision, "yyMMddHHmmss", CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out var buildTime))
            return (inclVersion ? $"{version} " : "") + $"{TimestampTag.FromDateTime(buildTime, TimestampTagStyles.ShortDateTime)}";
        return !inclVersion ? unknownVersion : version;
    }
}
