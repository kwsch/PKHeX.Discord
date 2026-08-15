using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Interactions;
using PKHeX.Core;

namespace PKHeX.Discord.Axew;

public class PersonalModule : SlashModuleBase
{
    private static readonly string[] AbilitySuffix =
        [" (1)", " (2)", " (H)"];

    [SlashCommand("personal", "Prints Personal data for the species.")]
    public async Task PrintDataAsync(
        [Summary("species", "Species name or species index")] string species,
        [Summary("form", "Form name or form index")] string? form = null)
    {
        var strings = GameInfo.Strings;

        ushort spec;

        if (ushort.TryParse(species, out var speciesIndex))
        {
            spec = speciesIndex;
        }
        else
        {
            var index = StringUtil.FindIndexIgnoreCase(strings.specieslist, species);

            if (index < 0)
            {
                await RespondAsync("Bad species argument!").ConfigureAwait(false);
                return;
            }

            spec = (ushort)index;
        }

        byte formIndex = 0;

        if (!string.IsNullOrWhiteSpace(form))
        {
            var forms = FormConverter.GetFormList(spec, strings.Types, strings.forms, GameInfo.GenderSymbolASCII, Latest.Context);

            if (byte.TryParse(form, out var numericForm))
            {
                formIndex = numericForm;
            }
            else
            {
                var formFound = StringUtil.FindIndexIgnoreCase(forms, form);

                if (formFound < 0)
                {
                    await RespondAsync("Bad form argument!").ConfigureAwait(false);
                    return;
                }

                formIndex = (byte)formFound;
            }
        }

        await PrintPersonalInfoAsync(spec, formIndex).ConfigureAwait(false);
    }

    private async Task PrintPersonalInfoAsync(ushort spec, byte form)
    {
        var strings = GameInfo.Strings;

        if (spec <= 0 || spec >= strings.specieslist.Length)
        {
            await RespondAsync("Bad species argument!").ConfigureAwait(false);
            return;
        }

        var forms = FormConverter.GetFormList(
            spec,
            strings.Types,
            strings.forms,
            GameInfo.GenderSymbolASCII,
            Latest.Context);

        if (form >= forms.Length)
        {
            await RespondAsync("Bad form argument!").ConfigureAwait(false);
            return;
        }

        IPersonalInfo pi = PersonalTable.SWSH.GetFormEntry(spec, form);

        if (pi.HP == 0)
            pi = PersonalTable.USUM.GetFormEntry(spec, form);

        var specName = strings.specieslist[spec];
        var formName = forms[form];
        var lines = GetPersonalInfoSummary(pi, strings);

        var builder = new EmbedBuilder
        {
            Color = Color.Gold
        };

        builder.AddField(x =>
        {
        x.Name =
            $"Personal Info for {specName}" +
            $"{(string.IsNullOrEmpty(formName) ? "" : $"-{formName}")}:";
            x.Value = string.Join('\n', lines.Select(GetPrettyLine));
            x.IsInline = false;
        });

        await RespondAsync("Personal Info!", embed: builder.Build()).ConfigureAwait(false);
    }

    private static string GetPrettyLine(string arg)
    {
        var split = arg.Split(':');
        return $"{Format.Bold(split[0])}:{split[1]}";
    }

    private static List<string> GetPersonalInfoSummary(
        IPersonalInfo pi,
        GameStrings strings)
    {
        var types = strings.types;
        var abilities = strings.abilitylist;

        var lines = new List<string>
        {
            $"Base Stats: {pi.HP}.{pi.ATK}.{pi.DEF}.{pi.SPA}.{pi.SPD}.{pi.SPE} (BST={pi.GetBaseStatTotal()})",
            $"EV Yield: {pi.EV_HP}.{pi.EV_ATK}.{pi.EV_DEF}.{pi.EV_SPA}.{pi.EV_SPD}.{pi.EV_SPE}",
            $"Gender Ratio: {pi.Gender}",
            $"Catch Rate: {pi.CatchRate}",
            $"Form Count: {pi.FormCount}",
            $"Evolution Stage: {pi.EvoStage}",
        };

        if (pi is IPersonalAbility p)
        {
            var count = p.AbilityCount;
            var msg = string.Join(" | ", Enumerable.Range(0, count).Select(z => abilities[p.GetAbilityAtIndex(z)] + AbilitySuffix[z]));
            lines.Add($"Abilities: {msg}");
        }
        lines.Add(string.Format(pi.Type1 != pi.Type2
            ? "Type: {0} / {1}"
            : "Type: {0}", types[pi.Type1], types[pi.Type2]));

        var expGroups = Enum.GetNames<GrowthRate>();
        var colors = Enum.GetNames<PersonalColor>();
        var eggGroups = Enum.GetNames<EggGroup>();

        lines.Add($"EXP Group: {expGroups[pi.EXPGrowth]}");
        lines.Add(string.Format(
            pi.EggGroup1 != pi.EggGroup2
                ? "Egg Group: {0} / {1}"
                : "Egg Group: {0}", eggGroups[pi.EggGroup1], eggGroups[pi.EggGroup2]));

        lines.Add($"Hatch Cycles: {pi.HatchCycles}");
        lines.Add($"Height: {(decimal)pi.Height / 100:00.00} m");
        lines.Add($"Weight: {(decimal)pi.Weight / 10:000.0} kg");
        lines.Add($"Color: {colors[pi.Color]}");

        return lines;
    }
}

public enum GrowthRate : byte
{
    MediumFast,
    Erratic,
    Fluctuating,
    MediumSlow,
    Fast,
    Slow,
}