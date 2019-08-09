using System;
using System.Collections.Generic;
using System.Linq;
using Verse;
using RimWorld;
using UnityEngine;

namespace VerbExpansionFramework
{
    public class VEF_Comp_Pawn_RangedVerbs : ThingComp
    {

        public Pawn Pawn
        {
            get
            {
                return this.pawn ?? (this.pawn = (Pawn) this.parent);
            }
        }

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            if (respawningAfterLoad == false)
            {
                TryGetRangedVerb(null);
            }
            else
            {
                UpdateRangedVerbs();
            }
        }

        public override void CompTick()
        {
            if (this.Pawn.Spawned && this.Pawn.IsHashIntervalTick(60))
            {
                TryGetRangedVerb(curRangedVerbTarget);
            }
        }

        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            if (AtLeastOnePawnHasRangedVerb())
            {
                Gizmo rangedVerbGizmo;
                if (ShouldUseSquadAttackGizmo() && !VEF_ModCompatibilityCheck.enabled_CombatExtended)
                {
                    rangedVerbGizmo = CreateSquadAttackGizmo(Pawn);
                }
                else
                {
                    rangedVerbGizmo = CreateVerbTargetCommand(CurRangedVerb);
                }
                yield return rangedVerbGizmo;
            }
        }

        private Command_VerbTarget CreateVerbTargetCommand(Verb verb)
        {
            VEF_Gizmo_SwitchRangedVerb command_VerbTarget = new VEF_Gizmo_SwitchRangedVerb(Pawn)
            {
                tutorTag = "VerbTarget",
                hotKey = KeyBindingDefOf.Misc5,
                icon = BaseContent.BadTex,
                order = +1f,
                verb = verb
            };
            if (verb != null && verb.IsStillUsableBy(Pawn))
            {
                if (verb.caster.Faction != Faction.OfPlayer)
                {
                    command_VerbTarget.Disable("CannotOrderNonControlled".Translate());
                }
                else if (verb.CasterIsPawn)
                {
                    // Disables Conditions
                    if (verb.CasterPawn.story.WorkTagIsDisabled(WorkTags.Violent))
                    {
                        command_VerbTarget.Disable("IsIncapableOfViolence".Translate(verb.CasterPawn.LabelShort, verb.CasterPawn));
                    }
                    else if (!verb.CasterPawn.drafter.Drafted)
                    {
                        command_VerbTarget.Disable("IsNotDrafted".Translate(verb.CasterPawn.LabelShort, verb.CasterPawn));
                    }

                    // Visible Conditions
                    if (verb == null || (rangedVerbs.Count == 1 && verb.EquipmentSource != null && verb.EquipmentSource == verb.CasterPawn.equipment.Primary) || (verb.CasterPawn.story.WorkTagIsDisabled(WorkTags.Violent) && !verb.CasterPawn.Drafted))
                    {
                        this.visible = false;
                    }
                    else
                    {
                        this.visible = true;
                    }

                    //Description, Icon and Label Conditions
                    Texture2D tempIcon = BaseContent.BadTex;
                    if (CurRangedVerb.EquipmentSource != null)
                    {
                        command_VerbTarget.defaultDesc = (verb.verbProps.label == verb.EquipmentSource.def.label) ? verb.EquipmentSource.LabelCap + ": " + verb.EquipmentSource.def.description.CapitalizeFirst() : verb.verbProps.label + " :: " + verb.EquipmentSource.LabelCap + ": " + verb.EquipmentSource.def.description.CapitalizeFirst();
                        tempIcon = CurRangedVerb.EquipmentSource.def.uiIcon;
                        if (tempIcon != BaseContent.BadTex || tempIcon != null)
                        {
                            command_VerbTarget.icon = tempIcon;
                        }
                    }
                    else if (verb.verbProps.LaunchesProjectile)
                    {
                        if (verb.HediffCompSource != null)
                        {
                            command_VerbTarget.defaultDesc = (verb.verbProps.label == verb.HediffSource.def.label) ? verb.HediffCompSource.Def.LabelCap + ": " + verb.HediffSource.def.description.CapitalizeFirst() : verb.verbProps.label + " :: " + verb.HediffSource.LabelCap + ": " + verb.HediffSource.def.description.CapitalizeFirst();
                        }
                        else
                        {
                            command_VerbTarget.defaultDesc = (verb.verbProps.label == verb.CasterPawn.def.label) ? "Biological weapon of " + verb.CasterPawn.def.label + ": " + verb.CasterPawn.def.description.CapitalizeFirst() : CurRangedVerb.verbProps.label.CapitalizeFirst() + " :: Biological weapon of " + verb.CasterPawn.def.label + ": " + verb.CasterPawn.def.description.CapitalizeFirst();
                        }
                        tempIcon = verb.GetProjectile().uiIcon;
                        if (tempIcon != BaseContent.BadTex || tempIcon != null)
                        {
                            command_VerbTarget.icon = tempIcon;
                        }
                    }
                }
            }
            else
            {
                this.visible = false;
                command_VerbTarget.Disable();
            }
            return command_VerbTarget;
        }

        private static Gizmo CreateSquadAttackGizmo(Pawn pawn)
        {
            Command_Target command_Target = new Command_Target();
            command_Target.defaultLabel = "CommandSquadAttack".Translate();
            command_Target.defaultDesc = "CommandSquadAttackDesc".Translate();
            command_Target.targetingParams = TargetingParameters.ForAttackAny();
            command_Target.hotKey = KeyBindingDefOf.Misc2;
            command_Target.icon = TexCommand.SquadAttack;
            string str;
            if (FloatMenuUtility.GetAttackAction(pawn, LocalTargetInfo.Invalid, out str) == null)
            {
                command_Target.Disable(str.CapitalizeFirst() + ".");
            }
            command_Target.action = delegate (Thing target)
            {
                IEnumerable<Pawn> enumerable = Find.Selector.SelectedObjects.Where(delegate (object x)
                {
                    return x is Pawn pawn3 && pawn3.IsColonistPlayerControlled && pawn3.Drafted;
                }).Cast<Pawn>();
                foreach (Pawn pawn2 in enumerable)
                {
                    FloatMenuUtility.GetAttackAction(pawn2, target, out string text)?.Invoke();
                }
            };
            return command_Target;
        }

        public Verb TryGetRangedVerb(Thing target)
        {
            UpdateRangedVerbs();
            if (this.curRangedVerb == null || !this.curRangedVerb.IsStillUsableBy(this.Pawn))
            {
                ChooseRangedVerb(target);
            }
            else if (!this.Pawn.IsColonist || this.Pawn.InMentalState)
            {
                if (this.curRangedVerbTarget != target || Find.TickManager.TicksGame >= this.curRangedVerbUpdateTick + 60 || !this.curRangedVerb.IsStillUsableBy(this.Pawn) || !this.curRangedVerb.IsUsableOn(target))
                {
                    ChooseRangedVerb(target);
                }
            }
            return this.curRangedVerb;
        }

        private void ChooseRangedVerb(Thing target)
        {
            List<VerbEntry> updatedAvailableVerbsList = GetRangedVerbs;
            if (updatedAvailableVerbsList.NullOrEmpty())
            {
                SetCurRangedVerb(null, null);
            }
            else if (updatedAvailableVerbsList.Count == 1 || VEF_ModCompatibilityCheck.enabled_CombatExtended)
            {
                SetCurRangedVerb(updatedAvailableVerbsList[0].verb, target);
            }
            else
            {
                float highestScore = 0f;
                int highestScoreIndex = -1;

                for (int i = 0; i < updatedAvailableVerbsList.Count; i++)
                {
                    Verb verb = updatedAvailableVerbsList[i].verb;
                    float currentScore;

                    if (target == null)
                    {

                        if (verb.verbProps.spawnDef != null)
                        {
                            currentScore = 50 + i;
                        }
                        else
                        {
                            ThingDef verbProjectile = verb.GetProjectile();
                            int projectileDamageAmount = (verb.EquipmentCompSource == null) ? verbProjectile.projectile.GetDamageAmount(1f) : verbProjectile.projectile.GetDamageAmount(verb.EquipmentSource);
                            List<float> accuracyList = (verb.EquipmentCompSource == null) ? new List<float>() { verb.verbProps.accuracyLong, verb.verbProps.accuracyMedium, verb.verbProps.accuracyShort } : new List<float>() { verb.EquipmentCompSource.parent.GetStatValue(StatDefOf.AccuracyLong), verb.EquipmentCompSource.parent.GetStatValue(StatDefOf.AccuracyMedium), verb.EquipmentCompSource.parent.GetStatValue(StatDefOf.AccuracyShort) };
                            accuracyList.Sort();
                            accuracyList.Reverse();
                            float accuracyValue = (accuracyList[0] + accuracyList[1]) / 2;
                            int burstShotCount = (verb.verbProps.burstShotCount == 0) ? 1 : verb.verbProps.burstShotCount;
                            float fullCycleTime = verb.verbProps.AdjustedFullCycleTime(verb, this.Pawn);

                            currentScore = ((accuracyValue * projectileDamageAmount * burstShotCount) / (fullCycleTime)) + verbProjectile.projectile.explosionRadius - (verb.verbProps.forcedMissRadius / burstShotCount);
                        }

                        if (currentScore > highestScore)
                        {
                            highestScore = currentScore;
                            highestScoreIndex = i;
                        }
                    }
                    else
                    {
                        if (verb.verbProps.spawnDef != null)
                        {
                            currentScore = 50 + i;
                        }
                        else
                        {
                            ThingDef verbProjectile = verb.GetProjectile();
                            int projectileDamageAmount = (verb.EquipmentCompSource == null) ? verbProjectile.projectile.GetDamageAmount(1f) : verbProjectile.projectile.GetDamageAmount(verb.EquipmentSource);
                            float accuracyValue = Verse.ShotReport.HitReportFor(Pawn, verb, target).TotalEstimatedHitChance;
                            int burstShotCount = (verb.verbProps.burstShotCount == 0) ? 1 : verb.verbProps.burstShotCount;
                            float fullCycleTime = verb.verbProps.AdjustedFullCycleTime(verb, this.Pawn);

                            currentScore = ((accuracyValue * projectileDamageAmount * burstShotCount) / (fullCycleTime)) + verbProjectile.projectile.explosionRadius - (verb.verbProps.forcedMissRadius / burstShotCount);
                            if (verb.IsIncendiary() && target.CanEverAttachFire() && !target.IsBurning())
                            {
                                currentScore += 10;
                            }
                            Pawn targetPawn = target as Pawn;
                            if (targetPawn != null && verb.IsEMP() && !targetPawn.RaceProps.IsFlesh)
                            {
                                currentScore += 10;
                            }
                        }

                        if (currentScore > highestScore)
                        {
                            highestScore = currentScore;
                            highestScoreIndex = i;
                        }
                    }
                }

                if (highestScoreIndex != -1)
                {
                    SetCurRangedVerb(updatedAvailableVerbsList[highestScoreIndex].verb, target);
                }
            }
        }

        public void UpdateRangedVerbs()
        {
            this.rangedVerbs.Clear();
            List<Verb> allVerbs = this.Pawn.verbTracker.AllVerbs;
            if (!allVerbs.NullOrEmpty())
            {
                for (int i = 0; i < allVerbs.Count; i++)
                {
                    if (!allVerbs[i].IsMeleeAttack && allVerbs[i].IsStillUsableBy(this.Pawn))
                    {
                        this.rangedVerbs.Add(new VerbEntry(allVerbs[i], this.Pawn));
                    }
                }
            }
            if (this.Pawn.equipment != null)
            {
                List<ThingWithComps> allEquipmentListForReading = this.Pawn.equipment.AllEquipmentListForReading;
                for (int j = 0; j < allEquipmentListForReading.Count; j++)
                {
                    ThingWithComps thingWithComps = allEquipmentListForReading[j];
                    CompEquippable equipmentComp = thingWithComps.GetComp<CompEquippable>();
                    if (equipmentComp != null)
                    {
                        List<Verb> allEquipmentVerbs = equipmentComp.AllVerbs;
                        if (allEquipmentVerbs != null)
                        {
                            for (int k = 0; k < allEquipmentVerbs.Count; k++)
                            {
                                if (VEF_ModCompatibilityCheck.enabled_rooloDualWield)
                                {
                                    try
                                    {
                                        ((Action)(() =>
                                        {
                                            ThingWithComps offHandEquipment = DualWield.Ext_Pawn.TryGetOffhandAttackVerb(Pawn, Pawn.mindState.enemyTarget, !Pawn.IsColonist).EquipmentSource;
                                            if (offHandEquipment != null)
                                            {
                                                if (!allEquipmentVerbs[k].IsMeleeAttack && allEquipmentVerbs[k].EquipmentSource != offHandEquipment && allEquipmentVerbs[k].IsStillUsableBy(this.Pawn))
                                                {
                                                    this.rangedVerbs.Add(new VerbEntry(allEquipmentVerbs[k], this.Pawn));
                                                }
                                            }
                                            else
                                            {
                                                if (!allEquipmentVerbs[k].IsMeleeAttack && allEquipmentVerbs[k].IsStillUsableBy(this.Pawn))
                                                {
                                                    this.rangedVerbs.Add(new VerbEntry(allEquipmentVerbs[k], this.Pawn));
                                                }
                                            }
                                        }))();
                                    }
                                    catch (TypeLoadException ex)
                                    {

                                    }
                                }
                                else if (VEF_ModCompatibilityCheck.enabled_CombatExtended)
                                {
                                    try
                                    {
                                        ((Action)(() =>
                                        {
                                            if (allEquipmentVerbs[k].verbProps.GetType().Equals(typeof(CombatExtended.VerbPropertiesCE)) && !allEquipmentVerbs[k].IsMeleeAttack && allEquipmentVerbs[k].IsStillUsableBy(this.Pawn) && allEquipmentVerbs[k].EquipmentSource.TryGetComp<CombatExtended.CompAmmoUser>() == null || allEquipmentVerbs[k].EquipmentSource.TryGetComp<CombatExtended.CompAmmoUser>().CanBeFiredNow)
                                            {
                                                this.rangedVerbs.Add(new VerbEntry(allEquipmentVerbs[k], this.Pawn));
                                            }
                                        }))();
                                    }
                                    catch (TypeLoadException ex)
                                    {

                                    }
                                }
                                else
                                {
                                    if (!allEquipmentVerbs[k].IsMeleeAttack && allEquipmentVerbs[k].IsStillUsableBy(this.Pawn))
                                    {
                                        this.rangedVerbs.Add(new VerbEntry(allEquipmentVerbs[k], this.Pawn));
                                    }
                                }
                            }
                        }
                    }
                }
            }
            if (!VEF_ModCompatibilityCheck.enabled_CombatExtended)
            {
                foreach (Verb verb in this.Pawn.health.hediffSet.GetHediffsVerbs())
                {
                    if (!verb.IsMeleeAttack && verb.IsStillUsableBy(this.Pawn))
                    {
                        this.rangedVerbs.Add(new VerbEntry(verb, this.Pawn));
                    }
                }
                return;
            }
        }

        public static bool ShouldUseSquadAttackGizmo()
        {
            return AtLeastOneSelectedColonistHasOtherRangedVerb() && AtLeastTwoSelectedColonistsHaveDifferentRangedVerbs();
        }

        private static bool AtLeastOnePawnHasRangedVerb()
        {
            {
                List<object> selectedObjectsListForReading = Find.Selector.SelectedObjectsListForReading;
                for (int i = 0; i < selectedObjectsListForReading.Count; i++)
                {
                    Pawn pawn = selectedObjectsListForReading[i] as Pawn;
                    if (pawn != null && pawn.IsColonistPlayerControlled && !pawn.story.WorkTagIsDisabled(WorkTags.Violent))
                    {
                        if (pawn.GetComp<VEF_Comp_Pawn_RangedVerbs>().CurRangedVerb != null && pawn.GetComp<VEF_Comp_Pawn_RangedVerbs>().CurRangedVerb.IsStillUsableBy(pawn))
                        {
                            return true;
                        }
                    }
                }
            }
            return false;
        }

        private static bool AtLeastOneSelectedColonistHasOtherRangedVerb()
        {
            if (Find.Selector.NumSelected <= 1)
            {
                return false;
            }
            List<object> selectedObjectsListForReading = Find.Selector.SelectedObjectsListForReading;
            for (int i = 0; i < selectedObjectsListForReading.Count; i++)
            {
                Pawn pawn = selectedObjectsListForReading[i] as Pawn;
                if (pawn != null && pawn.IsColonistPlayerControlled && !pawn.story.WorkTagIsDisabled(WorkTags.Violent))
                {
                    if (pawn.GetComp<VEF_Comp_Pawn_RangedVerbs>().GetRangedVerbs.Count > 1)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        private static bool AtLeastTwoSelectedColonistsHaveDifferentRangedVerbs()
        {
            if (Find.Selector.NumSelected <= 1)
            {
                return false;
            }
            VerbProperties verbDef = null;
            bool flag = false;
            List<object> selectedObjectsListForReading = Find.Selector.SelectedObjectsListForReading;
            for (int i = 0; i < selectedObjectsListForReading.Count; i++)
            {
                Pawn pawn = selectedObjectsListForReading[i] as Pawn;
                if (pawn != null && pawn.IsColonistPlayerControlled && !pawn.story.WorkTagIsDisabled(WorkTags.Violent))
                {
                    VerbProperties verbDef2;
                    if (pawn.GetComp<VEF_Comp_Pawn_RangedVerbs>().CurRangedVerb == null)
                    {
                        verbDef2 = null;
                    }
                    else
                    {
                        verbDef2 = pawn.GetComp<VEF_Comp_Pawn_RangedVerbs>().CurRangedVerb.verbProps;
                    }
                    if (!flag)
                    {
                        verbDef = verbDef2;
                        flag = true;
                    }
                    else if (verbDef2 != null && (verbDef2 != verbDef))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public Verb CurRangedVerb
        {
            get
            {
                return curRangedVerb;
            }
        }

        public List<VerbEntry> GetRangedVerbs
        {
            get
            {
                UpdateRangedVerbs();
                return this.rangedVerbs;
            }
        }

        public void SetCurRangedVerb(Verb v, Thing target)
        {
            this.curRangedVerb = v;
            this.curRangedVerbTarget = target;
            if (Current.ProgramState != ProgramState.Playing)
            {
                this.curRangedVerbUpdateTick = 0;
            }
            else
            {
                this.curRangedVerbUpdateTick = Find.TickManager.TicksGame;
            }
        }

        public void ExposeData()
        {
            if (Scribe.mode == LoadSaveMode.Saving && this.curRangedVerb != null && !this.curRangedVerb.IsStillUsableBy(this.Pawn))
            {
                this.curRangedVerb = null;
            }
            Scribe_References.Look<Verb>(ref this.curRangedVerb, "curRangedVerb", false);
            Scribe_Values.Look<int>(ref this.curRangedVerbUpdateTick, "curRangedVerbUpdateTick", 0, false);
            Scribe_Values.Look<int>(ref this.Pawn.meleeVerbs.lastTerrainBasedVerbUseTick, "lastTerrainBasedVerbUseTick", -99999, false);
            if (Scribe.mode == LoadSaveMode.PostLoadInit && this.curRangedVerb != null && this.curRangedVerb.BuggedAfterLoading)
            {
                this.curRangedVerb = null;
                Log.Warning(this.Pawn.ToStringSafe<Pawn>() + " had a bugged ranged verb after loading.", false);
            }
        }

        private Pawn pawn;

        private Verb curRangedVerb;

        private Thing curRangedVerbTarget;

        public bool visible = false;

        private int curRangedVerbUpdateTick;

        private List<VerbEntry> rangedVerbs = new List<VerbEntry>();
    }
}
