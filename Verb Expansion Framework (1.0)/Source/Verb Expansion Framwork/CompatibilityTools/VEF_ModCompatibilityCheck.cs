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
        public static bool enabled_rooloDualWield = ModsConfig.ActiveModsInLoadOrder.Any(m => m.Name == "Dual Wield");

        public static bool enabled_CombatExtended = ModsConfig.ActiveModsInLoadOrder.Any(m => m.Name == "CombatExtended");

        public static bool enabled_RangeAnimalFramework = ModsConfig.ActiveModsInLoadOrder.Any(m => m.Name == "Range Animal Framework");
    }
}
