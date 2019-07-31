using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;
using UnityEngine;

namespace VerbExpansionFramework
{
    public static class VEF_ModCompatibilityCheck
    {
        public static bool rooloDualWield
        {
            get
            {
                return ModsConfig.ActiveModsInLoadOrder.Any(m => m.Name == "Dual Wield");
            }
        }
    }
}
