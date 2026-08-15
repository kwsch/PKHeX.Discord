using System;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Interactions;
using PKHeX.Core;

namespace PKHeX.Discord.Axew;

public class LearnInfoModule : SlashModuleBase
{
    private static readonly string[] Splitters = [", ", ","];

    [SlashCommand("learn", "Checks if a Pokémon can learn all of the requested moves.")]
    public async Task LearnAsync(
        [Summary("species", "Species name")] string species,
        [Summary("moves", "Comma-separated move names")] string moves)
    {
        var moveArgs = moves.Split(Splitters, StringSplitOptions.RemoveEmptyEntries);
        var str = GameInfo.Strings;

        if (StringUtil.FindIndexIgnoreCase(str.specieslist, species) < 0)
        {
            await RespondAsync($"Bad species argument ({species})!").ConfigureAwait(false);
            return;
        }

        foreach (var move in moveArgs)
        {
            if (StringUtil.FindIndexIgnoreCase(str.movelist, move) >= 0)
                continue;

            await RespondAsync($"Bad move argument ({move})!").ConfigureAwait(false);
            return;
        }

        var summary = EncounterLearn.CanLearn(species, moveArgs);
        var msg = summary
            ? $"Yep! {species} can learn {string.Join(", ", moveArgs)}."
            : $"Nope, {species} can't learn {string.Join(", ", moveArgs)}";

        await RespondAsync(msg).ConfigureAwait(false);
    }

    [SlashCommand("encounter", "Returns encounter locations where a Pokémon can be found.")]
    public async Task EncounterAsync(
        [Summary("species", "Species name")] string species,
        [Summary("moves", "Comma-separated move names")] string moves)
    {
        var moveArgs = moves.Split(Splitters, StringSplitOptions.RemoveEmptyEntries);
        var str = GameInfo.Strings;

        if (StringUtil.FindIndexIgnoreCase(str.specieslist, species) < 0)
        {
            await RespondAsync($"Bad species argument ({species})!").ConfigureAwait(false);
            return;
        }

        foreach (var move in moveArgs)
        {
            if (StringUtil.FindIndexIgnoreCase(str.movelist, move) >= 0)
                continue;

            await RespondAsync($"Bad move argument ({move})!").ConfigureAwait(false);
            return;
        }

        var summary = EncounterLearn.GetLearnSummary(species, moveArgs);
        var builder = new EmbedBuilder
        {
            Color = new Color(114, 137, 218),
            Description = "Encounters:"
        };

        var sb = new StringBuilder();
        var key = string.Empty;
        bool any = false;
        bool capped = false;

        foreach (var line in summary)
        {
            if (line.StartsWith('='))
            {
                any = true;
                if (sb.Length > 0)
                {
                    var key1 = key;
                    var msg = sb.ToString();

                    builder.AddField(x =>
                    {
                        x.Name = key1;
                        x.Value = msg;
                        x.IsInline = false;
                    });
                }

                key = line.Replace("=", "");
                capped = false;
                sb.Clear();

                continue;
            }

            if (sb.Length > 850 && !capped)
            {
                capped = true;
                sb.AppendLine("...and more! Too long to show all.");
            }
            else if (!capped)
            {
                sb.AppendLine(line);
            }
        }

        if (!any)
        {
            var fail = $"Sorry, can't find {species}";
            if (moveArgs.Length > 0)
                fail += $" with {string.Join(", ", moveArgs)}.";

            await RespondAsync(fail).ConfigureAwait(false);
            return;
        }

        if (sb.Length > 0)
        {
            var key1 = key;
            var msg = sb.ToString();

            builder.AddField(x =>
            {
                x.Name = key1;
                x.Value = msg;
                x.IsInline = false;
            });
        }

        var response = $"Here's where you can find {species}";
        if (moveArgs.Length > 0)
            response += $" with {string.Join(", ", moveArgs)}";

        await RespondAsync(response + ":", embed: builder.Build()).ConfigureAwait(false);
    }
}