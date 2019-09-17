using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;
using UnityEngine;

namespace VerbExpansionFramework
{
    public class VEF_HediffCompProperties_SmokepopDefense : HediffCompProperties
    {
        public VEF_HediffCompProperties_SmokepopDefense()
        {
            this.compClass = typeof(VEF_HediffComp_SmokepopDefense);
        }

        public int rechargeTime = 1;

        public float smokeRadius = 3f;

        public SoundDef smokepopSound;
    }
}
