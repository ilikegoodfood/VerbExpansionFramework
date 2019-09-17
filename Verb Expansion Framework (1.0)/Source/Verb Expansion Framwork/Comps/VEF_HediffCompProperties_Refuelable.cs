using System;
using System.Collections.Generic;
using System.Linq;
using Verse;
using RimWorld;
using UnityEngine;

namespace VerbExpansionFramework
{
    class VEF_HediffCompProperties_Refuelable : HediffCompProperties
    {
        public VEF_HediffCompProperties_Refuelable()
        {
            this.compClass = typeof(VEF_HediffComp_Refuelable);
        }

        public string FuelLabel
        {
            get
            {
                return this.fuelLabel.NullOrEmpty() ? "Fuel".Translate() : this.fuelLabel;
            }
        }

        public string FuelGizmoLabel
        {
            get
            {
                return this.fuelGizmoLabel.NullOrEmpty() ? "Fuel".Translate() : this.fuelGizmoLabel;
            }
        }

        public Texture2D FuelIcon
        {
            get
            {
                if (this.fuelIcon == null)
                {
                    if (!this.fuelIconPath.NullOrEmpty())
                    {
                        this.fuelIcon = ContentFinder<Texture2D>.Get(this.fuelIconPath, true);
                    }
                    else
                    {
                        ThingDef thingDef;
                        if (this.fuelFilter.AnyAllowedDef != null)
                        {
                            thingDef = this.fuelFilter.AnyAllowedDef;
                        }
                        else
                        {
                            thingDef = ThingDefOf.Chemfuel;
                        }
                        this.fuelIcon = thingDef.uiIcon;
                    }
                }
                return this.fuelIcon;
            }
        }

        public float FuelMultiplierCurrentDifficulty
        {
            get
            {
                if (this.factorByDifficulty)
                {
                    return this.fuelMultiplier / Find.Storyteller.difficulty.maintenanceCostFactor;
                }
                return this.fuelMultiplier;
            }
        }

        public override void PostLoad()
        {
            this.fuelFilter.ResolveReferences();
        }

        public override IEnumerable<string> ConfigErrors(HediffDef parentDef)
        {
            foreach (string err in base.ConfigErrors(parentDef))
            {
                yield return err;
            }
            if (this.destroyOnNoFuel && this.initialFuelPercent <= 0f)
            {
                yield return "Refuelable component has destroyOnNoFuel, but initialFuelPercent <= 0";
            }
            yield break;
        }

        public float fuelConsumptionRate = 1f;

        public float fuelCapacity = 2f;

        public float initialFuelPercent;

        public float autoRefuelPercent = 0.3f;

        public float fuelConsumptionPerTickInRain;

        public ThingFilter fuelFilter;

        public bool destroyOnNoFuel;

        public bool consumeFuelOnlyWhenUsed;

        public bool showFuelGizmo;

        public bool targetFuelLevelConfigurable;

        public float initialConfigurableTargetFuelLevel;

        public bool drawOutOfFuelOverlay = true;

        public float minimumFueledThreshold;

        public bool drawFuelGaugeInMap;

        public bool atomicFueling;

        private float fuelMultiplier = 1f;

        public bool factorByDifficulty;

        public string fuelLabel;

        public string fuelGizmoLabel;

        public string outOfFuelMessage;

        public string fuelIconPath;

        private Texture2D fuelIcon;
    }
}
