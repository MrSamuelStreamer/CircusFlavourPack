using System.Reflection;
using HarmonyLib;
using Verse;
using Verse.AI;

namespace CircusFlavourPack.HarmonyPatches;

[HarmonyPatch]
static class Patch_AG_DeliverMetal
{
    static MethodBase target;
    static bool Prepare()
    {
        var type = AccessTools.TypeByName("AlphaGenes.WorkGiver_Warden_DeliverMetal");
        target = type == null ? null : AccessTools.DeclaredMethod(type, "JobOnThing");
        return target != null;
    }
    static MethodBase TargetMethod() => target;

    [HarmonyPriority(Priority.Last)]
    [HarmonyAfter("ferny.CaptureExpansion")]
    static bool Prefix(Thing x, ref Job __result)
    {
        if (x is Pawn p && !p.Spawned) { __result = null; return false; }
        return true;
    }
}
