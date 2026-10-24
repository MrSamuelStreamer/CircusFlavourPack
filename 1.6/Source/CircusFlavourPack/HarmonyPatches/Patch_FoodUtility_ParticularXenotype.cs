using HarmonyLib;
using RimWorld;
using Verse;

namespace CircusFlavourPack.HarmonyPatches;

// clown xenotype gets 'normal' meal comparable nutrition from VE dessert meals
[HarmonyPatch(typeof(FoodUtility), nameof(FoodUtility.NutritionForEater))]
public static class Patch_FoodUtility_NutritionForEater_ParticularXenotype
{
    private const string ClownXenotypeDefName = "MSS_Clown";
    private static XenotypeDef clownDef;
    private static XenotypeDef ClownDef => clownDef ??= DefDatabase<XenotypeDef>.GetNamedSilentFail(ClownXenotypeDefName);

    public static void Postfix(Pawn eater, Thing food, ref float __result)
    {
        if (food?.def == null || !TryGetClownDessertNutrition(food.def.defName, out float nutrition))
        {
            return;
        }
        XenotypeDef clown = ClownDef;
        if (clown == null || eater?.genes?.Xenotype != clown)
        {
            return;
        }
        __result = nutrition;
    }

    private static bool TryGetClownDessertNutrition(string defName, out float nutrition)
    {
        switch (defName)
        {
            case "VCE_SimpleDessert":
            case "VCE_FineDessert":
                nutrition = 0.9f;
                return true;
            case "VCE_LavishDessert":
                nutrition = 1.0f;
                return true;
            case "VCE_GourmetDessert":
                nutrition = 0.7f;
                return true;
            default:
                nutrition = 0f;
                return false;
        }
    }
}
