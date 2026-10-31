using RimWorld;
using Verse;

namespace CircusFlavourPack.Fireproofing;

public class CompProperties_AbilityRingOfFireRehearsal : CompProperties_AbilityEffect
{
    public CompProperties_AbilityRingOfFireRehearsal()
    {
        compClass = typeof(CompAbilityEffect_RingOfFireRehearsal);
    }
}

/// <summary>
/// Permanently fireproofs every floor cell and every player-owned structural building
/// (walls, doors, roof supports, fences) within the ability's effect radius of the caster.
/// </summary>
public class CompAbilityEffect_RingOfFireRehearsal : CompAbilityEffect
{
    public override void Apply(LocalTargetInfo target, LocalTargetInfo dest)
    {
        base.Apply(target, dest);
        Pawn caster = parent.pawn;
        Map map = caster.MapHeld;
        MapComponent_FireproofRegistry registry = MapComponent_FireproofRegistry.For(map);
        if (registry == null)
            return;

        int newBuildings = 0;
        int newCells = 0;
        foreach (IntVec3 cell in GenRadial.RadialCellsAround(caster.PositionHeld, parent.def.EffectRadius, true))
        {
            if (!cell.InBounds(map))
                continue;
            if (registry.Register(cell))
                newCells++;
            foreach (Thing thing in cell.GetThingList(map))
            {
                if (IsStructural(thing) && registry.Register(thing))
                    newBuildings++;
            }
        }

        FleckMaker.Static(caster.PositionHeld, map, FleckDefOf.PsycastAreaEffect, parent.def.EffectRadius);
        Messages.Message("MSS_CFP_RingOfFire_Applied".Translate(caster.Named("PAWN"), newBuildings, newCells),
            caster, MessageTypeDefOf.PositiveEvent);
    }

    private static bool IsStructural(Thing thing)
    {
        if (thing is not Building || thing.Faction != Faction.OfPlayerSilentFail)
            return false;
        ThingDef def = thing.def;
        return def.IsWall || def.IsDoor || def.holdsRoof || def.IsFence;
    }
}
