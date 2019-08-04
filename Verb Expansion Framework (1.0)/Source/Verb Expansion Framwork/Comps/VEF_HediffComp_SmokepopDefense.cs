using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;
using UnityEngine;

namespace VerbExpansionFramework
{
    class VEF_HediffComp_SmokepopDefense : HediffComp
    {
        public VEF_HediffCompProperties_SmokepopDefense Props
        {
            get
            {
                return (VEF_HediffCompProperties_SmokepopDefense)this.props;
            }
        }

        public override void CompPostMake()
        {
            owner = Pawn;
            Props.smokeRadius = UnityEngine.Random.Range(Props.smokeRadius * 0.9f, Props.smokeRadius * 1.1f);
            Props.rechargeTime = UnityEngine.Mathf.RoundToInt(UnityEngine.Random.Range(Props.rechargeTime * 60 * 0.9f, Props.rechargeTime * 60 * 1.1f) / 60);
        }
        public void TryTriggerSmokepopDefense(DamageInfo dinfo)
        {
            if (Find.TickManager.TicksGame > lastUsedSmoke + (Props.rechargeTime * 60))
            {
                if (!dinfo.Def.isExplosive && dinfo.Def.harmsHealth && dinfo.Def.ExternalViolenceFor(this.owner as Thing) && dinfo.Instigator is Pawn instigatorPawn && instigatorPawn.GetComp<VEF_Comp_Pawn_RangedVerbs>().CurRangedVerb != null && instigatorPawn.Position.DistanceTo(this.owner.Position) > 3f)
                {
                    IntVec3 position = this.owner.Position;
                    Map map = this.owner.Map;
                    DamageDef smoke = DamageDefOf.Smoke;
                    ThingDef gas_Smoke = ThingDefOf.Gas_Smoke;
                    GenExplosion.DoExplosion(position, map, Props.smokeRadius, smoke, this.owner as Thing, -1, -1f, null, null, null, null, gas_Smoke, 1f, 1, false, null, 0f, 1, 0f, false);
                    lastUsedSmoke = Find.TickManager.TicksGame;
                }
            }
        }

        private Pawn owner;

        private int lastUsedSmoke = -99999;
    }
}
