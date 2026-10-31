using CircusFlavourPack.Fireproofing;
using HarmonyLib;
using RimWorld;
using Verse;

namespace CircusFlavourPack.HarmonyPatches;

/// <summary>
/// Floors in cells fireproofed by Ring of Fire Rehearsal cannot catch fire or burn away.
/// TerrainFlammability, fire ignition chance and Fire.TryBurnFloor all check this method.
/// </summary>
[HarmonyPatch(typeof(FireUtility), nameof(FireUtility.TerrainFlammableNow))]
public static class Patch_FireUtility_TerrainFlammableNow
{
    [HarmonyPostfix]
    public static void Postfix(IntVec3 c, Map map, ref bool __result)
    {
        if (__result && (MapComponent_FireproofRegistry.For(map)?.IsFireproof(c) ?? false))
            __result = false;
    }
}
