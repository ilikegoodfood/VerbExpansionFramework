using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;
using UnityEngine;

namespace VerbExpansionFramework
{
    class VEF_Comp_HealthModifiers : HediffComp
    {

        public override void CompPostMake()
        {
            base.CompPostMake();
        }

        public bool propogate = false;

        public float healthScaleFactor;

        public float healthScaleOffset;

        public int healthOffset;
    }
}
