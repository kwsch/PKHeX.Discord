using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using PKHeX.Core;

namespace PKHeX.Discord
{
    public class PersonalModule : ModuleBase<SocketCommandContext>
    {
        [Command("personal"), Alias("pi")]
        [Summary("Prints Personal data for the species.")]
        public async Task PrintDataAsync([Summary("Species Index")]int species, [Summary("Form Index")]int form)
        {
            await PrintPersonalInfoAsync(species, form).ConfigureAwait(false);
        }

        [Command("personal"), Alias("pi")]
        [Summary("Prints Personal data for the species.")]
        public async Task PrintDataAsync([Summary("species-form")][Remainder]string species)
        {
            var split = species.Split('-');
            if (split.Length == 0)
            {
                await ReplyAsync("Bad input arguments!").ConfigureAwait(false);
                return;
            }
            GetSpeciesForm(split, out var spec, out var form);
            await PrintDataAsync(spec, form).ConfigureAwait(false);
        }

        private static void GetSpeciesForm(IReadOnlyList<string> split, out int spec, out int form)
        {
            var strings = GameInfo.Strings;
            spec = StringUtil.FindIndexIgnoreCase(strings.specieslist, split[0]);

            form = 0;
            if (split.Count <= 1)
                return;
            var formstr = split[1];
            if (int.TryParse(formstr, out form))
                return;
            var forms = FormConverter.GetFormList(spec, strings.Types, strings.forms, GameInfo.GenderSymbolASCII, 8);
            form = StringUtil.FindIndexIgnoreCase(forms, formstr);
        }

        private static readonly string[] AbilitySuffix = { " (1)", " (2)", " (H)" };
        private static readonly string[] ItemPrefix = { "Item 1 (50%)", "Item 2 (5%)", "Item 3 (1%)" };

        private async Task PrintPersonalInfoAsync(int spec, int form)
        {
            var strings = GameInfo.Strings;
            if (spec <= 0 || spec >= strings.specieslist.Length)
            {
                await ReplyAsync("Bad species argument!").ConfigureAwait(false);
                return;
            }
            var forms = FormConverter.GetFormList(spec, strings.Types, strings.forms, GameInfo.GenderSymbolASCII, 8);
            if (form < 0 || form >= forms.Length)
            {
                await ReplyAsync("Bad form argument!").ConfigureAwait(false);
                return;
            }
            var pi = PersonalTable.SWSH.GetFormeEntry(spec, form);
            if (pi.HP == 0)
                pi = PersonalTable.USUM.GetFormeEntry(spec, form);

            var specName = strings.specieslist[spec];
            var formName = forms[form];
            var lines = GetPersonalInfoSummary(pi, strings);

            var builder = new EmbedBuilder
            {
                Color = Color.Gold,
            };
            builder.AddField(x =>
            {
                x.Name = $"Personal Info for {specName}-{formName}:";
                x.Value = string.Join('\n', lines.Select(GetPrettyLine));
                x.IsInline = false;
            });

            await ReplyAsync("Personal Info!", embed: builder.Build()).ConfigureAwait(false);
        }

        private static string GetPrettyLine(string arg)
        {
            var split = arg.Split(':');
            return $"{Format.Bold(split[0])}:{split[1]}";
        }

        private static IEnumerable<string> GetPersonalInfoSummary(PersonalInfo pi, GameStrings strings)
        {
            var types = strings.types;
            var abilities = strings.abilitylist;
            var itemNames = strings.itemlist;
            var lines = new List<string>
            {
                $"Base Stats: {pi.HP}.{pi.ATK}.{pi.DEF}.{pi.SPA}.{pi.SPD}.{pi.SPE} (BST={pi.BST})",
                $"EV Yield: {pi.EV_HP}.{pi.EV_ATK}.{pi.EV_DEF}.{pi.EV_SPA}.{pi.EV_SPD}.{pi.EV_SPE}",
                $"Gender Ratio: {pi.Gender}",
                $"Catch Rate: {pi.CatchRate}",
                $"Form Count: {pi.FormeCount}",
            };

            var abils = pi.Abilities;
            var msg = string.Join(" | ", abils.Select((z, j) => abilities[z] + AbilitySuffix[j]));
            lines.Add($"Abilities: {msg}");
            lines.Add(string.Format(pi.Type1 != pi.Type2
                ? "Type: {0} / {1}"
                : "Type: {0}", types[pi.Type1], types[pi.Type2]));
            var items = pi.Items;
            if (items.Distinct().Count() == 1)
                lines.Add($"Items: {itemNames[pi.Items[0]]}");
            else
                lines.AddRange(items.Select((z, j) => $"{ItemPrefix[j]}: {itemNames[z]}"));

            var ExpGroups = Enum.GetNames(typeof(EXPGroup));
            var Colors = Enum.GetNames(typeof(PokeColor));
            var EggGroups = Enum.GetNames(typeof(EggGroup));
            lines.Add($"EXP Group: {ExpGroups[pi.EXPGrowth]}");
            lines.Add(string.Format(pi.EggGroup1 != pi.EggGroup2
                ? "Egg Group: {0} / {1}"
                : "Egg Group: {0}", EggGroups[pi.EggGroup1], EggGroups[pi.EggGroup2]));
            lines.Add($"Hatch Cycles: {pi.HatchCycles}");
            lines.Add($"Height: {(decimal) pi.Height / 100:00.00} m");
            lines.Add($"Weight: {(decimal) pi.Weight / 10:000.0} kg");
            lines.Add($"Color: {Colors[pi.Color]}");
            return lines;
        }
    }

    public enum PokeColor
    {
        Red,
        Blue,
        Yellow,
        Green,
        Black,
        Brown,
        Purple,
        Gray,
        White,
        Pink,
    }

    public enum EggGroup
    {
        None,
        Monster,
        Water1,
        Bug,
        Flying,
        Field,
        Fairy,
        Grass,
        HumanLike,
        Water3,
        Mineral,
        Amorphous,
        Water2,
        Ditto,
        Dragon,
        Undiscovered,
    }

    public enum EXPGroup
    {
        MediumFast,
        Erratic,
        Fluctuating,
        MediumSlow,
        Fast,
        Slow,
    }
}