using System;
using HarmonyLib;
using RimWorld;
using Verse;

namespace CircusFlavourPack.HarmonyPatches;

/// <summary>
/// Swaps a pawn's xenotype when it joins the player faction as a colonist.
/// Recruiting, wanderers, rescued refugees, quest joiners, taming and the debug recruit action all end in SetFaction.
/// </summary>
[HarmonyPatch(typeof(Pawn), nameof(Pawn.SetFaction))]
public static class Patch_Pawn_SetFaction
{
    [HarmonyPrefix]
    public static void Prefix(Pawn __instance, out Faction __state)
    {
        __state = __instance.Faction;
    }

    [HarmonyPostfix]
    public static void Postfix(Pawn __instance, Faction newFaction, Faction __state)
    {
        if (!IsColonistJoin(__instance, __state, newFaction))
            return;

        XenotypeDef xenotype = CircusFlavourPackMod.settings.JoinXenotype;
        if (xenotype == null || !IsEligiblePawn(__instance) || __instance.genes.Xenotype == xenotype)
            return;

        __instance.genes.SetXenotype(xenotype);
        Messages.Message(
            "MSS_CFP_XenotypeChangedOnJoin".Translate(__instance.Named("PAWN"), newFaction.Name.Named("FACTION"), xenotype.LabelCap.Named("XENOTYPE")),
            __instance, MessageTypeDefOf.NeutralEvent);
    }

    private static bool IsColonistJoin(Pawn pawn, Faction oldFaction, Faction newFaction)
    {
        if (!CircusFlavourPackMod.settings.xenotypeOnJoinEnabled || !ModsConfig.BiotechActive)
            return false;
        // Skips scenario setup, and the new-colony/Archonexus restart dialog, which reassigns colonists while the game is playing.
        if (Current.ProgramState != ProgramState.Playing || Find.WindowStack.IsOpen<Dialog_ConfigureIdeo>())
            return false;
        if (newFaction == null || newFaction != Faction.OfPlayerSilentFail || oldFaction == newFaction || PawnGenerator.IsBeingGenerated(pawn))
            return false;
        // Lodgers leave again but a xenotype swap would stay; slaves are excluded by choice.
        return pawn.RaceProps.Humanlike && !pawn.IsQuestLodger() && !Patch_PawnGuestTracker_SetGuestStatus.Enslaving;
    }

    private static bool IsEligiblePawn(Pawn pawn)
    {
        if (pawn.genes == null || pawn.IsMutant)
            return false;
        // VRE Androids keep a gene tracker, but a xenotype swap would graft organic genes onto their android genes.
        return !pawn.genes.GenesListForReading.Any(g => g.def.defName.StartsWith("VREA_", StringComparison.Ordinal));
    }
}
