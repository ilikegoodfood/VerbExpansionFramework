using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;
using Verse.AI;
using Verse.Sound;
using UnityEngine;

namespace VerbExpansionFramework
{
    [StaticConstructorOnStartup]
    public class VEF_ThingComp_ShieldDefense : ThingComp
    {
        public VEF_ThingCompProperties_ShieldDefense Props
        {
            get
            {
                return (VEF_ThingCompProperties_ShieldDefense)this.props;
            }
        }

        public float Energy
        {
            get
            {
                return this.energy;
            }
        }

        public ShieldState ShieldState
        {
            get
            {
                if (this.ticksToReset > 0)
                {
                    return ShieldState.Resetting;
                }
                return ShieldState.Active;
            }
        }

        public Pawn Pawn
        {
            get
            {
                return this.pawn;
            }
        }

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);

            if (!respawningAfterLoad)
            {
                this.Props.energyMax = UnityEngine.Random.Range(this.Props.energyMax * 0.9f, this.Props.energyMax * 1.1f);
                this.energyGainPerTick = UnityEngine.Random.Range(this.Props.energyGainPerSecond * 0.9f, this.Props.energyGainPerSecond * 1.1f) / 60f;

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

            this.drawSize = Math.Max(Pawn.Graphic.drawSize.x, Pawn.Graphic.drawSize.y);
        }

        private bool ShouldDisplay
        {
            get
            {
                Pawn pawn = (Pawn)base.parent;
                return pawn.Spawned && !pawn.Dead && !pawn.Downed && (pawn.InAggroMentalState || pawn.Drafted || (pawn.Faction.HostileTo(Faction.OfPlayer) && !pawn.IsPrisoner) || Find.TickManager.TicksGame < this.lastKeepDisplayTick + this.KeepDisplayingTicks);
            }
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look<Pawn>(ref this.pawn, "pawn");
            Scribe_Values.Look<float>(ref this.Props.energyMax, "energy", 0f, false);
            Scribe_Values.Look<float>(ref this.energyGainPerTick, "energyGain", 0f, false);
            Scribe_Values.Look<float>(ref this.energy, "energy", 0f, false);
            Scribe_Values.Look<int>(ref this.ticksToReset, "ticksToReset", -1, false);
            Scribe_Values.Look<int>(ref this.lastKeepDisplayTick, "lastKeepDisplayTick", 0, false);
        }

        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            if (Find.Selector.SingleSelectedThing == Pawn)
            {
                yield return new VEF_Gizmo_EnergyShieldStatus
                {
                    shield = this
                };
            }
        }

        public override void CompTick()
        {
            base.CompTick();
            if (Pawn == null)
            {
                this.energy = 0f;
                return;
            }
            if (this.ShieldState == ShieldState.Resetting)
            {
                this.ticksToReset--;
                if (this.ticksToReset <= 0)
                {
                    this.Reset();
                }
            }
            else if (this.ShieldState == ShieldState.Active)
            {
                this.energy += this.energyGainPerTick;
                if (this.energy > this.Props.energyMax)
                {
                    this.energy = this.Props.energyMax;
                }
            }
        }

        public override void PostPreApplyDamage(DamageInfo dinfo, out bool absorbed)
        {
            base.PostPreApplyDamage(dinfo, out absorbed);
            if (this.ShieldState != ShieldState.Active)
            {
                absorbed = false;
                return;
            }
            if (dinfo.Def == DamageDefOf.EMP)
            {
                this.energy = 0f;
                this.Break();
                absorbed = false;
                return;
            }
            if (dinfo.Def.isRanged || dinfo.Def.isExplosive)
            {
                this.energy -= dinfo.Amount * this.EnergyLossPerDamage;
                if (this.energy < 0f)
                {
                    this.Break();
                }
                else
                {
                    this.AbsorbedDamage(dinfo);
                }
                absorbed = true;
                return;
            }
            absorbed = false;
            return;
        }

        public void KeepDisplaying()
        {
            this.lastKeepDisplayTick = Find.TickManager.TicksGame;
        }

        private void AbsorbedDamage(DamageInfo dinfo)
        {
            SoundDefOf.EnergyShield_AbsorbDamage.PlayOneShot(new TargetInfo(Pawn.Position, Pawn.Map, false));
            this.impactAngleVect = Vector3Utility.HorizontalVectorFromAngle(dinfo.Angle);
            Vector3 loc = Pawn.TrueCenter() + this.impactAngleVect.RotatedBy(180f) * 0.5f * this.drawSize;
            float num = Mathf.Min(10f, 2f + dinfo.Amount / 10f);
            MoteMaker.MakeStaticMote(loc, Pawn.Map, ThingDefOf.Mote_ExplosionFlash, num);
            int num2 = (int)num;
            for (int i = 0; i < num2; i++)
            {
                MoteMaker.ThrowDustPuff(loc, Pawn.Map, Rand.Range(0.8f, 1.2f));
            }
            this.lastAbsorbDamageTick = Find.TickManager.TicksGame;
            this.KeepDisplaying();
        }

        private void Break()
        {
            Log.Message("Break");
            SoundDefOf.EnergyShield_Broken.PlayOneShot(new TargetInfo(Pawn.Position, Pawn.Map, false));
            Log.Message("Play Sound");
            MoteMaker.MakeStaticMote(Pawn.TrueCenter(), Pawn.Map, ThingDefOf.Mote_ExplosionFlash, 11f + this.drawSize);
            Log.Message("Make Mote");
            for (int i = 0; i < 6; i++)
            {
                Vector3 loc = Pawn.TrueCenter() + Vector3Utility.HorizontalVectorFromAngle((float)Rand.Range(0, 360)) * Rand.Range(0.3f, 0.6f) * this.drawSize;
                MoteMaker.ThrowDustPuff(loc, Pawn.Map, Rand.Range(this.drawSize - 0.2f, this.drawSize + 0.2f));
            }
            this.energy = 0f;
            this.ticksToReset = this.StartingTicksToReset;
        }

        private void Reset()
        {
            if (Pawn.Spawned)
            {
                SoundDefOf.EnergyShield_Reset.PlayOneShot(new TargetInfo(Pawn.Position, Pawn.Map, false));
                MoteMaker.ThrowLightningGlow(Pawn.TrueCenter(), Pawn.Map, 3f);
            }
            this.ticksToReset = -1;
            this.energy = this.EnergyOnReset;
        }

        public override void PostDraw()
        {
            base.PostDraw();
            if (this.ShieldState == ShieldState.Active && this.ShouldDisplay)
            {
                float num = Mathf.Lerp(this.drawSize + 0.2f, this.drawSize + 0.55f, this.energy);
                Vector3 vector = Pawn.Drawer.DrawPos;
                vector.y = AltitudeLayer.MoteOverhead.AltitudeFor();
                int num2 = Find.TickManager.TicksGame - this.lastAbsorbDamageTick;
                if (num2 < 8)
                {
                    float num3 = (float)(8 - num2) / 8f * 0.05f;
                    vector += this.impactAngleVect * num3;
                    num -= num3;
                }
                float angle = (float)Rand.Range(0, 360);
                Vector3 s = new Vector3(num, 1f, num);
                Matrix4x4 matrix = default(Matrix4x4);
                matrix.SetTRS(vector, Quaternion.AngleAxis(angle, Vector3.up), s);
                Graphics.DrawMesh(MeshPool.plane10, matrix, VEF_ThingComp_ShieldDefense.BubbleMat, 0);
            }
        }

        public bool AllowVerbCast(Pawn caster, LocalTargetInfo targ, Verb verb)
        {
            return !(verb is Verb_LaunchProjectile) || ReachabilityImmediate.CanReachImmediate(caster.Position, targ, caster.Map, PathEndMode.Touch, null);
        }

        private float energy;

        private float energyGainPerTick = 0f;

        private int ticksToReset = -1;

        private int lastKeepDisplayTick = -9999;

        private Pawn pawn;

        private float drawSize;

        private Vector3 impactAngleVect;

        private int lastAbsorbDamageTick = -9999;

        private const float MinDrawSize = 1.2f;

        private const float MaxDrawSize = 1.55f;

        private const float MaxDamagedJitterDist = 0.05f;

        private const int JitterDurationTicks = 8;

        private int StartingTicksToReset = 3200;

        private float EnergyOnReset = 0.2f;

        private float EnergyLossPerDamage = 0.033f;

        private int KeepDisplayingTicks = 1000;

        private static readonly Material BubbleMat = MaterialPool.MatFrom("Other/ShieldBubble", ShaderDatabase.Transparent);
    }
}
