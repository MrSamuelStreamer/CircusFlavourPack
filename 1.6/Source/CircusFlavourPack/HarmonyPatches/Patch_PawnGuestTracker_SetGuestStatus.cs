using HarmonyLib;
using RimWorld;
using Verse;

namespace CircusFlavourPack.HarmonyPatches;

/// <summary>
/// Handles both ends of slavery for the xenotype swap.
/// Enslaving: SetGuestStatus calls Pawn.SetFaction before it records the Slave status, so Patch_Pawn_SetFaction
/// cannot tell a new slave from a new colonist by looking at the pawn. The flag marks that span.
/// Freeing: a freed slave of the colony stays in the faction without any SetFaction call, so it is swapped here.
/// </summary>
[HarmonyPatch(typeof(Pawn_GuestTracker), nameof(Pawn_GuestTracker.SetGuestStatus))]
public static class Patch_PawnGuestTracker_SetGuestStatus
{
    internal static bool Enslaving;

    [HarmonyPrefix]
    public static void Prefix(Pawn_GuestTracker __instance, GuestStatus guestStatus, out (bool wasEnslaving, bool wasSlave) __state)
    {
        __state = (Enslaving, __instance.IsSlave);
        Enslaving = guestStatus == GuestStatus.Slave;
    }

    [HarmonyPostfix]
    public static void Postfix(Pawn_GuestTracker __instance, Pawn ___pawn, GuestStatus guestStatus, (bool wasEnslaving, bool wasSlave) __state)
    {
        // Slaves of other factions are released off the map, so only a freed slave still in the player faction reaches here.
        if (!__state.wasSlave || guestStatus != GuestStatus.Guest || __instance.IsSlave || ___pawn.Faction != Faction.OfPlayerSilentFail)
            return;

        Patch_Pawn_SetFaction.ApplyJoinXenotype(___pawn, ___pawn.Faction);
    }

    // A finalizer runs even when vanilla throws, so the flag cannot stay set.
    [HarmonyFinalizer]
    public static void Finalizer((bool wasEnslaving, bool wasSlave) __state)
    {
        Enslaving = __state.wasEnslaving;
    }
}
