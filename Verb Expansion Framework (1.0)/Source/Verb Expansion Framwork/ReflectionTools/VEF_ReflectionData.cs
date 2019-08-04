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

            if (VEF_ModCompatibilityCheck.enabled_CombatExtended)
            {
                PG_CE_CompAmmoUser_CanBeFiredNow = AccessTools.Property(type: GenTypes.GetTypeInAnyAssemblyNew("CombatExtended.CompAmmoUser", "CombatExtended"), name: "CanBeFiredNow").GetGetMethod();
       
                T_CombatExtended_CompAmmoUser = GenTypes.GetTypeInAnyAssemblyNew("CombatExtended.CompAmmoUser", "CombatExtended");
            }

            if (VEF_ModCompatibilityCheck.enabled_rooloDualWield)
            {
                TryGetOffHandEquipment = (FuncOut<Pawn_EquipmentTracker, ThingWithComps, bool>)Delegate.CreateDelegate(typeof(FuncOut<Pawn_EquipmentTracker, ThingWithComps, bool>), AccessTools.Method(GenTypes.GetTypeInAnyAssemblyNew("DualWield.Ext_Pawn_EquipmentTracker", "DualWield"), "TryGetOffHandEquipment"));
            }
        }

        //Fields
            //Core
        public static FieldInfo FI_Pawn_HealthTracker_pawn = AccessTools.Field(type: typeof(Pawn_HealthTracker), name: "pawn");

        //Methods
            //Roolo's Dual Wield
        public static FuncOut<Pawn_EquipmentTracker, ThingWithComps, bool> TryGetOffHandEquipment;

        public delegate V FuncOut<T, U, V>(T input, out U output);

        //Properties Getter
            //CombatExtended
        public static MethodBase PG_CE_CompAmmoUser_CanBeFiredNow;

        //Types
            //CombatExtended
        public static Type T_CombatExtended_CompAmmoUser;
    }
}
