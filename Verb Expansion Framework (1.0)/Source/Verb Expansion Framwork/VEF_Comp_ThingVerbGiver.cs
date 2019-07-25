using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;
using UnityEngine;

namespace VerbExpansionFramework
{
    public class VEF_Comp_ThingVerbGiver : ThingComp, IVerbOwner
    {
        public VEF_Comp_ThingVerbGiver()
        {
            this.verbTracker = new VerbTracker(this);
        }

        public Pawn Pawn
        {
            get
            {
                Pawn caster = null;
                if (!this.verbTracker.AllVerbs.NullOrEmpty())
                {
                    caster = (Pawn)this.verbTracker.AllVerbs[0].caster;
                }
                return this.pawn ?? (this.pawn = caster);
            }
        }

        public VEF_CompProperties_ThingVerbGiver Props => (VEF_CompProperties_ThingVerbGiver)this.props;

        public VerbTracker VerbTracker
        {
            get
            {
                return this.verbTracker;
            }
        }

        public List<VerbProperties> VerbProperties
        {
            get
            {
                return this.Props.verbs;
            }
        }

        public List<Tool> Tools
        {
            get
            {
                return this.Props.tools;
            }
        }

        Thing IVerbOwner.ConstantCaster
        {
            get
            {
                return this.Pawn;
            }
        }

        ImplementOwnerTypeDef IVerbOwner.ImplementOwnerTypeDef
        {
            get
            {
                return ImplementOwnerTypeDefOf.Weapon;
            }
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Deep.Look<VerbTracker>(ref this.verbTracker, "verbTracker", new object[]
            {
                this
            });
        }

        public override void CompTick()
        {
            base.CompTick();
            this.verbTracker.VerbsTick();
        }

        string IVerbOwner.UniqueVerbOwnerID()
        {
            return "VEF_Comp_ThingVerbGiver_" + this.parent.ThingID;
        }

        bool IVerbOwner.VerbsStillUsableBy(Pawn p)
        {
            Apparel apparel = this.parent as Apparel;
            if (apparel != null)
            {
                return p.apparel.WornApparel.Contains(apparel);
            }
            return p.equipment.AllEquipmentListForReading.Contains(this.parent);
        }

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            if (Props.tools != null)
            {
                for (int i = 0; i < Props.tools.Count; i++)
                {
                    Props.tools[i].id = i.ToString();
                }
            }
            if (this.verbTracker.AllVerbs != null)
            {
                foreach (Verb v in this.verbTracker.AllVerbs)
                {
                    v.caster = this.Pawn;
                }
            }
        }

        private Pawn pawn;

        public VerbTracker verbTracker;
    }

    public class VEF_CompProperties_ThingVerbGiver : CompProperties
    {
        public VEF_CompProperties_ThingVerbGiver()
        {
            this.compClass = typeof(VEF_Comp_ThingVerbGiver);
        }

        public List<VerbProperties> verbs;

        public List<Tool> tools;
    }
}
