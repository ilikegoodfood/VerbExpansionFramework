using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;
using UnityEngine;

namespace VerbExpansionFramework
{
    public class VEF_HediffSetDef : Def
    {
        public HediffDef setHediff;

        public HediffDef setHediffIncomplete;

        public List<HediffDef> hediffParts;
    }
}
