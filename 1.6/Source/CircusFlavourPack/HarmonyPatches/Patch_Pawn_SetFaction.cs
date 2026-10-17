using System;
using HarmonyLib;
using RimWorld;
using Verse;

namespace CircusFlavourPack.HarmonyPatches;

/// <summary>
/// Swaps a pawn's xenotype when it joins the player faction as a colonist.
/// Recruiting, wanderers, rescued refugees, quest joiners, taming and the debug recruit action all end in SetFaction.
/// Slaves are skipped here and swapped when freed, see Patch_PawnGuestTracker_SetGuestStatus.
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
        if (newFaction == null || newFaction != Faction.OfPlayerSilentFail || __state == newFaction)
            return;
        // Enslavement calls SetFaction before the Slave status is set; freed slaves are handled by the SetGuestStatus patch.
        if (PawnGenerator.IsBeingGenerated(__instance) || Patch_PawnGuestTracker_SetGuestStatus.Enslaving)
            return;

        ApplyJoinXenotype(__instance, newFaction);
    }

    /// <summary>Swaps the pawn to the configured xenotype if the setting is on and the pawn is eligible.</summary>
    internal static void ApplyJoinXenotype(Pawn pawn, Faction faction)
    {
        if (!CircusFlavourPackMod.settings.xenotypeOnJoinEnabled || !ModsConfig.BiotechActive)
            return;
        // Skips scenario setup, and the new-colony/Archonexus restart dialog, which reassigns colonists while the game is playing.
        if (Current.ProgramState != ProgramState.Playing || Find.WindowStack.IsOpen<Dialog_ConfigureIdeo>())
            return;
        // Lodgers leave again but a xenotype swap would stay.
        if (!pawn.RaceProps.Humanlike || pawn.IsQuestLodger())
            return;

        XenotypeDef xenotype = CircusFlavourPackMod.settings.JoinXenotype;
        if (xenotype == null || !IsEligiblePawn(pawn) || pawn.genes.Xenotype == xenotype)
            return;

        pawn.genes.SetXenotype(xenotype);
        Messages.Message(
            "MSS_CFP_XenotypeChangedOnJoin".Translate(pawn.Named("PAWN"), faction.Name.Named("FACTION"), xenotype.LabelCap.Named("XENOTYPE")),
            pawn, MessageTypeDefOf.NeutralEvent);
    }

    private static bool IsEligiblePawn(Pawn pawn)
    {
        if (pawn.genes == null || pawn.IsMutant)
            return false;
        // VRE Androids keep a gene tracker, but a xenotype swap would graft organic genes onto their android genes.
        return !pawn.genes.GenesListForReading.Any(g => g.def.defName.StartsWith("VREA_", StringComparison.Ordinal));
    }
}
