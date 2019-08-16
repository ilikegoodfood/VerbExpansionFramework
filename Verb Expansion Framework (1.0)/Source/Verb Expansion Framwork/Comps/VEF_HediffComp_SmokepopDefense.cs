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
            Props.smokeRadius = UnityEngine.Random.Range(Props.smokeRadius * 0.9f, Props.smokeRadius * 1.1f);
            Props.rechargeTime = UnityEngine.Mathf.RoundToInt(UnityEngine.Random.Range(Props.rechargeTime * 0.9f, Props.rechargeTime * 1.1f));
        }

        public void TryTriggerSmokepopDefense(DamageInfo dinfo)
        {
            Log.Message("Trying to trigger smokepop defense");
            if (Find.TickManager.TicksGame > this.lastUsedSmoke + (this.Props.rechargeTime * 60))
            {
                Log.Message("lastUsedSmoke found and rechargeTime passed");
                if (!dinfo.Def.isExplosive && dinfo.Def.harmsHealth && dinfo.Def.ExternalViolenceFor(Pawn))
                {
                    if (dinfo.Instigator is Pawn instigatorPawn && instigatorPawn.GetComp<VEF_Comp_Pawn_RangedVerbs>().CurRangedVerb != null && instigatorPawn.Position.DistanceTo(Pawn.Position) > 1f)
                    {
                        IntVec3 position = Pawn.Position;
                        Map map = Pawn.Map;
                        DamageDef smoke = DamageDefOf.Smoke;
                        ThingDef gas_Smoke = ThingDefOf.Gas_Smoke;
                        GenExplosion.DoExplosion(position, map, this.Props.smokeRadius, smoke, Pawn as Thing, -1, -1f, null, null, null, null, gas_Smoke, 1f, 1, false, null, 0f, 1, 0f, false);
                        this.lastUsedSmoke = Find.TickManager.TicksGame;
                    }
                    if (dinfo.Instigator is Building instigatorTurret && dinfo.Weapon != null && dinfo.Weapon.IsRangedWeapon)
                    {
                        IntVec3 position = Pawn.Position;
                        Map map = Pawn.Map;
                        DamageDef smoke = DamageDefOf.Smoke;
                        ThingDef gas_Smoke = ThingDefOf.Gas_Smoke;
                        GenExplosion.DoExplosion(position, map, this.Props.smokeRadius, smoke, Pawn as Thing, -1, -1f, null, null, null, null, gas_Smoke, 1f, 1, false, null, 0f, 1, 0f, false);
                        this.lastUsedSmoke = Find.TickManager.TicksGame;
                    }
                }
            }
        }

        public override void CompExposeData()
        {
            base.CompExposeData();
            Scribe_Values.Look<int>(ref this.lastUsedSmoke, "lastUsedSmoke", - 99999, false);
            Scribe_Values.Look<int>(ref Props.rechargeTime, "rechargeTime", 1, false);
            Scribe_Values.Look<float>(ref Props.smokeRadius, "smokeRadius", 0, false);
        }

        private int lastUsedSmoke = -99999;
    }
}
