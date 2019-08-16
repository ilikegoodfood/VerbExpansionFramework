using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;
using UnityEngine;

namespace VerbExpansionFramework
{
    [StaticConstructorOnStartup]
    public static class VEF_ModCompatibilityCheck
    {
        static VEF_ModCompatibilityCheck()
        {
            if (ModsConfig.ActiveModsInLoadOrder.Any(m => m.Name == "Range Animal Framework") || GenTypes.GetTypeInAnyAssemblyNew("AnimalRangeAttack.AnimalRangeAttack_Init", "AnimalRangeAttack") != null)
            {
                enabled_RangeAnimalFramework = true;
            }

            if (ModsConfig.ActiveModsInLoadOrder.Any(m => m.Name == "Smokepop Defense Framework") || GenTypes.GetTypeInAnyAssemblyNew("SmokepopCompFramework.SCF_Harmony", "SmokepopCompFramework") != null)
            {
                enabled_SmokepopDefenseFramework = true;
            }

            if (ModsConfig.ActiveModsInLoadOrder.Any(m => m.Name == "Variable Health Framework") || GenTypes.GetTypeInAnyAssemblyNew("VariableHealthFramework.VHF_Harmony", "VariableHealthFramework") != null)
            {
                enabled_VariableHealthFramework = true;
            }
        }

        public static bool enabled_rooloDualWield = ModsConfig.ActiveModsInLoadOrder.Any(m => m.Name == "Dual Wield");

        public static bool enabled_CombatExtended = ModsConfig.ActiveModsInLoadOrder.Any(m => m.Name == "Combat Extended");

        public static bool enabled_RangeAnimalFramework;

        public static bool enabled_SmokepopDefenseFramework;

        public static bool enabled_VariableHealthFramework;
    }
}
