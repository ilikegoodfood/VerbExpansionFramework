using System;
using System.Collections.Generic;
using System.Linq;
using Verse;
using RimWorld;
using UnityEngine;

namespace VerbExpansionFramework
{
    public class VEF_KillInfo
    {
        public VEF_KillInfo(Thing t, DamageDef dDef)
        {
            this.thing = t;
            this.damageDef = dDef;
        }

        public Thing thing;
        public DamageDef damageDef;
    }
}
