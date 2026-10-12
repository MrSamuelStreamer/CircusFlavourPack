using HarmonyLib;
using UnityEngine;
using Verse;

namespace CircusFlavourPack;

public class CircusFlavourPackMod : Mod
{
    public static Settings settings;

    public CircusFlavourPackMod(ModContentPack content) : base(content)
    {
        settings = GetSettings<Settings>();
#if DEBUG
        Harmony.DEBUG = true;
#endif
        Harmony harmony = new Harmony("MrSamuelStreamer.rimworld.CircusFlavourPack.main");
        harmony.PatchAll();
    }

    public override void DoSettingsWindowContents(Rect inRect)
    {
        base.DoSettingsWindowContents(inRect);
        settings.DoWindowContents(inRect);
    }

    public override string SettingsCategory()
    {
        return "MSS_CFP_SettingsCategory".Translate();
    }
}
