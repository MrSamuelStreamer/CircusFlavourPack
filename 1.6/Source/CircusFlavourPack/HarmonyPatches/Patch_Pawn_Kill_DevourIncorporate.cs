using System;
using HarmonyLib;
using RimWorld;
using Verse;

namespace CircusFlavourPack.HarmonyPatches;

/// <summary>
/// Big and Small's Devour ability (BS_SnekEngulf_New) swallows the victim into a BigAndSmall.EngulfHediff on the devourer.
/// When the swallowed pawn dies, this runs Big and Small's Incorporate gene steal on it for the devourer.
/// Player devourers pick genes from the Incorporate dialog. Other devourers get random genes.
/// Big and Small is bound by reflection, so this assembly still loads without it.
/// </summary>
[HarmonyPatch(typeof(Pawn), nameof(Pawn.Kill), typeof(DamageInfo?), typeof(Hediff))]
public static class Patch_Pawn_Kill_DevourIncorporate
{
    private const int GenePickCount = 4;
    private const int RandomPickCount = 4;
    // Value the Incorporate ability passes to RemoveGenesOverLimit after use.
    private const int MetabolismLimit = -9;

    private delegate void IncorporateGenesFn(Pawn pawn, object target, int genePickCount, bool stealTraits, bool userPicks, int randomPickCount, bool excludeBodySwap);
    private delegate bool RemoveGenesOverLimitFn(Pawn pawn, int limit);

    private static readonly Type EngulfHediffType = AccessTools.TypeByName("BigAndSmall.EngulfHediff");
    private static readonly Type IncorporateType = AccessTools.TypeByName("BigAndSmall.CompProperties_IncorporateEffect");
    private static readonly IncorporateGenesFn IncorporateGenes = BindDelegate<IncorporateGenesFn>("IncorporateGenes");
    private static readonly RemoveGenesOverLimitFn RemoveGenesOverLimit = BindDelegate<RemoveGenesOverLimitFn>("RemoveGenesOverLimit");

    public static bool Prepare()
    {
        if (!ModsConfig.IsActive("redmattis.bigsmall.core"))
            return false;
        if (EngulfHediffType != null && IncorporateGenes != null && RemoveGenesOverLimit != null)
            return true;

        ModLog.Warn("Big and Small is active but its Devour/Incorporate API was not found. Devour will not incorporate genes.");
        return false;
    }

    [HarmonyPrefix]
    public static void Prefix(Pawn __instance, out Hediff __state)
    {
        // The victim is held in the EngulfHediff's ThingOwner, so the hediff is its ParentHolder.
        __state = EngulfHediffType.IsInstanceOfType(__instance.ParentHolder) ? (Hediff)__instance.ParentHolder : null;
    }

    [HarmonyPostfix]
    public static void Postfix(Pawn __instance, Hediff __state)
    {
        if (__state == null || !__instance.Dead)
            return;

        Pawn devourer = __state.pawn;
        if (devourer == null || devourer.Dead || devourer.Destroyed || devourer.genes == null)
            return;

        try
        {
            bool userPicks = devourer.Faction is { IsPlayer: true };
            // The corpse goes back into the stomach and keeps digesting. Incorporate accepts the dead pawn itself.
            IncorporateGenes(devourer, __instance, GenePickCount, true, userPicks, RandomPickCount, false);
            RemoveGenesOverLimit(devourer, MetabolismLimit);
        }
        catch (Exception e)
        {
            ModLog.Error($"Failed to incorporate genes from devoured {__instance.ToStringSafe()} into {devourer.ToStringSafe()}.", e);
        }
    }

    private static T BindDelegate<T>(string methodName) where T : Delegate
    {
        if (IncorporateType == null)
            return null;
        var method = AccessTools.Method(IncorporateType, methodName);
        return method == null ? null : (T)Delegate.CreateDelegate(typeof(T), method, false);
    }
}
