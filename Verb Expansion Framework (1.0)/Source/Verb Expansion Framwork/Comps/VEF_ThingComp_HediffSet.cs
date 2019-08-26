using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;
using UnityEngine;


namespace VerbExpansionFramework
{
    public class VEF_ThingComp_HediffSet : ThingComp
    {
        public VEF_ThingComp_HediffSet(VEF_HediffSetDef hediffSetDef)
        {
            this.hediffSetDef = hediffSetDef;

            Pawn pawn2 = (Pawn)this.parent;
            if (pawn2 != null)
            {
                pawn = pawn2;
            }
            else if (this.parent.holdingOwner?.Owner is Pawn_EquipmentTracker eqTracker)
            {
                pawn = eqTracker.pawn;
            }
            else if (this.parent.holdingOwner?.Owner is Pawn_ApparelTracker apTracker)
            {
                pawn = apTracker.pawn;
            }

            UpdateHediffSet();
        }

        public Pawn Pawn
        {
            get
            {
                return this.pawn;
            }
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look<Pawn>(ref this.pawn, "pawn");
            Scribe_Values.Look<VEF_HediffSetDef>(ref this.hediffSetDef, "hediffSetDef");
        }

        public void UpdateHediffSet()
        {
            int count = 0;
            foreach (HediffDef hediffDef in this.hediffSetDef.hediffParts)
            {
                if (Pawn.health.hediffSet.hediffs.FirstOrDefault(h => h.def == hediffDef) != null)
                {
                    count += 1;
                }
            }

            HediffDef hediffDefIncomplete = hediffSetDef.setHediffIncomplete;
            HediffDef hediffDefComplete = hediffSetDef.setHediff;
            Hediff hediffIncomplete = Pawn.health.hediffSet.hediffs.FirstOrDefault(h => h.def == hediffDefIncomplete);
            Hediff hediffComplete = Pawn.health.hediffSet.hediffs.FirstOrDefault(h => h.def == hediffDefComplete);

            bool complete = count == hediffSetDef.hediffParts.Count + 1;

            if (count == 0)
            {
                if (hediffIncomplete != null)
                {
                    Pawn.health.RemoveHediff(hediffIncomplete);
                }
                if (hediffComplete != null)
                {
                    Pawn.health.RemoveHediff(hediffComplete);
                }
                Pawn.AllComps.Remove(this);
            }
            else if (complete)
            {
                if (hediffIncomplete != null)
                {
                    Pawn.health.RemoveHediff(hediffIncomplete);
                }
                if (hediffComplete == null)
                {
                    Pawn.health.AddHediff(hediffDefComplete);
                }
            }
            else
            {
                if (hediffComplete != null)
                {
                    Pawn.health.RemoveHediff(hediffComplete);
                }
                if (hediffIncomplete == null)
                {
                    Pawn.health.AddHediff(hediffDefIncomplete);
                }
            }
        }

        private Pawn pawn;

        public VEF_HediffSetDef hediffSetDef;
    }
}
