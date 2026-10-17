using HarmonyLib;
using RimWorld;

namespace CircusFlavourPack.HarmonyPatches;

/// <summary>
/// Flags the span of an enslavement. SetGuestStatus calls Pawn.SetFaction before it records the Slave status,
/// so Patch_Pawn_SetFaction cannot tell a new slave from a new colonist by looking at the pawn.
/// </summary>
[HarmonyPatch(typeof(Pawn_GuestTracker), nameof(Pawn_GuestTracker.SetGuestStatus))]
public static class Patch_PawnGuestTracker_SetGuestStatus
{
    internal static bool Enslaving;

    [HarmonyPrefix]
    public static void Prefix(GuestStatus guestStatus, out bool __state)
    {
        __state = Enslaving;
        Enslaving = guestStatus == GuestStatus.Slave;
    }

    // A finalizer runs even when vanilla throws, so the flag cannot stay set.
    [HarmonyFinalizer]
    public static void Finalizer(bool __state)
    {
        Enslaving = __state;
    }
}
