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
            Command_Target rangedVerbGizmo = new VEF_Gizmo_SwitchRangedVerb(this.Pawn)
            {
                action = delegate (Thing target)
                {
                    IEnumerable<Pawn> selectedPawns = Find.Selector.SelectedObjects.Where(delegate (object x)
                    {
                        Pawn pawn = x as Pawn;
                        return pawn != null && pawn.IsColonistPlayerControlled && pawn.Drafted;
                    }).Cast<Pawn>();
                    foreach (Pawn pawn2 in selectedPawns)
                    {
                        string text;
                        VEF_FloatMenuUtility.GetAttackAction(pawn2, target, out text)?.Invoke();
                    }
                },
                disabled = true,
                disabledReason = "IsNotDrafted".Translate(this.Pawn.LabelShortCap, this.Pawn),
                hotKey = KeyBindingDefOf.Misc5,
                icon = BaseContent.BadTex,
                order = +1f,
                targetingParams = TargetingParameters.ForAttackAny(),
            };

            if (this.Pawn.IsColonist)
            {
                // Disabled conditions
                if (this.Pawn.Drafted && !Pawn.story.WorkTagIsDisabled(WorkTags.Violent))
                {
                    rangedVerbGizmo.disabled = false;
                }
                else if (Pawn.story.WorkTagIsDisabled(WorkTags.Violent))
                {
                    rangedVerbGizmo.disabledReason = "IsIncapableOfViolenceLower".Translate(pawn.LabelShort, pawn);
                }

                // Visible Conditions
                if (curRangedVerb == null || (rangedVerbs.Count == 1 && CurRangedVerb.EquipmentCompSource != null && curRangedVerb.EquipmentSource == Pawn.equipment.Primary) || (Pawn.story.WorkTagIsDisabled(WorkTags.Violent) && !pawn.Drafted) || AtLeastTwoSelectedColonistsHaveDifferentRangedVerbs())
                {
                    this.visible = false;
                }
                else
                {
                    this.visible = true;
                }

                // Label, Desc Icon and Squad Conditios
                if (ShouldUseSquadAttackGizmo())
                {
                    rangedVerbGizmo.defaultLabel = "CommandSquadAttack".Translate();
                    rangedVerbGizmo.defaultDesc = "CommandSquadAttackDesc".Translate();
                    rangedVerbGizmo.targetingParams = TargetingParameters.ForAttackAny();
                    rangedVerbGizmo.hotKey = KeyBindingDefOf.Misc2;
                    rangedVerbGizmo.icon = TexCommand.SquadAttack;
                    rangedVerbGizmo.action = delegate (Thing target)
                    {
                        IEnumerable<Pawn> enumerable = Find.Selector.SelectedObjects.Where(delegate (object x)
                        {
                            Pawn pawn3 = x as Pawn;
                            return pawn3 != null && pawn3.IsColonistPlayerControlled && pawn3.Drafted;
                        }).Cast<Pawn>();
                        foreach (Pawn pawn2 in enumerable)
                        {
                            FloatMenuUtility.GetAttackAction(pawn2, target, out string text)?.Invoke();
                        }
                    };
                    this.visible = true;
                }
                else if (CurRangedVerb != null)
                {
                    Texture2D tempIcon = BaseContent.BadTex;
                    if (CurRangedVerb.EquipmentSource != null)
                    {
                        if (VEF_ModCompatibilityCheck.rooloDualWield && CurRangedVerb.EquipmentSource == Pawn.equipment.Primary && VEF_ReflectedMethods.TryGetOffHandEquipment(Pawn.equipment, out ThingWithComps offHandEquipment))
                        {
                            rangedVerbGizmo.defaultDesc = (CurRangedVerb.verbProps.label == CurRangedVerb.EquipmentSource.def.label) ? CurRangedVerb.EquipmentSource.LabelCap + ": " + CurRangedVerb.EquipmentSource.DescriptionDetailed : CurRangedVerb.verbProps.label + " :: " + CurRangedVerb.EquipmentSource.LabelCap + ": " + CurRangedVerb.EquipmentSource.DescriptionDetailed;
                            tempIcon = CurRangedVerb.EquipmentSource.def.uiIcon;
                            if (tempIcon != BaseContent.BadTex || tempIcon != null)
                            {
                                rangedVerbGizmo.icon = tempIcon;
                            }
                        }
                        else
                        {
                            rangedVerbGizmo.defaultDesc = (CurRangedVerb.verbProps.label == CurRangedVerb.EquipmentSource.def.label) ? CurRangedVerb.EquipmentSource.LabelCap + ": " + CurRangedVerb.EquipmentSource.DescriptionDetailed : CurRangedVerb.verbProps.label + " :: " + CurRangedVerb.EquipmentSource.LabelCap + ": " + CurRangedVerb.EquipmentSource.DescriptionDetailed;
                            tempIcon = CurRangedVerb.EquipmentSource.def.uiIcon;
                            if (tempIcon != BaseContent.BadTex || tempIcon != null)
                            {
                                rangedVerbGizmo.icon = tempIcon;
                            }
                        }
                    }
                    else if (CurRangedVerb.verbProps.LaunchesProjectile)
                    {
                        if (CurRangedVerb.HediffCompSource != null)
                        {
                            rangedVerbGizmo.defaultDesc = (CurRangedVerb.verbProps.label == CurRangedVerb.HediffSource.def.label) ? CurRangedVerb.HediffCompSource.Def.LabelCap + ": " + CurRangedVerb.HediffCompSource.Def.description : CurRangedVerb.verbProps.label + " :: " + CurRangedVerb.HediffCompSource.Def.LabelCap + ": " + CurRangedVerb.HediffCompSource.Def.description;
                        }
                        else
                        {
                            rangedVerbGizmo.defaultDesc = (CurRangedVerb.verbProps.label == Pawn.def.label) ? "Biological weapon of " + this.Pawn.def.label + ": " + this.Pawn.def.description : CurRangedVerb.verbProps.label + " :: Biological weapon of " + this.Pawn.def.label + ": " + this.Pawn.def.description;
                        }
                        tempIcon = CurRangedVerb.GetProjectile().uiIcon;
                        if (tempIcon != BaseContent.BadTex || tempIcon != null)
                        {
                            rangedVerbGizmo.icon = tempIcon;
                        }
                    }
                }
            }
            else
            {
                this.visible = false;
            }
            yield return rangedVerbGizmo;
        }

        private Command_VerbTarget CreateVerbTargetCommand(Thing ownerThing, Verb verb)
        {
            Command_VerbTarget command_VerbTarget = new Command_VerbTarget();
            command_VerbTarget.defaultDesc = ownerThing.LabelCap + ": " + ownerThing.def.description.CapitalizeFirst();
            command_VerbTarget.icon = ownerThing.def.uiIcon;
            command_VerbTarget.iconAngle = ownerThing.def.uiIconAngle;
            command_VerbTarget.iconOffset = ownerThing.def.uiIconOffset;
            command_VerbTarget.tutorTag = "VerbTarget";
            command_VerbTarget.verb = verb;
            if (verb.caster.Faction != Faction.OfPlayer)
            {
                command_VerbTarget.Disable("CannotOrderNonControlled".Translate());
            }
            else if (verb.CasterIsPawn)
            {
                if (verb.CasterPawn.story.WorkTagIsDisabled(WorkTags.Violent))
                {
                    command_VerbTarget.Disable("IsIncapableOfViolence".Translate(verb.CasterPawn.LabelShort, verb.CasterPawn));
                }
                else if (!verb.CasterPawn.drafter.Drafted)
                {
                    command_VerbTarget.Disable("IsNotDrafted".Translate(verb.CasterPawn.LabelShort, verb.CasterPawn));
                }
            }
            return command_VerbTarget;
        }

        private Command_Target CreateSquadTargetCommand(Pawn pawn)
        {
			Command_Target command_Target = new Command_Target();
			command_Target.defaultLabel = "CommandSquadAttack".Translate();
			command_Target.defaultDesc = "CommandSquadAttackDesc".Translate();
			command_Target.targetingParams = TargetingParameters.ForAttackAny();
			command_Target.hotKey = KeyBindingDefOf.Misc1;
			command_Target.icon = TexCommand.SquadAttack;
			string str;
			if (FloatMenuUtility.GetAttackAction(pawn, LocalTargetInfo.Invalid, out str) == null)
			{
				command_Target.Disable(str.CapitalizeFirst() + ".");
			}
			command_Target.action = delegate(Thing target)
			{
				IEnumerable<Pawn> enumerable = Find.Selector.SelectedObjects.Where(delegate(object x)
				{
					Pawn pawn3 = x as Pawn;
					return pawn3 != null && pawn3.IsColonistPlayerControlled && pawn3.Drafted;
				}).Cast<Pawn>();
				foreach (Pawn pawn2 in enumerable)
				{
					string text;
					Action attackAction = FloatMenuUtility.GetAttackAction(pawn2, target, out text);
					if (attackAction != null)
					{
						attackAction();
					}
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
            List<VerbEntry> updatedAvailableVerbsList = getRangedVerbs;
            if (updatedAvailableVerbsList.NullOrEmpty())
            {
                SetCurRangedVerb(null, null);
            }
            else if (updatedAvailableVerbsList.Count == 1)
            {
                SetCurRangedVerb(updatedAvailableVerbsList[0].verb, target);
            }
            else
            {
                float highestScore = 0f;
                int highestScoreIndex = -1;
                
                if (target == null)
                {
                    for (int i = 0; i < updatedAvailableVerbsList.Count; i++)
                    {
                        float currentScore;

                        if (updatedAvailableVerbsList[i].verb.verbProps.spawnDef != null)
                        {
                            currentScore = 50 + i;
                        }
                        else
                        {
                            VerbEntry verbEntry = updatedAvailableVerbsList[i];
                            Verb v = verbEntry.verb;
                            ThingDef verbProjectile = v.GetProjectile();
                            int projectileDamageAmount = (v.EquipmentCompSource == null) ? verbProjectile.projectile.GetDamageAmount(1f) : verbProjectile.projectile.GetDamageAmount(v.EquipmentCompSource.parent);
                            List<float> accuracyList = (v.EquipmentCompSource == null) ? new List<float>() { v.verbProps.accuracyLong, v.verbProps.accuracyMedium, v.verbProps.accuracyShort, v.verbProps.accuracyTouch } : new List<float>() { v.EquipmentCompSource.parent.GetStatValue(StatDefOf.AccuracyLong), v.EquipmentCompSource.parent.GetStatValue(StatDefOf.AccuracyMedium), v.EquipmentCompSource.parent.GetStatValue(StatDefOf.AccuracyShort), v.EquipmentCompSource.parent.GetStatValue(StatDefOf.AccuracyTouch) };
                            accuracyList.Sort();
                            accuracyList.Reverse();
                            float accuracyValue = (accuracyList[0] + accuracyList[1]) / 2;
                            int burstShotCount = (v.verbProps.burstShotCount == 0) ? 1 : v.verbProps.burstShotCount;
                            float fullCycleTime = v.verbProps.AdjustedFullCycleTime(v, this.Pawn);

                            currentScore = ((accuracyValue * projectileDamageAmount * burstShotCount) / (fullCycleTime)) + v.GetProjectile().projectile.explosionRadius - (v.verbProps.forcedMissRadius / burstShotCount);
                        }

                        if (currentScore > highestScore)
                        {
                            highestScore = currentScore;
                            highestScoreIndex = i;
                        }
                    }
                }
                else
                {
                    for (int i = 0; i < updatedAvailableVerbsList.Count; i++)
                    {
                        float currentScore;

                        if (updatedAvailableVerbsList[i].verb.verbProps.spawnDef != null)
                        {
                            currentScore = 50 + i;
                        }
                        else
                        {
                            VerbEntry verbEntry = updatedAvailableVerbsList[i];
                            Verb v = verbEntry.verb;
                            ThingDef verbProjectile = v.GetProjectile();
                            int projectileDamageAmount = (v.EquipmentCompSource == null) ? verbProjectile.projectile.GetDamageAmount(1f) : verbProjectile.projectile.GetDamageAmount(v.EquipmentCompSource.parent);
                            float accuracyValue = Verse.ShotReport.HitReportFor(Pawn, v, target).TotalEstimatedHitChance;
                            int burstShotCount = (v.verbProps.burstShotCount == 0) ? 1 : v.verbProps.burstShotCount;
                            float fullCycleTime = v.verbProps.AdjustedFullCycleTime(v, this.Pawn);

                            currentScore = ((accuracyValue * projectileDamageAmount * burstShotCount) / (fullCycleTime)) + v.GetProjectile().projectile.explosionRadius - (v.verbProps.forcedMissRadius / burstShotCount);
                            if (target != null && v.IsIncendiary() && target.CanEverAttachFire() && !target.IsBurning())
                            {
                                currentScore += 10;
                            }
                            Pawn targetPawn = target as Pawn;
                            if (targetPawn != null && v.IsEMP() && !targetPawn.RaceProps.IsFlesh)
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
                                if (VEF_ModCompatibilityCheck.rooloDualWield)
                                {
                                    ThingWithComps offHandEquipment;
                                    VEF_ReflectedMethods.TryGetOffHandEquipment(Pawn.equipment, out offHandEquipment);
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
            foreach (Verb verb in this.Pawn.health.hediffSet.GetHediffsVerbs())
            {
                if (!verb.IsMeleeAttack && verb.IsStillUsableBy(this.Pawn))
                {
                    this.rangedVerbs.Add(new VerbEntry(verb, this.Pawn));
                }
            }
            return;
        }

        public static bool ShouldUseSquadAttackGizmo()
        {
            return AtLeastOneSelectedColonistHasOtherRangedVerb() && AtLeastTwoSelectedColonistsHaveDifferentRangedVerbs();
        }

        private static bool AtLeastOneSelectedColonistHasOtherRangedVerb()
        {
            List<object> selectedObjectsListForReading = Find.Selector.SelectedObjectsListForReading;
            for (int i = 0; i < selectedObjectsListForReading.Count; i++)
            {
                Pawn pawn = selectedObjectsListForReading[i] as Pawn;
                if (pawn != null && pawn.IsColonistPlayerControlled && !pawn.story.WorkTagIsDisabled(WorkTags.Violent))
                {
                    if (pawn.GetComp<VEF_Comp_Pawn_RangedVerbs>().CurRangedVerb != null && ( pawn.equipment.Primary == null || pawn.GetComp<VEF_Comp_Pawn_RangedVerbs>().CurRangedVerb.EquipmentSource != pawn.equipment.Primary))
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
                    else if (verbDef2 != verbDef)
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

        public List<VerbEntry> getRangedVerbs
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

        private const int BestRangedVerbUpdateInterval = 60;
    }
}
