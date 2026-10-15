using System.Collections.Generic;
using HarmonyLib;
using RimWorld;
using Verse;

namespace CircusFlavourPack.HarmonyPatches;

// filter out all filler rewards except silver
[HarmonyPatch(typeof(RewardsGenerator), nameof(RewardsGenerator.ResetStaticData))]
public static class Patch_RewardsGenerator_SetFiller
{
    [HarmonyPostfix]
    public static void Postfix(List<ThingDef> ___MarketValueFillers)
    {
        if (!ModsConfig.IsActive("sarg.alpharandom")) return;

        ___MarketValueFillers.RemoveAll(x => x != ThingDefOf.Silver);
        ModLog.Log($"Reward fillers restricted to silver");
    }
}
