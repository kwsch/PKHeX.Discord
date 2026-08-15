using System;
using PKHeX.Core;
using PKHeX.Core.AutoMod;

namespace PKHeX.Discord.Axew;

public static class AutoLegalityWrapper
{
    public static ITrainerInfo GetTrainerInfo<T>() where T : PKM, new()
    {
        if (typeof(T) == typeof(PK8))
            return TrainerSettings.GetSavedTrainerData(GameVersion.SWSH);
        if (typeof(T) == typeof(PB8))
            return TrainerSettings.GetSavedTrainerData(GameVersion.BDSP);
        if (typeof(T) == typeof(PA8))
            return TrainerSettings.GetSavedTrainerData(GameVersion.PLA);
        if (typeof(T) == typeof(PK9))
            return TrainerSettings.GetSavedTrainerData(GameVersion.SV);
        if (typeof(T) == typeof(PA9))
            return TrainerSettings.GetSavedTrainerData(GameVersion.ZA);

        throw new ArgumentException("Type does not have a recognized trainer fetch.", typeof(T).Name);
    }

    public static ITrainerInfo GetTrainerInfo(GameVersion version) => TrainerSettings.GetSavedTrainerData(version);

    public static PKM GetLegal(this ITrainerInfo sav, IBattleTemplate set, out string res)
    {
        var result = sav.GetLegalFromSet(set);
        res = result.Status switch
        {
            LegalizationResult.Regenerated => "Regenerated",
            LegalizationResult.Failed => "Failed",
            LegalizationResult.Timeout => "Timeout",
            LegalizationResult.VersionMismatch => "VersionMismatch",
            _ => "",
        };
        return result.Created;
    }

    public static string GetLegalizationHint(IBattleTemplate set, ITrainerInfo sav, PKM pk) => set.SetAnalysis(sav, pk);
    public static PKM LegalizePokemon(this PKM pk) => pk.Legalize();
    public static IBattleTemplate GetTemplate(ShowdownSet set) => new RegenTemplate(set);
}