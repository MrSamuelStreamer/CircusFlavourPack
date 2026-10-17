using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;

namespace CircusFlavourPack;

public class Settings : ModSettings
{
    /// <summary>Swap a pawn's xenotype when it joins the player faction as a colonist.</summary>
    public bool xenotypeOnJoinEnabled = false;

    /// <summary>defName of the XenotypeDef applied on joining. Only non-inheritable xenotypes are allowed.</summary>
    public string joinXenotypeDefName;

    /// <summary>
    /// The configured xenotype, or null when unset, no longer loaded, or inheritable.
    /// Inheritable xenotypes are rejected because SetXenotype only clears xenogenes, so germline genes would stack on the pawn's endogenes.
    /// </summary>
    public XenotypeDef JoinXenotype
    {
        get
        {
            if (joinXenotypeDefName.NullOrEmpty())
                return null;
            XenotypeDef def = DefDatabase<XenotypeDef>.GetNamedSilentFail(joinXenotypeDefName);
            return def is { inheritable: false } ? def : null;
        }
    }

    public void DoWindowContents(Rect wrect)
    {
        Listing_Standard options = new();
        options.Begin(wrect);

        options.CheckboxLabeled(
            "MSS_CFP_Settings_XenotypeOnJoin".Translate(), ref xenotypeOnJoinEnabled,
            "MSS_CFP_Settings_XenotypeOnJoin_Tooltip".Translate());
        options.Gap();

        if (options.ButtonTextLabeled(
                "MSS_CFP_Settings_JoinXenotype".Translate(), CurrentXenotypeLabel(),
                tooltip: "MSS_CFP_Settings_JoinXenotype_Tooltip".Translate()))
            Find.WindowStack.Add(new FloatMenu(XenotypeOptions()));

        options.End();
    }

    private string CurrentXenotypeLabel()
    {
        if (joinXenotypeDefName.NullOrEmpty())
            return "MSS_CFP_Settings_XenotypeNone".Translate();
        XenotypeDef def = DefDatabase<XenotypeDef>.GetNamedSilentFail(joinXenotypeDefName);
        if (def == null)
            return "MSS_CFP_Settings_XenotypeMissing".Translate(joinXenotypeDefName);
        if (def.inheritable)
            return "MSS_CFP_Settings_XenotypeInheritable".Translate(def.LabelCap);
        return def.LabelCap;
    }

    private List<FloatMenuOption> XenotypeOptions()
    {
        List<FloatMenuOption> list = [new("MSS_CFP_Settings_XenotypeNone".Translate(), () => joinXenotypeDefName = null)];
        list.AddRange(DefDatabase<XenotypeDef>.AllDefsListForReading
            .Where(def => !def.inheritable)
            .OrderBy(def => def.label)
            .Select(def => new FloatMenuOption(def.LabelCap, () => joinXenotypeDefName = def.defName, def.Icon, Color.white)));
        return list;
    }

    public override void ExposeData()
    {
        // Save keys keep the old "conversion" names so existing settings files still load.
        Scribe_Values.Look(ref xenotypeOnJoinEnabled, "xenotypeOnConversionEnabled", false);
        Scribe_Values.Look(ref joinXenotypeDefName, "conversionXenotypeDefName");
    }
}
