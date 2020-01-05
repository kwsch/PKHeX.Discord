using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using PKHeX.Core;

namespace PKHeX.Discord
{
    public class LearnInfoModule : ModuleBase<SocketCommandContext>
    {
        private static readonly string[] Splitters = {", ", ","};

        [Command("learn"), Alias("canlearn")]
        [Summary("Checks if the pkm can learn all of the moves asked.")]
        public async Task LearnAsync([Remainder][Summary("Separate the species and moves with a comma.")] string speciesAndMoves)
        {
            var args = speciesAndMoves.Split(Splitters, StringSplitOptions.RemoveEmptyEntries);
            var species = args[0];

            // Sanity check
            var str = GameInfo.Strings;
            if (StringUtil.FindIndexIgnoreCase(str.specieslist, species) < 0)
            {
                await ReplyAsync($"Bad species argument ({species})!").ConfigureAwait(false);
                return;
            }
            foreach (var move in args.Skip(1))
            {
                if (StringUtil.FindIndexIgnoreCase(str.movelist, move) >= 0)
                    continue;
                await ReplyAsync($"Bad move argument ({move})!").ConfigureAwait(false);
                return;
            }

            var summary = EncounterLearn.CanLearn(species, args.Skip(1));
            var msg = summary
                ? $"Yep! {species} can learn {string.Join(", ", args.Skip(1))}."
                : $"Nope, {species} can't learn {string.Join(", ", args.Skip(1))}";
            await ReplyAsync(msg).ConfigureAwait(false);
        }

        [Command("encounter"), Alias("find")]
        [Summary("Returns a list of encounter locations where a pkm can be found, to learn all of the moves asked.")]
        public async Task EncounterAsync([Remainder][Summary("Separate the species and moves with a comma.")]string speciesAndMoves)
        {
            var args = speciesAndMoves.Split(Splitters, StringSplitOptions.RemoveEmptyEntries);
            var species = args[0];

            // Sanity check
            var str = GameInfo.Strings;
            if (StringUtil.FindIndexIgnoreCase(str.specieslist, species) < 0)
            {
                await ReplyAsync($"Bad species argument ({species})!").ConfigureAwait(false);
                return;
            }
            foreach (var move in args.Skip(1))
            {
                if (StringUtil.FindIndexIgnoreCase(str.movelist, move) >= 0)
                    continue;
                await ReplyAsync($"Bad move argument ({move})!").ConfigureAwait(false);
                return;
            }

            var summary = EncounterLearn.GetLearnSummary(species, args.Skip(1));
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
                if (line.StartsWith("="))
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
                    sb.AppendLine( "...and more! Too long to show all.");
                }
                else if (!capped)
                {
                    sb.AppendLine(line);
                }
            }

            if (!any)
            {
                var fail = $"Sorry, can't find {species}";
                if (args.Length > 1)
                    fail += $" with {string.Join(", ", args.Skip(1))}.";
                await ReplyAsync(fail).ConfigureAwait(false);
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
            if (args.Length > 1)
                response += $" with {string.Join(", ", args.Skip(1))}";

            await ReplyAsync(response + ":", false, builder.Build()).ConfigureAwait(false);
        }
    }
}