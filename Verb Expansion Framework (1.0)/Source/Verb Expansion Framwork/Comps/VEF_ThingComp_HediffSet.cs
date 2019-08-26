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
            Log.Message("Updating Hediff Set");
            if (Pawn == null)
            {
                Log.Message("Null Pawn");
                return;
            }
            else if (Pawn.health.hediffSet.hediffs.NullOrEmpty() || this.hediffSetDef == null || this.hediffSetDef.hediffParts.NullOrEmpty())
            {
                Log.Message("Null Hediffs");
                Pawn.AllComps.Remove(this);
                return;
            }
            Log.Message("Passed Null Checks.");

            int count = 0;
            
            foreach (HediffDef hediffDef in this.hediffSetDef.hediffParts)
            {
                Log.Message("Iterating for HediffParts");
                if (Pawn.health.hediffSet.hediffs.FirstOrDefault(h => h.def == hediffDef) != null)
                {
                    Log.Message("HedifPart Found");
                    count += 1;
                }
            }
            Log.Message("Completed iterating for parts.");

            bool complete = count == hediffSetDef.hediffParts.Count;
            Log.Message("Count = " + count + ", Complete = " + complete);

            HediffDef hediffDefIncomplete = hediffSetDef.setHediffIncomplete;
            HediffDef hediffDefComplete = hediffSetDef.setHediff;
            Hediff hediffIncomplete = Pawn.health.hediffSet.hediffs.FirstOrDefault(h => h.def == hediffDefIncomplete);
            Hediff hediffComplete = Pawn.health.hediffSet.hediffs.FirstOrDefault(h => h.def == hediffDefComplete);
            Log.Message("Gathered Hediff Data and Hediffs");

            if (count == 0)
            {
                Log.Message("Count = 0");
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
                Log.Message("Compete. Count = " + count);
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
                Log.Message("Incomplete. Count =" + count);
                if (hediffComplete != null)
                {
                    Pawn.health.RemoveHediff(hediffComplete);
                }
                if (hediffIncomplete == null)
                {
                    Pawn.health.AddHediff(hediffDefIncomplete);
                }
            }
            Log.Message("End.");
            return;
        }

        private Pawn pawn;

        public VEF_HediffSetDef hediffSetDef;
    }
}
