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
    public class VEF_Gizmo_SwitchRangedVerb : Command_VerbTarget
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
            if (VEF_ModCompatibilityCheck.enabled_rooloDualWield && verb.EquipmentSource != null && verb.EquipmentSource.def == pawn.equipment.Primary.def && !VEF_Comp_Pawn_RangedVerbs.ShouldUseSquadAttackGizmo())
            {
                try
                {
                    ((Action)(() =>
                    {
                        object[] parameters = new object[] { pawn.equipment, null };
                        bool hasOffHandEquipment = (bool)VEF_ReflectionData.MB_TryGetOffHandEquipment.Invoke(null, parameters);
                        ThingWithComps offHandThing = (ThingWithComps)parameters[1];
                        if (offHandThing != null && offHandThing.GetComp<CompEquippable>() != null && !offHandThing.GetComp<CompEquippable>().AllVerbs.NullOrEmpty())
                        {
                                GUI.color = offHandThing.DrawColor;
                                Material mat = (!this.disabled) ? null : TexUI.GrayscaleGUI;
                                Texture2D texture2D = offHandThing.def.uiIcon;
                                if (texture2D == null)
                                {
                                    texture2D = BaseContent.BadTex;
                                }
                                Rect outerRect = new Rect(topLeft.x, topLeft.y + 10f, this.GetWidth(maxWidth), 75f);
                                Widgets.DrawTextureFitted(outerRect, texture2D, this.iconDrawScale * 0.85f, this.iconProportions, this.iconTexCoords, this.iconAngle, mat);
                                GUI.color = Color.white;
                        }
                    }))();
                }
                catch (TypeLoadException ex)
                {

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

        public override void ProcessInput(Event ev)
        {
            UpdateCurRangedVerb();
            UpdateAllRangedVerbs();

            this.tutorTag = "VerbSelection";
            base.ProcessInput(ev);

            if (allRangedVerbs.Count > 1 && GetSelectedPawns().Count == 1)
            {
                Find.WindowStack.Add(CreateRangedVerbsMenu());
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
                    verbLabel = (verbEntry.verb.verbProps.label == verbEntry.verb.EquipmentSource.def.label) ? verbEntry.verb.EquipmentSource.LabelCap : verbEntry.verb.verbProps.label.CapitalizeFirst() + ": " + verbEntry.verb.EquipmentSource.LabelCap;
                }
                else if (verbEntry.verb.HediffCompSource != null)
                {
                    verbLabel = (verbEntry.verb.verbProps.label == verbEntry.verb.HediffSource.def.label) ? verbEntry.verb.HediffSource.def.LabelCap : verbEntry.verb.verbProps.label.CapitalizeFirst();
                }
                else
                {
                    verbLabel = (verbEntry.verb.verbProps.label == this.pawn.def.label) ? "Race-Defined weapon of " + this.pawn.def.label : verbEntry.verb.verbProps.label.CapitalizeFirst() + ": " + "Race-Defined weapon of " + this.pawn.def.label;
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
            this.allRangedVerbs = this.pawn.GetComp<VEF_Comp_Pawn_RangedVerbs>().GetRangedVerbs;
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

        private Pawn pawn;

        private List<VerbEntry> allRangedVerbs;

        private List<Verb> groupedVerbs;
    }
}
