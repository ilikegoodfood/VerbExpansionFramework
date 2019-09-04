using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;
using UnityEngine;

namespace VerbExpansionFramework
{
    public class VEF_ThingCompProperties_SmokepopDefense : CompProperties
    {
        public VEF_ThingCompProperties_SmokepopDefense()
        {
            this.compClass = typeof(VEF_ThingComp_SmokepopDefense);
        }

        public int rechargeTime = 1;

        public float smokeRadius = 3f;

        public SoundDef smokepopSound;
    }
}
