using System;
using HarmonyLib;
using RimWorld;
using Verse;

namespace CircusFlavourPack.HarmonyPatches;

/// <summary>
/// Swaps a pawn's xenotype when it converts to the player faction's primary ideoligion.
/// Every conversion path (persuasion, rituals, the convert ability, mental breaks, debug actions) ends in SetIdeo.
/// </summary>
[HarmonyPatch(typeof(Pawn_IdeoTracker), nameof(Pawn_IdeoTracker.SetIdeo))]
public static class Patch_PawnIdeoTracker_SetIdeo
{
    [HarmonyPrefix]
    public static void Prefix(Pawn_IdeoTracker __instance, out Ideo __state)
    {
        __state = __instance.Ideo;
    }

    [HarmonyPostfix]
    public static void Postfix(Pawn_IdeoTracker __instance, Pawn ___pawn, Ideo __state)
    {
        Ideo newIdeo = __instance.Ideo;
        if (!IsInGameConversionToPrimary(___pawn, __state, newIdeo))
            return;

        XenotypeDef xenotype = CircusFlavourPackMod.settings.ConversionXenotype;
        if (xenotype == null || !IsEligiblePawn(___pawn) || ___pawn.genes.Xenotype == xenotype)
            return;

        ___pawn.genes.SetXenotype(xenotype);
        Messages.Message(
            "MSS_CFP_XenotypeChangedOnConversion".Translate(___pawn.Named("PAWN"), newIdeo.name.Named("IDEO"), xenotype.LabelCap.Named("XENOTYPE")),
            ___pawn, MessageTypeDefOf.NeutralEvent);
    }

    private static bool IsInGameConversionToPrimary(Pawn pawn, Ideo oldIdeo, Ideo newIdeo)
    {
        if (!CircusFlavourPackMod.settings.xenotypeOnConversionEnabled || !ModsConfig.IdeologyActive || !ModsConfig.BiotechActive)
            return false;
        // Skips scenario setup, and the new-colony/Archonexus restart dialog, which reassigns colonists while the game is playing.
        if (Current.ProgramState != ProgramState.Playing || Find.WindowStack.IsOpen<Dialog_ConfigureIdeo>())
            return false;
        // A null old ideo means a freshly generated pawn; equal ideos mean vanilla returned early (same ideo or a baby).
        if (oldIdeo == null || newIdeo == null || oldIdeo == newIdeo || PawnGenerator.IsBeingGenerated(pawn))
            return false;
        return newIdeo == Faction.OfPlayerSilentFail?.ideos?.PrimaryIdeo;
    }

    private static bool IsEligiblePawn(Pawn pawn)
    {
        if (pawn.genes == null || pawn.IsMutant)
            return false;
        // VRE Androids keep an ideo and a gene tracker, but a xenotype swap would graft organic genes onto their android genes.
        return !pawn.genes.GenesListForReading.Any(g => g.def.defName.StartsWith("VREA_", StringComparison.Ordinal));
    }
}
