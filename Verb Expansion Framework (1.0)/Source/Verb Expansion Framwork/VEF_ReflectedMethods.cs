using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Harmony;
using RimWorld;
using Verse;
using UnityEngine;

namespace VerbExpansionFramework
{
    [StaticConstructorOnStartup]
    public static class VEF_ReflectedMethods
    {
        static VEF_ReflectedMethods()
        {
            // Convert DualWield.Ext_Pawn_EquipmentTracker.TryGetOffHandEquipment to a delegate
            if (VEF_ModCompatibilityCheck.rooloDualWield)
            {
                TryGetOffHandEquipment = (FuncOut<Pawn_EquipmentTracker, ThingWithComps, bool>)Delegate.CreateDelegate(typeof(FuncOut<Pawn_EquipmentTracker, ThingWithComps, bool>), AccessTools.Method(GenTypes.GetTypeInAnyAssemblyNew("DualWield.Ext_Pawn_EquipmentTracker", "DualWield"), "TryGetOffHandEquipment"));
            }
        }

        public static FuncOut<Pawn_EquipmentTracker, ThingWithComps, bool> TryGetOffHandEquipment;

        public delegate V FuncOut<T, U, V>(T input, out U output);
    }
}
