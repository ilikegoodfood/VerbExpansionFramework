using System;
using System.Collections.Generic;
using System.Linq;
using Verse;
using RimWorld;
using UnityEngine;

namespace VerbExpansionFramework
{
    class VEF_Verb_ExplodeSafe : VEF_Verb_Explode
    {
        protected override bool TryCastShot()
        {
            GenExplosion.DoExplosion(caster.Position, caster.Map, verbPropsExplode.explosionRadius, verbPropsExplode.explosionDamageType, caster, Mathf.RoundToInt(this.verbPropsExplode.AdjustedExplosionDamageAmount(this, this.CasterPawn)), -1, this.verbPropsExplode.explosionSound, this.TryGetWeapon(), null, null, this.verbPropsExplode.postExplosionSpawnThingDef, this.verbPropsExplode.postExplosionSpawnThingChance, this.verbPropsExplode.postExplosionSpawnThingCount, this.verbPropsExplode.applyDamageToExplosionCellsNeighbors, this.verbPropsExplode.preExplosionSpawnThingDef, this.verbPropsExplode.preExplosionSpawnThingChance, this.verbPropsExplode.preExplosionSpawnThingCount, this.verbPropsExplode.chanceToStartFire, this.verbPropsExplode.damageFalloff);
            return true;
        }

        public override bool CanHitTargetFrom(IntVec3 root, LocalTargetInfo targ)
        {
            if (targ.Thing != null && targ.Thing == this.caster)
            {
                return this.verbProps.targetParams.canTargetSelf;
            }
            return this.TryFindShootLineFromTo(root, targ, out ShootLine shootLine);
        }

        private ThingDef TryGetWeapon()
        {
            if (this.EquipmentSource != null)
            {
                return this.EquipmentSource.def;
            }
            else if (this.HediffSource != null)
            {
                return null;
            }
            else
            {
                return this.caster.def;
            }
        }

        private VEF_VerbProperties_Explode verbPropsExplode => (VEF_VerbProperties_Explode)verbProps;
    }
}
