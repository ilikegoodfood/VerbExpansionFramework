using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;
using UnityEngine;

namespace VerbExpansionFramework
{
    public class VEF_ThingCompProperties_ShieldDefense : CompProperties
    {
        public VEF_ThingCompProperties_ShieldDefense()
        {
            this.compClass = typeof(VEF_ThingComp_ShieldDefense);
        }

        public float energyMax = 0f;

        public float energyGainPerSecond = 0f;
    }
}
