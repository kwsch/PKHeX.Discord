using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using PKHeX.Core;

namespace PKHeX.Discord
{
    public class LearnInfo : ModuleBase<SocketCommandContext>
    {
        [Command("learn"), Alias("canlearn")]
        [Summary("Checks if the pkm can learn all of the moves asked.")]
        public async Task LearnAsync([Remainder]string context)
        {
            var args = context.Split(", ");
            var species = args[0];
            var summary = EncounterLearn.CanLearn(species, args.Skip(1));
            var msg = summary
                ? $"Yep! {species} can learn {string.Join(", ", args.Skip(1))}."
                : $"Nope, {species} can't' learn {string.Join(", ", args.Skip(1))}";
            await ReplyAsync(msg).ConfigureAwait(false);
        }

        [Command("encounter"), Alias("find")]
        [Summary("Returns a list of encounter locations where a pkm can be found, to learn all of the moves asked.")]
        public async Task EncounterAsync([Remainder]string context)
        {
            var args = context.Split(", ");
            var species = args[0];
            var builder = new EmbedBuilder
            {
                Color = new Color(114, 137, 218),
                Description = $"Here's where you can find {species}:"
            };

            var summary = EncounterLearn.GetLearnSummary(species, args.Skip(1));
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

            if (!any)
            {
                await ReplyAsync("None").ConfigureAwait(false);
                return;
            }

            await ReplyAsync(null, false, builder.Build()).ConfigureAwait(false);
        }
    }
}