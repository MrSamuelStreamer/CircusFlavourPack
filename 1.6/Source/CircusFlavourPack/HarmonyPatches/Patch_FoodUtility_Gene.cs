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

// desserts had preferability of `DesperateOnly` meaning they would eat raw shit before a filling dessert.
// xml patch for preferability easily fixes this however non-gene pawns would also prefer the food despite it only giving them 0.2 nutrition so they'd eat 4-5 meals a go.
// FoodOptimality is -150 for DesperateOnly preferability. the below makes a non-gene pawns resultingly treat desserts as DesperateOnly despite the xml tag.
//had to re define the list since i couldn't be arsed messing with the code at the top, it was probably better to cache and redo as internal but fuck it
[HarmonyPatch(typeof(FoodUtility), nameof(FoodUtility.FoodOptimality))]
public static class Patch_FoodUtility_FoodOptimality_Gene
{
    private const string GeneDefName = "MSS_ClownDigestion";
    private static GeneDef geneDef;
    private static bool geneLookedUp;

    private static readonly System.Collections.Generic.HashSet<string> DessertDefNames = new()
    {
        "VCE_SimpleDessert",
        "VCE_FineDessert",
        "VCE_LavishDessert",
        "VCE_GourmetDessert"
    };

    public static void Postfix(Pawn eater, ThingDef foodDef, ref float __result)
    {
        if (foodDef == null || !DessertDefNames.Contains(foodDef.defName))
        {
            return;
        }

        if (!eater_HasGene(eater))
        {
            __result -= 150f;
        }
    }

    private static bool eater_HasGene(Pawn eater)
    {
        if (!geneLookedUp)
        {
            geneDef = DefDatabase<GeneDef>.GetNamedSilentFail(GeneDefName);
            geneLookedUp = true;
        }
        return geneDef != null && eater?.genes != null && eater.genes.HasActiveGene(geneDef);
    }
}
