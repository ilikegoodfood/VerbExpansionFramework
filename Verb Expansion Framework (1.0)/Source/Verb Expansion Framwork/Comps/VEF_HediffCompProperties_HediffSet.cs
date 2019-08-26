using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;
using UnityEngine;

namespace VerbExpansionFramework
{
    public class VEF_HediffCompProperties_HediffSet : HediffCompProperties
    {
        public VEF_HediffCompProperties_HediffSet()
        {
            this.compClass = typeof(VEF_HediffComp_HediffSet);
        }

        public VEF_HediffSetDef hediffSetDef;
    }
}
