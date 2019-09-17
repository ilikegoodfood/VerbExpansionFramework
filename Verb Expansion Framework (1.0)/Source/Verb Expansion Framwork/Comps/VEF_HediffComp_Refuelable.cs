using System;
using System.Collections.Generic;
using System.Linq;
using Verse;
using RimWorld;
using UnityEngine;

namespace VerbExpansionFramework
{
    class VEF_HediffComp_Refuelable : VEF_HediffCompWithGizmo
    {
        public VEF_HediffCompProperties_Refuelable Props
        {
            get
            {
                return (VEF_HediffCompProperties_Refuelable)this.props;
            }
        }

        public float TargetFuelLevel
        {
            get
            {
                if (this.configuredTargetFuelLevel >= 0f)
                {
                    return this.configuredTargetFuelLevel;
                }
                if (this.Props.targetFuelLevelConfigurable)
                {
                    return this.Props.initialConfigurableTargetFuelLevel;
                }
                return this.Props.fuelCapacity;
            }
            set
            {
                this.configuredTargetFuelLevel = Mathf.Clamp(value, 0f, this.Props.fuelCapacity);
            }
        }

        public float Fuel
        {
            get
            {
                return this.fuel;
            }
        }

        public float FuelPercentOfTarget
        {
            get
            {
                return this.fuel / this.TargetFuelLevel;
            }
        }

        public float FuelPercentOfMax
        {
            get
            {
                return this.fuel / this.Props.fuelCapacity;
            }
        }

        public bool IsFull
        {
            get
            {
                return this.TargetFuelLevel - this.fuel < 1f;
            }
        }

        public bool HasFuel
        {
            get
            {
                return this.fuel > 0f && this.fuel >= this.Props.minimumFueledThreshold;
            }
        }

        private float ConsumptionRatePerTick
        {
            get
            {
                return this.Props.fuelConsumptionRate / 60000f;
            }
        }

        public bool ShouldAutoRefuelNow
        {
            get
            {
                return this.FuelPercentOfTarget <= this.Props.autoRefuelPercent && !this.IsFull && this.TargetFuelLevel > 0f && this.ShouldAutoRefuelNowIgnoringFuelPct;
            }
        }

        public bool ShouldAutoRefuelNowIgnoringFuelPct
        {
            get
            {
                return !this.Pawn.IsBurning() && this.Pawn.Map.designationManager.DesignationOn(this.Pawn, DesignationDefOf.Flick) == null;
            }
        }

        public override void CompExposeData()
        {
            base.CompExposeData();
            Scribe_Values.Look<float>(ref this.fuel, "fuel", 0f, false);
            Scribe_Values.Look<float>(ref this.configuredTargetFuelLevel, "configuredTargetFuelLevel", -1f, false);
        }

        public override void Notify_PawnDied()
        {
            base.Notify_PawnDied();
            if (this.Pawn.Map != null && this.Props.fuelFilter.AllowedDefCount == 1 && this.Props.initialFuelPercent == 0f)
            {
                ThingDef thingDef = this.Props.fuelFilter.AllowedThingDefs.First<ThingDef>();
                float num = 1f;
                int i = GenMath.RoundRandom(num * this.fuel);
                while (i > 0)
                {
                    Thing thing = ThingMaker.MakeThing(thingDef, null);
                    thing.stackCount = Mathf.Min(i, thingDef.stackLimit);
                    i -= thing.stackCount;
                    GenPlace.TryPlaceThing(thing, this.Pawn.Position, this.Pawn.Map, ThingPlaceMode.Near, null, null);
                }
            }
        }

        public override string CompTipStringExtra
        {
            get
            {
                return this.InspectString();
            }   
        }

        private string InspectString()
        {
            string text = string.Concat(new string[]
            {
                this.Props.FuelLabel,
                ": ",
                this.fuel.ToStringDecimalIfSmall(),
                " / ",
                this.Props.fuelCapacity.ToStringDecimalIfSmall()
            });
            if (!this.Props.consumeFuelOnlyWhenUsed && this.HasFuel)
            {
                int numTicks = (int)(this.fuel / this.Props.fuelConsumptionRate * 60000f);
                text = text + " (" + numTicks.ToStringTicksToPeriod() + ")";
            }
            if (!this.HasFuel && !this.Props.outOfFuelMessage.NullOrEmpty())
            {
                text += string.Format("\n{0} ({1}x {2})", this.Props.outOfFuelMessage, this.GetFuelCountToFullyRefuel(), this.Props.fuelFilter.AnyAllowedDef.label);
            }
            if (this.Props.targetFuelLevelConfigurable)
            {
                text = text + "\n" + "ConfiguredTargetFuelLevel".Translate(this.TargetFuelLevel.ToStringDecimalIfSmall());
            }
            return text;
        }

        public override void CompPostTick(ref float severityAdjustment)
        {
            base.CompPostTick(ref severityAdjustment);
            if (!this.Props.consumeFuelOnlyWhenUsed)
            {
                this.ConsumeFuel(this.ConsumptionRatePerTick);
            }
            if (this.Props.fuelConsumptionPerTickInRain > 0f && this.Pawn.Spawned && this.Pawn.Map.weatherManager.RainRate > 0.4f && !this.Pawn.Map.roofGrid.Roofed(this.Pawn.Position))
            {
                this.ConsumeFuel(this.Props.fuelConsumptionPerTickInRain);
            }
        }

        public void ConsumeFuel(float amount)
        {
            if (this.fuel <= 0f)
            {
                return;
            }
            this.fuel -= amount;
            if (this.fuel <= 0f)
            {
                this.fuel = 0f;
                if (this.Props.destroyOnNoFuel)
                {
                    this.Pawn.health.hediffSet.hediffs.Remove(this.parent);
                }
                this.Pawn.BroadcastCompSignal("RanOutOfFuel");
            }
        }

        public void Refuel(List<Thing> fuelThings)
        {
            if (this.Props.atomicFueling)
            {
                if (fuelThings.Sum((Thing t) => t.stackCount) < this.GetFuelCountToFullyRefuel())
                {
                    Log.ErrorOnce("Error refueling; not enough fuel available for proper atomic refuel", 19586442, false);
                    return;
                }
            }
            int num = this.GetFuelCountToFullyRefuel();
            while (num > 0 && fuelThings.Count > 0)
            {
                Thing thing = fuelThings.Pop<Thing>();
                int num2 = Mathf.Min(num, thing.stackCount);
                this.Refuel((float)num2);
                thing.SplitOff(num2).Destroy(DestroyMode.Vanish);
                num -= num2;
            }
        }

        public void Refuel(float amount)
        {
            this.fuel += amount * this.Props.FuelMultiplierCurrentDifficulty;
            if (this.fuel > this.Props.fuelCapacity)
            {
                this.fuel = this.Props.fuelCapacity;
            }
            this.Pawn.BroadcastCompSignal("Refueled");
        }

        public void Notify_UsedThisTick()
        {
            this.ConsumeFuel(this.ConsumptionRatePerTick);
        }

        public int GetFuelCountToFullyRefuel()
        {
            if (this.Props.atomicFueling)
            {
                return Mathf.CeilToInt(this.Props.fuelCapacity / this.Props.FuelMultiplierCurrentDifficulty);
            }
            float f = (this.TargetFuelLevel - this.fuel) / this.Props.FuelMultiplierCurrentDifficulty;
            return Mathf.Max(Mathf.CeilToInt(f), 1);
        }

        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            if (this.Props.targetFuelLevelConfigurable)
            {
                yield return new Command_SetTargetFuelLevel
                {
                    refuelable = this,
                    defaultLabel = "CommandSetTargetFuelLevel".Translate(),
                    defaultDesc = "CommandSetTargetFuelLevelDesc".Translate(),
                    icon = VEF_HediffComp_Refuelable.SetTargetFuelLevelCommand
                };
            }
            if (this.Props.showFuelGizmo && Find.Selector.SingleSelectedThing == this.parent)
            {
                yield return new Gizmo_RefuelableFuelStatus
                {
                    refuelable = this
                };
            }
            if (Prefs.DevMode)
            {
                yield return new Command_Action
                {
                    defaultLabel = "Debug: Set fuel to 0",
                    action = delegate ()
                    {
                        this.fuel = 0f;
                        this.parent.BroadcastCompSignal("Refueled");
                    }
                };
                yield return new Command_Action
                {
                    defaultLabel = "Debug: Set fuel to 0.1",
                    action = delegate ()
                    {
                        this.fuel = 0.1f;
                        this.parent.BroadcastCompSignal("Refueled");
                    }
                };
                yield return new Command_Action
                {
                    defaultLabel = "Debug: Set fuel to max",
                    action = delegate ()
                    {
                        this.fuel = this.Props.fuelCapacity;
                        this.parent.BroadcastCompSignal("Refueled");
                    }
                };
            }
            yield break;
        }

        private float fuel;

        // Token: 0x0400170E RID: 5902
        private float configuredTargetFuelLevel = -1f;

        // Token: 0x0400170F RID: 5903
        private CompFlickable flickComp;

        // Token: 0x04001710 RID: 5904
        public const string RefueledSignal = "Refueled";

        // Token: 0x04001711 RID: 5905
        public const string RanOutOfFuelSignal = "RanOutOfFuel";

        // Token: 0x04001712 RID: 5906
        private static readonly Texture2D SetTargetFuelLevelCommand = ContentFinder<Texture2D>.Get("UI/Commands/SetTargetFuelLevel", true);

        // Token: 0x04001713 RID: 5907
        private static readonly Vector2 FuelBarSize = new Vector2(1f, 0.2f);

        // Token: 0x04001714 RID: 5908
        private static readonly Material FuelBarFilledMat = SolidColorMaterials.SimpleSolidColorMaterial(new Color(0.6f, 0.56f, 0.13f), false);

        // Token: 0x04001715 RID: 5909
        private static readonly Material FuelBarUnfilledMat = SolidColorMaterials.SimpleSolidColorMaterial(new Color(0.3f, 0.3f, 0.3f), false);
    }
}
