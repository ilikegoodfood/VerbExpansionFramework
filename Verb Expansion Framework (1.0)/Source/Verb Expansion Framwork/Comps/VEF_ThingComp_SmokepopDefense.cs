using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;
using UnityEngine;

namespace VerbExpansionFramework
{
    public class VEF_ThingComp_SmokepopDefense : ThingComp
    {
        public VEF_ThingCompProperties_SmokepopDefense Props
        {
            get
            {
                return (VEF_ThingCompProperties_SmokepopDefense)this.props;
            }
        }

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            if (!respawningAfterLoad)
            {
                Props.smokeRadius = UnityEngine.Random.Range(Props.smokeRadius * 0.9f, Props.smokeRadius * 1.1f);
                Props.rechargeTime = UnityEngine.Mathf.RoundToInt(UnityEngine.Random.Range(Props.rechargeTime * 60 * 0.9f, Props.rechargeTime * 60 * 1.1f) / 60);

                Pawn pawn2 = (Pawn)this.parent;
                if (pawn2 != null)
                {
                    pawn = pawn2;
                }
                else if (this.parent.holdingOwner?.Owner is Pawn_EquipmentTracker eqTracker)
                {
                    pawn = eqTracker.pawn;
                }
                else if (this.parent.holdingOwner?.Owner is Pawn_ApparelTracker apTracker)
                {
                    pawn = apTracker.pawn;
                }
            }
        }
        public override void PostPreApplyDamage(DamageInfo dinfo, out bool absorbed)
        {
            absorbed = false;
            if (Find.TickManager.TicksGame > lastUsedSmoke + (Props.rechargeTime * 60))
            {
                if (!dinfo.Def.isExplosive && dinfo.Def.harmsHealth && dinfo.Def.ExternalViolenceFor(pawn))
                {
                    if (dinfo.Instigator is Pawn instigatorPawn && instigatorPawn.GetComp<VEF_Comp_Pawn_RangedVerbs>() is VEF_Comp_Pawn_RangedVerbs comp && comp.CurRangedVerb is Verb verb && !verb.IsMeleeAttack)
                    {
                        IntVec3 position = pawn.Position;
                        Map map = pawn.Map;
                        DamageDef smoke = DamageDefOf.Smoke;
                        ThingDef gas_Smoke = ThingDefOf.Gas_Smoke;
                        GenExplosion.DoExplosion(position, map, this.Props.smokeRadius, smoke, pawn as Thing, -1, -1f, this.Props.smokepopSound, null, null, null, gas_Smoke, 1f, 1, false, null, 0f, 1, 0f, false);
                        this.lastUsedSmoke = Find.TickManager.TicksGame;
                    }
                    if (dinfo.Instigator as Building != null && dinfo.Weapon != null && dinfo.Weapon.IsRangedWeapon)
                    {
                        IntVec3 position = pawn.Position;
                        Map map = pawn.Map;
                        DamageDef smoke = DamageDefOf.Smoke;
                        ThingDef gas_Smoke = ThingDefOf.Gas_Smoke;
                        GenExplosion.DoExplosion(position, map, this.Props.smokeRadius, smoke, pawn as Thing, -1, -1f, this.Props.smokepopSound, null, null, null, gas_Smoke, 1f, 1, false, null, 0f, 1, 0f, false);
                        this.lastUsedSmoke = Find.TickManager.TicksGame;
                    }
                }
            }
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look<Pawn>(ref this.pawn, "pawn");
            Scribe_Values.Look<int>(ref this.lastUsedSmoke, "lastUsedSmoke", -99999, false);
            Scribe_Values.Look<int>(ref Props.rechargeTime, "rechargeTime", 1, false);
            Scribe_Values.Look<float>(ref Props.smokeRadius, "smokeRadius", 0, false);
        }

        private Pawn pawn;

        private int lastUsedSmoke = -99999;
    }
}
