using RimWorld;
using Verse;

namespace CircusFlavourPack.Fireproofing;

/// <summary>Sets Flammability to zero for buildings fireproofed by Ring of Fire Rehearsal.</summary>
public class StatPart_Fireproofed : StatPart
{
    public override void TransformValue(StatRequest req, ref float val)
    {
        if (IsFireproofed(req))
            val = 0f;
    }

    public override string ExplanationPart(StatRequest req)
    {
        return IsFireproofed(req) ? "MSS_CFP_RingOfFire_StatExplanation".Translate().Resolve() : null;
    }

    private static bool IsFireproofed(StatRequest req)
    {
        Thing thing = req.Thing;
        if (thing is not Building)
            return false;
        return MapComponent_FireproofRegistry.For(thing.MapHeld)?.IsFireproof(thing) ?? false;
    }
}
