using System;
using HarmonyLib;
using SomeThingsFloat;
using Verse;

namespace CircusFlavourPack;

/// <summary>
/// Some Things Float draws the spawn-value slider with a maximum of 500.
/// While that window is drawing, a slider whose max is 500 gets a max of 1,000,000 instead.
/// The stored MaxSpawnValue is left alone. 1,000,000 stays inside the int cast used for stack counts.
/// </summary>
[StaticConstructorOnStartup]
[HarmonyPatch(typeof(SomeThingsFloatMod), nameof(SomeThingsFloatMod.DoSettingsWindowContents))]
public static class Patch_SomeThingsFloat_MaxSpawnValue
{
    public const float RaisedSpawnCap = 1_000_000f;

    public static bool DrawingSettings;

    static Patch_SomeThingsFloat_MaxSpawnValue()
    {
        new Harmony("MrSamuelStreamer.rimworld.CircusFlavourPack.SomeThingsFloat")
            .PatchAll(typeof(Patch_SomeThingsFloat_MaxSpawnValue).Assembly);
        Patch_SomeThingsFloat_SpawnLoot.EnsurePool();
    }

    public static void Prefix() => DrawingSettings = true;

    public static Exception Finalizer(Exception __exception)
    {
        DrawingSettings = false;
        return __exception;
    }
}

/// <summary>
/// Raises only the spawn-value slider. Other sliders in that window use a different maximum.
/// </summary>
[HarmonyPatch(typeof(Listing_Standard), nameof(Listing_Standard.Slider), typeof(float), typeof(float), typeof(float))]
public static class Patch_SomeThingsFloat_SpawnValueSlider
{
    public static void Prefix(ref float max)
    {
        if (Patch_SomeThingsFloat_MaxSpawnValue.DrawingSettings && max == 500f)
            max = Patch_SomeThingsFloat_MaxSpawnValue.RaisedSpawnCap;
    }
}
