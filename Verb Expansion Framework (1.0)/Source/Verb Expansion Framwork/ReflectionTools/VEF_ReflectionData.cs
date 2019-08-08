using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Reflection.Emit;
using Harmony;
using RimWorld;
using Verse;
using UnityEngine;

namespace VerbExpansionFramework
{

    [StaticConstructorOnStartup]
    public static class VEF_ReflectionData
    {
        static VEF_ReflectionData()
        {

        }

        //Fields
            //Core
        public static FieldInfo FI_Pawn_HealthTracker_pawn = AccessTools.Field(type: typeof(Pawn_HealthTracker), name: "pawn");
    }
}
