using HarmonyLib;
using RimWorld;
using Verse;

namespace CircusFlavourPack.HarmonyPatches;

// pawns with the clown digestion gene get 'normal' meal comparable nutrition from VE dessert meals
[HarmonyPatch(typeof(FoodUtility), nameof(FoodUtility.NutritionForEater))]
public static class Patch_FoodUtility_NutritionForEater_Gene
{
    private const string DessertNutritionGeneDefName = "MSS_ClownDigestion";
    private static GeneDef dessertNutritionGeneDef;
    private static GeneDef DessertNutritionGeneDef => dessertNutritionGeneDef ??= DefDatabase<GeneDef>.GetNamedSilentFail(DessertNutritionGeneDefName);

    public static void Postfix(Pawn eater, Thing food, ref float __result)
    {
        if (food?.def == null || !TryGetClownDessertNutrition(food.def.defName, out float nutrition))
        {
            return;
        }

        GeneDef gene = DessertNutritionGeneDef;
        if (gene == null || eater?.genes == null || !eater.genes.HasActiveGene(gene))
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
