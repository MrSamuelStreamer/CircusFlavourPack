using System;
using System.Collections.Generic;
using HarmonyLib;
using RimWorld;
using SomeThingsFloat;
using Verse;
using FloatMod = SomeThingsFloat.SomeThingsFloat;

namespace CircusFlavourPack;

/// <summary>
/// Some Things Float only rolls food, skins, wood, and medicine.
/// Any other tradeable item is added to that list. Their own value cap, commonality, and stack roll stay as they are.
/// </summary>
[HarmonyPatch(typeof(SomeThingsFloat.SomeThingsFloat), MethodType.StaticConstructor)]
public static class Patch_SomeThingsFloat_SpawnLoot
{
    public static bool InsideSpawn;

    static Dictionary<ThingDef, float> savedCommonality;

    public static void Postfix() => EnsurePool();

    public static void EnsurePool()
    {
        if (FloatMod.ThingsToCreate == null)
            return;

        foreach (ThingDef def in DefDatabase<ThingDef>.AllDefsListForReading)
        {
            if (IsSpawnableItem(def))
                FloatMod.ThingsToCreate.Add(def);
        }
    }

    [HarmonyPatch(typeof(FloatingThings_MapComponent), nameof(FloatingThings_MapComponent.TrySpawnThingAtMapEdge))]
    public static class Patch_Spawn
    {
        public static void Prefix()
        {
            InsideSpawn = true;
            EnsurePool();
            savedCommonality = new Dictionary<ThingDef, float>();
            foreach (ThingDef def in FloatMod.ThingsToCreate)
            {
                if (def.generateCommonality > 0f)
                    continue;

                // A weight of 0 is never picked. Give those items the same chance as a normal commonality of 1.
                savedCommonality[def] = def.generateCommonality;
                def.generateCommonality = 1f;
            }
        }

        public static Exception Finalizer(Exception __exception)
        {
            InsideSpawn = false;
            if (savedCommonality != null)
            {
                foreach (KeyValuePair<ThingDef, float> pair in savedCommonality)
                    pair.Key.generateCommonality = pair.Value;
                savedCommonality = null;
            }

            return __exception;
        }
    }

    /// <summary>
    /// Weapons and apparel need a material. Their spawn calls MakeThing without one.
    /// </summary>
    [HarmonyPatch(typeof(ThingMaker), nameof(ThingMaker.MakeThing))]
    public static class Patch_Stuff
    {
        public static void Prefix(ThingDef def, ref ThingDef stuff)
        {
            if (!InsideSpawn || def == null || !def.MadeFromStuff || stuff != null)
                return;

            stuff = GenStuff.DefaultStuffFor(def);
        }
    }

    static bool IsSpawnableItem(ThingDef def)
    {
        if (def.category != ThingCategory.Item || def.BaseMarketValue <= 0f || def.destroyOnDrop)
            return false;
        if (def.thingClass == null || def.thingClass == typeof(UnfinishedThing) || def.IsCorpse || def.IsPlant || def.IsFilth)
            return false;
        if (typeof(Pawn).IsAssignableFrom(def.thingClass) || typeof(Building).IsAssignableFrom(def.thingClass) || typeof(MinifiedThing).IsAssignableFrom(def.thingClass))
            return false;
        if (def.tradeability == Tradeability.None)
            return false;
        return true;
    }
}
