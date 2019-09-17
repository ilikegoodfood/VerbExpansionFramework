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
        public VEF_ThingComp_HediffSet(Pawn pawn, VEF_HediffSetDef hediffSetDef)
        {
            base.Initialize(null);
            this.hediffSetDef = hediffSetDef;
            this.parent = pawn;
            this.pawn = pawn;
        }

        public Pawn Pawn
        {
            get
            {
                return this.pawn ?? (this.pawn = (Pawn)this.parent);
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
            if (Pawn == null)
            {
                return;
            }
            else if (Pawn.health.hediffSet.hediffs.NullOrEmpty() || this.hediffSetDef == null || this.hediffSetDef.hediffParts.NullOrEmpty())
            {
                Pawn.AllComps.Remove(this);
                return;
            }

            int count = 0;
            List<Hediff> allHediffs = Pawn.health.hediffSet.hediffs;

            foreach (HediffDef hediffDef in this.hediffSetDef.hediffParts)
            {
                if (allHediffs.FirstOrDefault(h => h.def == hediffDef) is Hediff hediff)
                {
                    count += 1;
                    allHediffs.Remove(hediff);
                }
            }

            bool complete = count == hediffSetDef.hediffParts.Count;

            HediffDef hediffDefIncomplete = hediffSetDef.setHediffIncomplete;
            HediffDef hediffDefComplete = hediffSetDef.setHediff;
            Hediff hediffIncomplete = null;
            Hediff hediffComplete = null;

            if (hediffDefIncomplete != null)
            {
                hediffIncomplete = Pawn.health.hediffSet.hediffs.FirstOrDefault(h => h.def == hediffDefIncomplete);
            }
            if (hediffDefComplete != null)
            {
                hediffComplete = Pawn.health.hediffSet.hediffs.FirstOrDefault(h => h.def == hediffDefComplete);
            }

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
                if (hediffComplete == null && hediffDefComplete != null)
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
                if (hediffIncomplete == null && hediffDefIncomplete != null)
                {
                    Pawn.health.AddHediff(hediffDefIncomplete);
                }
            }
            return;
        }

        private Pawn pawn;

        public VEF_HediffSetDef hediffSetDef;
    }
}
