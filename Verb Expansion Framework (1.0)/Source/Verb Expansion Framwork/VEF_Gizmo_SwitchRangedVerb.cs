using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;
using Verse.Sound;
using UnityEngine;

namespace VerbExpansionFramework
{
    [StaticConstructorOnStartup]
    public class VEF_Gizmo_SwitchRangedVerb : Command_Target
    {
        public VEF_Gizmo_SwitchRangedVerb(Pawn pawn)
        {
            this.pawn = pawn;
            UpdateCurRangedVerb();
        }

        public override bool Visible
        {
            get
            {
                return this.pawn.GetComp<VEF_Comp_Pawn_RangedVerbs>().visible;
            }
        }


        public override Color IconDrawColor
        {
            get
            {
                if (verb != null && verb.EquipmentSource != null)
                {
                    return verb.EquipmentSource.DrawColor;
                }
                return base.IconDrawColor;
            }
        }

        public override GizmoResult GizmoOnGUI(Vector2 topLeft, float maxWidth)
        {
            GizmoResult result = base.GizmoOnGUI(topLeft, maxWidth);
            if (VEF_ModCompatibilityCheck.rooloDualWield && verb.EquipmentSource != null && verb.EquipmentSource.def == pawn.equipment.Primary.def && !VEF_Comp_Pawn_RangedVerbs.ShouldUseSquadAttackGizmo())
            {
                VEF_ReflectedMethods.TryGetOffHandEquipment(this.pawn.equipment, out ThingWithComps offHandThing);
                if (offHandThing != null)
                {
                    GUI.color = offHandThing.DrawColor;
                    Material mat = (!this.disabled) ? null : TexUI.GrayscaleGUI;
                    Texture2D texture2D = offHandThing.def.uiIcon;
                    bool flag = texture2D == null;
                    if (flag)
                    {
                        texture2D = BaseContent.BadTex;
                    }
                    Rect outerRect = new Rect(topLeft.x, topLeft.y + 10f, this.GetWidth(maxWidth), 75f);
                    Widgets.DrawTextureFitted(outerRect, texture2D, this.iconDrawScale * 0.85f, this.iconProportions, this.iconTexCoords, this.iconAngle, mat);
                    GUI.color = Color.white;
                }
            }
            return result;
        }

        public override void GizmoUpdateOnMouseover()
        {
            if (verb != null)
            {
                verb.verbProps.DrawRadiusRing(this.pawn.Position);
                if (!groupedVerbs.NullOrEmpty<Verb>())
                {
                    foreach (Verb verb in groupedVerbs)
                    {
                        verb.verbProps.DrawRadiusRing(verb.caster.Position);
                    }
                }
            }
        }

        public override void MergeWith(Gizmo other)
        {
            if (!(other is VEF_Gizmo_SwitchRangedVerb merge_TargetVerb))
            {
                Log.ErrorOnce("Tried to merge Command_VerbTarget with unexpected type", 73406263, false);
                return;
            }
            if (groupedVerbs == null)
            {
                groupedVerbs = new List<Verb>();
            }
            groupedVerbs.Add(merge_TargetVerb.verb);
            if (merge_TargetVerb.groupedVerbs != null)
            {
                groupedVerbs.AddRange(merge_TargetVerb.groupedVerbs);
            }
        }

        public override void ProcessInput(Event ev)
        {
            this.tutorTag = "VerbSelection";
            base.ProcessInput(ev);
            SoundDefOf.Tick_Tiny.PlayOneShotOnCamera(null);

            UpdateCurRangedVerb();
            UpdateAllRangedVerbs();
            if (this.verb != null)
            {
                if (allRangedVerbs.Count > 1 && GetSelectedPawns().Count == 1)
                {
                    Find.WindowStack.Add(CreateRangedVerbsMenu());
                }
                Find.Targeter.BeginTargeting(targetingParams, delegate (LocalTargetInfo target)
                {
                    this.action(target.Thing);
                }, null, null, null);
            }
        }

        private FloatMenu CreateRangedVerbsMenu()
        {
            List<FloatMenuOption> floatOptionList = new List<FloatMenuOption>();

            foreach (VerbEntry verbEntry in allRangedVerbs)
            {
                string verbLabel = null;

                void onSelectVerb()
                {
                    this.verb = verbEntry.verb;
                    pawn.GetComp<VEF_Comp_Pawn_RangedVerbs>().SetCurRangedVerb(this.verb, null);
                    Find.Targeter.StopTargeting();
                }

                if (verbEntry.verb.EquipmentCompSource != null)
                {
                    verbEntry.verb.EquipmentSource.TryGetQuality(out QualityCategory qualityString);
                    verbLabel = (verbEntry.verb.verbProps.label == verbEntry.verb.EquipmentSource.def.label) ? verbEntry.verb.EquipmentSource.LabelCap : verbEntry.verb.verbProps.label + " " + qualityString;
                }
                else if (verbEntry.verb.HediffCompSource != null)
                {
                    verbLabel = (verbEntry.verb.verbProps.label == verbEntry.verb.HediffSource.def.label) ? verbEntry.verb.HediffCompSource.Def.LabelCap : verbEntry.verb.verbProps.label;
                }
                else
                {
                    verbLabel = (verbEntry.verb.verbProps.label == this.pawn.def.label) ? "Race-Defined weapon of " + this.pawn.def.label : verbEntry.verb.verbProps.label + ": " + "Race-Defined weapon of " + this.pawn.def.label;
                }

                floatOptionList.Add(new FloatMenuOption(verbLabel, onSelectVerb));
            }

            return new FloatMenu(floatOptionList);
        }

        private void UpdateCurRangedVerb()
        {
            this.verb = this.pawn.GetComp<VEF_Comp_Pawn_RangedVerbs>().TryGetRangedVerb(null);
        }

        private void UpdateAllRangedVerbs()
        {
            this.allRangedVerbs = this.pawn.GetComp<VEF_Comp_Pawn_RangedVerbs>().getRangedVerbs;
        }

        private List<Pawn> GetSelectedPawns()
        {
            IEnumerable<Pawn> selectedPawnsIEnum = Find.Selector.SelectedObjects.Where(delegate (object x)
            {
                Pawn pawn3 = x as Pawn;
                return pawn3 != null && pawn3.IsColonistPlayerControlled && pawn3.Drafted;
            }).Cast<Pawn>();
            return selectedPawnsIEnum.ToList();
        }

        public Verb verb;

        private Pawn pawn;

        private List<VerbEntry> allRangedVerbs;

        private List<Verb> groupedVerbs;
    }
}
