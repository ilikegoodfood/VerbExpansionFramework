using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;
using UnityEngine;


namespace VerbExpansionFramework
{
    public class VEF_HediffCompProperties_HealthModifier : HediffCompProperties
    {
        public VEF_HediffCompProperties_HealthModifier()
        {
            this.compClass = typeof(VEF_HediffCompProperties_HealthModifier);
        }

        public float healthScaleOffset = 0f;

        public float healthScaleFactorOffset = 0f;
    }
}
