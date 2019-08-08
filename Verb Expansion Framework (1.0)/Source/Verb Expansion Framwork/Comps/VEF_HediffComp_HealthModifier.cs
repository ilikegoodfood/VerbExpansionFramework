using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;
using UnityEngine;


namespace VerbExpansionFramework
{
    public class VEF_HediffComp_HealthModifier : HediffComp
    {
        public VEF_HediffCompProperties_HealthModifier Props
        {
            get
            {
                return (VEF_HediffCompProperties_HealthModifier)this.props;
            }
        }
    }
}
