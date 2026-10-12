using HarmonyLib;
using Verse;

namespace CircusFlavourPack;

public class CircusFlavourPackMod : Mod
{
    public CircusFlavourPackMod(ModContentPack content) : base(content)
    {
#if DEBUG
        Harmony.DEBUG = true;
#endif
        Harmony harmony = new Harmony("MrSamuelStreamer.rimworld.CircusFlavourPack.main");
        harmony.PatchAll();
    }
}
