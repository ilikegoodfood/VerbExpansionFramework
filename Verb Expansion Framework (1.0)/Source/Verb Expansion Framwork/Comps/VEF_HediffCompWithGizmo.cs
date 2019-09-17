using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;
using UnityEngine;

namespace VerbExpansionFramework
{
    class VEF_HediffCompWithGizmo : HediffComp
    {
        public virtual IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            yield break;
        }
    }
}
