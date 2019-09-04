using System;
using System.Collections.Generic;
using System.Linq;
using Verse;
using RimWorld;
using UnityEngine;

namespace VerbExpansionFramework
{
    public class VEF_VerbProperties_Explode : VerbProperties
    {
        public float AdjustedExplosionDamageAmount(Verb ownerVerb, Pawn attacker)
        {
            if (ownerVerb.verbProps != this)
            {
                Log.ErrorOnce("Tried to calculate explosion damage amount for a verb with different verb props. verb=" + ownerVerb, 5469809, false);
                return 0f;
            }
            return this.explosionDamageAmount * this.GetDamageFactorFor(ownerVerb, attacker);
        }

        public void DrawExplodeRadiusRing(IntVec3 center)
        {
            if (Find.CurrentMap == null)
            {
                return;
            }
            if (this.explosionRadius < (float)(Find.CurrentMap.Size.x + Find.CurrentMap.Size.z) && this.explosionRadius < GenRadial.MaxRadialPatternRadius)
            {
                GenDraw.DrawRadiusRing(center, this.explosionRadius);
            }
        }

        public float explosionRadius;

        public DamageDef explosionDamageType;

        public int explosionDamageAmount;

        public SoundDef explosionSound;

        public ThingDef postExplosionSpawnThingDef;

        public float postExplosionSpawnThingChance = 1f;

        public int postExplosionSpawnThingCount = 1;

        public ThingDef preExplosionSpawnThingDef;

        public float preExplosionSpawnThingChance = 1f;

        public int preExplosionSpawnThingCount = 1;

        public bool applyDamageToExplosionCellsNeighbors = false;

        public float chanceToStartFire = 0f;

        public bool damageFalloff = false;
    }
}
