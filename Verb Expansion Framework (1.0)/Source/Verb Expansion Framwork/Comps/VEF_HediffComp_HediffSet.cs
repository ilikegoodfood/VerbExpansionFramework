using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;
using UnityEngine;

namespace VerbExpansionFramework
{
    public class VEF_HediffComp_HediffSet : HediffComp
    {
        public VEF_HediffCompProperties_HediffSet Props
        {
            get
            {
                return (VEF_HediffCompProperties_HediffSet)this.Props;
            }
        }

        public override void CompPostMake()
        {
            base.CompPostMake();
            if (Pawn.AllComps.FirstOrDefault(c => c is VEF_ThingComp_HediffSet compHediffSet && compHediffSet.hediffSetDef == this.Props.hediffSetDef) == null)
            {
                Pawn.AllComps.Add(new VEF_ThingComp_HediffSet(this.Props.hediffSetDef));
            }
        }
    }
}
