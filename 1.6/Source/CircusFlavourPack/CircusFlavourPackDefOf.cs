using RimWorld;
using Verse;

namespace CircusFlavourPack;

[DefOf]
public static class CircusFlavourPackDefOf
{
    // Prefix defNames with MSS_CFP_ — Cerebrex Flavour Pack already owns the bare CFP_ prefix.

    static CircusFlavourPackDefOf() => DefOfHelper.EnsureInitializedInCtor(typeof(CircusFlavourPackDefOf));
}
