using System.Collections.Generic;
using Verse;

namespace CircusFlavourPack.Fireproofing;

/// <summary>
/// Remembers which buildings and floor cells on a map were fireproofed by Ring of Fire Rehearsal.
/// Protection is permanent: buildings stay registered until destroyed, cells stay registered forever.
/// </summary>
public class MapComponent_FireproofRegistry : MapComponent
{
    public MapComponent_FireproofRegistry(Map map) : base(map)
    {
    }

    private HashSet<Thing> buildings = [];
    private HashSet<IntVec3> cells = [];

    // Fire code asks per cell and per thing many times a tick; Map.GetComponent scans a list, so cache the last map.
    private static Map cachedMap;
    private static MapComponent_FireproofRegistry cachedRegistry;

    public static MapComponent_FireproofRegistry For(Map map)
    {
        if (map == null)
            return null;
        if (map != cachedMap)
        {
            cachedRegistry = map.GetComponent<MapComponent_FireproofRegistry>();
            cachedMap = map;
        }
        return cachedRegistry;
    }

    public bool IsFireproof(Thing thing) => buildings.Count > 0 && buildings.Contains(thing);

    public bool IsFireproof(IntVec3 cell) => cells.Count > 0 && cells.Contains(cell);

    public bool Register(Thing thing) => buildings.Add(thing);

    public bool Register(IntVec3 cell) => cells.Add(cell);

    public override void ExposeData()
    {
        base.ExposeData();
        if (Scribe.mode == LoadSaveMode.Saving)
            buildings.RemoveWhere(t => t == null || t.Destroyed);

        Scribe_Collections.Look(ref buildings, "buildings", LookMode.Reference);
        Scribe_Collections.Look(ref cells, "cells", LookMode.Value);

        if (Scribe.mode == LoadSaveMode.PostLoadInit)
        {
            buildings ??= [];
            cells ??= [];
            buildings.RemoveWhere(t => t == null);
        }
    }
}
