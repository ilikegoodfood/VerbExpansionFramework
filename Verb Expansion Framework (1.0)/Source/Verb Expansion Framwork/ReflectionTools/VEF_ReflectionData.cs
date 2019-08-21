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
            if (VEF_ModCompatibilityCheck.enabled_rooloDualWield)
            {
                MB_TryGetOffHandEquipment = AccessTools.Method(type: GenTypes.GetTypeInAnyAssemblyNew("DualWield.Ext_Pawn_EquipmentTracker", "DualWield"), name: "TryGetOffHandEquipment");
            }
        }

        //Fields
        //Core
        public static FieldInfo FI_Pawn_HealthTracker_pawn = AccessTools.Field(type: typeof(Pawn_HealthTracker), name: "pawn");

        //Methods
        //Core
        public static MethodBase MB_Pawn_DraftController_GetGizmo()
        {
            var predicateClass = typeof(Pawn_DraftController).GetNestedTypes(AccessTools.all).FirstOrDefault(t => t.FullName.Contains("c__Iterator0"));
            return predicateClass.GetMethods(AccessTools.all).FirstOrDefault(m => m.Name.Contains("MoveNext"));
        }
        //TryGetOffHandEquipment
        public static MethodBase MB_TryGetOffHandEquipment;
    }
}
