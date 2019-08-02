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
            owner = (Pawn)parent;
            if (!respawningAfterLoad)
            {
                Props.smokeRadius = UnityEngine.Random.Range(Props.smokeRadius * 0.9f, Props.smokeRadius * 1.1f);
                Props.rechargeTime = UnityEngine.Mathf.RoundToInt(UnityEngine.Random.Range(Props.rechargeTime * 60 * 0.9f, Props.rechargeTime * 60 * 1.1f) / 60);
            }
        }
        public override void PostPreApplyDamage(DamageInfo dinfo, out bool absorbed)
        {
            absorbed = false;
            if (Find.TickManager.TicksGame > lastUsedSmoke + (Props.rechargeTime * 60))
            {
                if (!dinfo.Def.isExplosive && dinfo.Def.harmsHealth && dinfo.Def.ExternalViolenceFor(this.owner as Thing) && dinfo.Instigator is Pawn instigatorPawn && instigatorPawn.GetComp<VEF_Comp_Pawn_RangedVerbs>().CurRangedVerb != null && !instigatorPawn.GetComp<VEF_Comp_Pawn_RangedVerbs>().CurRangedVerb.IsMeleeAttack)
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

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_References.Look<ThingWithComps>(ref this.owner, "owner", false);
            Scribe_Values.Look<int>(ref this.lastUsedSmoke, "lastUsedSmoke");
            Scribe_Values.Look<int>(ref Props.rechargeTime, "rechargeTime", 1, false);
            Scribe_Values.Look<float>(ref Props.smokeRadius, "smokeRadius", 0, false);
        }

        private ThingWithComps owner;

        private int lastUsedSmoke = -99999;
    }
}
