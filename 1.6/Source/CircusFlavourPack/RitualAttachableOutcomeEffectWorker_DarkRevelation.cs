using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;

namespace CircusFlavourPack;

/// <summary>
/// Ritual reward that instantly finishes one Anomaly research project.
/// Prefers the next psychic ritual in the tree; has a configurable chance of other Anomaly research instead,
/// and falls back to other Anomaly research once every psychic ritual is known.
/// Only projects whose prerequisites are already done are picked, so FinishProject never chains unlocks.
/// Entity codex discovery is bypassed on purpose: the reward unlocks research without needing to study the entity first.
/// </summary>
public class RitualAttachableOutcomeEffectWorker_DarkRevelation : RitualAttachableOutcomeEffectWorker
{
    public override AcceptanceReport CanApplyNow(Precept_Ritual ritual, Map map)
    {
        if (!ModsConfig.AnomalyActive)
            return "MSS_CFP_DarkRevelation_NeedsAnomaly".Translate();
        if (Find.Anomaly.GenerateMonolith && Find.Anomaly.HighestLevelReached < 1)
            return "MSS_CFP_DarkRevelation_MonolithDormant".Translate();
        if (!RitualPool().Any() && !AnomalyPool().Any())
            return "MSS_CFP_DarkRevelation_NothingLeft".Translate();
        return true;
    }

    public override void Apply(Dictionary<Pawn, int> totalPresence, LordJob_Ritual jobRitual, RitualOutcomePossibility outcome,
        out string extraOutcomeDesc, ref LookTargets letterLookTargets)
    {
        extraOutcomeDesc = null;
        if (!ModsConfig.AnomalyActive)
            return;

        ResearchProjectDef project = PickProject();
        if (project == null)
        {
            extraOutcomeDesc = "MSS_CFP_DarkRevelation_NothingRevealed".Translate();
            return;
        }

        Find.ResearchManager.FinishProject(project, doCompletionLetter: false);
        extraOutcomeDesc = def.letterInfoText.Formatted(project.LabelCap).Resolve();
    }

    private static ResearchProjectDef PickProject()
    {
        List<ResearchProjectDef> rituals = RitualPool().ToList();
        List<ResearchProjectDef> anomaly = AnomalyPool().ToList();

        bool useAnomaly = rituals.Count == 0
                          || (anomaly.Count > 0 && Rand.Chance(CircusFlavourPackMod.settings.darkRevelationAnomalyChance));
        List<ResearchProjectDef> pool = useAnomaly ? anomaly : rituals;
        return pool.RandomElementWithFallback();
    }

    private static HashSet<ResearchProjectDef> RitualProjects() =>
        DefDatabase<PsychicRitualDef>.AllDefsListForReading
            .Where(r => r.playerCastable && r.researchPrerequisite != null)
            .Select(r => r.researchPrerequisite)
            .ToHashSet();

    private static IEnumerable<ResearchProjectDef> RitualPool() =>
        RitualProjects().Where(IsNextAvailable);

    private static IEnumerable<ResearchProjectDef> AnomalyPool()
    {
        HashSet<ResearchProjectDef> ritualProjects = RitualProjects();
        return DefDatabase<ResearchProjectDef>.AllDefsListForReading
            .Where(p => p.tab == ResearchTabDefOf.Anomaly && !ritualProjects.Contains(p) && IsNextAvailable(p));
    }

    /// <summary>
    /// Deliberately not ResearchProjectDef.IsHidden: that also hides projects whose entity codex entry is undiscovered,
    /// which emptied the pools after the first unlock. Only the "monolith not yet activated" block is kept.
    /// </summary>
    private static bool IsNextAvailable(ResearchProjectDef p) => !p.IsFinished && p.PrerequisitesCompleted && MonolithAllows(p);

    private static bool MonolithAllows(ResearchProjectDef p) =>
        p.knowledgeCategory == null || !Find.Anomaly.GenerateMonolith || Find.Anomaly.HighestLevelReached >= 1;
}
