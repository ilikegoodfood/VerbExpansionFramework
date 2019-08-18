using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using Harmony;
using RimWorld;
using RimWorld.Planet;
using Verse;
using Verse.AI;
using UnityEngine;

namespace VerbExpansionFramework
{

    [StaticConstructorOnStartup]
    public class HarmonyPatches
    {
        private static readonly Type patchType = typeof(HarmonyPatches);
        private static MethodBase MB_Pawn_DraftController_GetGizmo()
        {
            var predicateClass = typeof(Pawn_DraftController).GetNestedTypes(AccessTools.all).FirstOrDefault(t => t.FullName.Contains("c__Iterator0"));
            return predicateClass.GetMethods(AccessTools.all).FirstOrDefault(m => m.Name.Contains("MoveNext"));
        }

        static HarmonyPatches()
        {
            // Harmony Setup and Debug
            Log.Message("VEF :: Performing Hamrony Patches");
            HarmonyInstance.DEBUG = false;
            HarmonyInstance harmony = HarmonyInstance.Create(id: "com.framework.expansion.verb");

            // Harmony Patches required for Core operation.
            harmony.Patch(original: AccessTools.Method(type: typeof(Alert_BrawlerHasRangedWeapon), name: nameof(Alert_BrawlerHasRangedWeapon.GetReport)), prefix: null, postfix: new HarmonyMethod(type: patchType, name: nameof(Alert_BrawlerHasRangedWeapon_GetReportPostfix)));
            harmony.Patch(original: AccessTools.Method(type: typeof(AttackTargetsCache), name: nameof(AttackTargetsCache.GetPotentialTargetsFor)), prefix: null, postfix: new HarmonyMethod(type: patchType, name: nameof(AttackTargetsCache_GetPotentialTargetsForPostfix)));
            harmony.Patch(original: AccessTools.Constructor(type: typeof(BattleLogEntry_RangedFire), parameters: new Type[] { typeof(Thing), typeof(Thing), typeof(ThingDef), typeof(ThingDef), typeof(bool) }), prefix: new HarmonyMethod(type: patchType, name: nameof(BattleLogEntry_WeaponDefGrammarPrefix)), postfix: null);
            harmony.Patch(original: AccessTools.Constructor(type: typeof(BattleLogEntry_RangedImpact), parameters: new Type[] { typeof(Thing), typeof(Thing), typeof(Thing), typeof(ThingDef), typeof(ThingDef), typeof(ThingDef) }), prefix: new HarmonyMethod(type: patchType, name: nameof(BattleLogEntry_WeaponDefGrammarPrefix)), postfix: null);
            harmony.Patch(original: AccessTools.Method(type: typeof(Command_VerbTarget), name: nameof(Command_VerbTarget.ProcessInput)), prefix: null, postfix: new HarmonyMethod(type: patchType, name: nameof(Command_VerbTarget_ProcessInputPostfix)));
            harmony.Patch(original: AccessTools.Method(type: typeof(FloatMenuUtility), name: nameof(FloatMenuUtility.GetAttackAction)), prefix: null, postfix: new HarmonyMethod(type: patchType, name: nameof(FloatMenuUtility_GetAttackActionPostfix)));
            harmony.Patch(original: AccessTools.Method(type: typeof(HealthCardUtility), name: "GenerateSurgeryOption"), prefix: null, postfix: new HarmonyMethod(type: patchType, name: nameof(HealthCardUtility_GenerateSurgeryOptionPostfix)));
            harmony.Patch(original: AccessTools.Property(type: typeof(Pawn), name: nameof(Pawn.HealthScale)).GetGetMethod(true), prefix: null, postfix: new HarmonyMethod(type: patchType, name: nameof(Pawn_GetHealthScalePostfix)));
            harmony.Patch(original: AccessTools.Method(type: typeof(HediffSet), name: "CalculateBleedRate"), prefix: null, postfix: new HarmonyMethod(type: patchType, name: nameof(HediffSet_CalculateBleedRatePostfix)));
            harmony.Patch(original: AccessTools.Method(type: typeof(JobDriver_Wait), name: "CheckForAutoAttack"), prefix: null, postfix: new HarmonyMethod(type: patchType, name: nameof(JobDriver_Wait_CheckForAutoAttackPostfix)));
            if (!VEF_ModCompatibilityCheck.enabled_CombatExtended)
            {
                harmony.Patch(original: AccessTools.Method(type: typeof(Pawn), name: nameof(Pawn.TryGetAttackVerb)), prefix: null, postfix: new HarmonyMethod(type: patchType, name: nameof(Pawn_TryGetAttackVerbPostfix)));
                harmony.Patch(original: AccessTools.Method(type: typeof(PawnAttackGizmoUtility), name: "GetSquadAttackGizmo"), prefix: null, postfix: new HarmonyMethod(type: patchType, name: nameof(PawnAttackGizmoUtility_GetSquadAttackGizmoPostfix)));
            }
            harmony.Patch(original: MB_Pawn_DraftController_GetGizmo(), prefix: null, postfix: null, transpiler: new HarmonyMethod(type: patchType, name: nameof(Pawn_DraftController_GetGizmosTranspiler)));
            harmony.Patch(original: AccessTools.Method(type: typeof(Pawn_HealthTracker), name: nameof(Pawn_HealthTracker.PreApplyDamage)), prefix: null, postfix: new HarmonyMethod(type: patchType, name: nameof(Pawn_HealthTracker_PreApplyDamagePostfix)));
            harmony.Patch(original: AccessTools.Method(type: typeof(SmokepopBelt), name: nameof(SmokepopBelt.CheckPreAbsorbDamage)), prefix: null, postfix: new HarmonyMethod(type: patchType, name: nameof(SmokepopBelt_CheckPreAbsorbDamagePostfix)));
            harmony.Patch(original: AccessTools.Method(type: typeof(ThoughtWorker_IsCarryingRangedWeapon), name: "CurrentStateInternal"), prefix: null, postfix: new HarmonyMethod(type: patchType, name: nameof(ThoughtWorker_IsCarryingRangedWeapon_CurrentStateInternalPostfix)));
            harmony.Patch(original: AccessTools.Method(type: typeof(Verb_ShootOneUse), name: "SelfConsume"), prefix: null, postfix: new HarmonyMethod(type: patchType, name: nameof(Verb_ShootOneUse_SelfConsume)));

            // Harmony Patches for Mod Compatibility
            if (VEF_ModCompatibilityCheck.enabled_rooloDualWield)
            {
                // Dual Wield
                harmony.Patch(original: AccessTools.Method(type: GenTypes.GetTypeInAnyAssemblyNew("DualWield.Ext_Verb", "DualWield"), name: "OffhandTryStartCastOn"), prefix: new HarmonyMethod(type: patchType, name: nameof(DualWield_Ext_Verb_OffhandTryStartCastOn)), postfix: null);
            }

            if (VEF_ModCompatibilityCheck.enabled_RangeAnimalFramework)
            {
                //Range Animal Framework
                harmony.Patch(original: AccessTools.Method(type: GenTypes.GetTypeInAnyAssemblyNew("AnimalRangeAttack.ARA_FightAI_Patch", "AnimalRangeAttack"), name: "Prefix"), prefix: null, postfix: null, transpiler: new HarmonyMethod(type: patchType, name: nameof(PrefixSlayerTranspiler)));
                harmony.Patch(original: AccessTools.Method(type: GenTypes.GetTypeInAnyAssemblyNew("AnimalRangeAttack.ARA__ManHunter_Patch", "AnimalRangeAttack"), name: "Prefix"), prefix: null, postfix: null, transpiler: new HarmonyMethod(type: patchType, name: nameof(PrefixSlayerTranspiler)));
                harmony.Patch(original: AccessTools.Method(type: GenTypes.GetTypeInAnyAssemblyNew("AnimalRangeAttack.ARA__VerbCheck_Patch", "AnimalRangeAttack"), name: "Prefix"), prefix: null, postfix: null, transpiler: new HarmonyMethod(type: patchType, name: nameof(PrefixSlayerTranspiler)));
            }
        }

        // Harmony Patches required for Core operation.
        private static void Alert_BrawlerHasRangedWeapon_GetReportPostfix(ref AlertReport __result)
        {
            // Log.Message("Postfixing");
            IEnumerable<Pawn> brawlersWithRangedWeapon = BrawlersWithRangedWeapon(__result);
            IEnumerable<Pawn> brawlersWithRangedHediff = BrawlersWithRangedHediff();
            
            if (brawlersWithRangedWeapon == null)
            {
                __result = AlertReport.CulpritsAre(brawlersWithRangedHediff);
            }
            else if (brawlersWithRangedHediff == null)
            {
                __result = AlertReport.CulpritsAre(brawlersWithRangedWeapon);
            }
            else
            {
                __result = AlertReport.CulpritsAre(brawlersWithRangedWeapon.Concat(brawlersWithRangedHediff));
            }
            return;
        }

        private static IEnumerable<Pawn> BrawlersWithRangedHediff()
        {
            foreach (Pawn pawn in PawnsFinder.AllMaps_FreeColonistsSpawned)
            {
                if (pawn.story.traits.HasTrait(TraitDefOf.Brawler) && !(pawn.equipment.Primary != null && pawn.equipment.Primary.def.IsRangedWeapon) && pawn.health.hediffSet.GetHediffsVerbs() != null)
                {
                    foreach (Verb verb in pawn.health.hediffSet.GetHediffsVerbs())
                    {
                        if (!verb.IsMeleeAttack)
                        {
                            yield return pawn;
                        }
                    }
                }
            }
        }

        private static IEnumerable<Pawn> BrawlersWithRangedWeapon(AlertReport report)
        {
            IEnumerable<GlobalTargetInfo> resultTargInfo = report.culprits;

            if (resultTargInfo != null)
            {
                foreach (GlobalTargetInfo targInfo in resultTargInfo)
                {
                    yield return (Pawn)targInfo.Thing;
                }
            }
        }

        private static void AttackTargetsCache_GetPotentialTargetsForPostfix(IAttackTargetSearcher th, ref List<IAttackTarget> __result)
        {
            Thing thing = th.Thing;
            Pawn pawn = thing as Pawn;

            if (pawn != null && pawn.InAggroMentalState)
            {
                foreach (Pawn pawn2 in pawn.Map.mapPawns.AllPawnsSpawned)
                {
                    if (pawn2.RaceProps.ToolUser)
                    {
                        __result.Add(pawn2);
                    }
                }
            }
            return;
        }

        private static void BattleLogEntry_WeaponDefGrammarPrefix(Thing initiator, ref ThingDef weaponDef)
        {
            MethodInfo MI_GiveShortHash = AccessTools.Method(type: typeof(ShortHashGiver), name: "GiveShortHash", parameters: new Type[] { typeof(Def), typeof(Type) });

            if (initiator is Pawn pawn)
            {
                Verb usedVerb = pawn.GetComp<VEF_Comp_Pawn_RangedVerbs>().CurRangedVerb;

                if (usedVerb == null)
                {
                    usedVerb = pawn.GetComp<VEF_Comp_Pawn_RangedVerbs>().TryGetRangedVerb((pawn.mindState == null) ? null: pawn.mindState.enemyTarget) ;
                }
                if (usedVerb == null)
                {
                    ThingDef tempThingDef = new ThingDef() { defName = "tempThingDef :: " + pawn.Label, label = "VerbError".Translate(), thingClass = typeof(ThingWithComps), category = ThingCategory.Item };

                    MI_GiveShortHash.Invoke(null, new object[] { tempThingDef, tempThingDef.GetType() });
                    // Traverse.Create(tempThingDef).Field("verbs").SetValue(new List<VerbProperties>() { usedVerb.verbProps });

                    weaponDef = tempThingDef;
                    return;
                }
                else if (pawn.equipment != null && pawn.equipment.Primary != null && usedVerb.EquipmentSource != null && usedVerb.EquipmentSource.def == pawn.equipment.Primary.def)
                {
                    return;
                }
                else if (usedVerb.EquipmentSource != null)
                {
                    weaponDef = usedVerb.EquipmentSource.def;
                }
                else if (usedVerb.HediffSource != null)
                {
                    ThingDef tempThingDef = new ThingDef() { defName = "tempThingDef :: " + usedVerb.HediffSource.def.label, label = (usedVerb.verbProps.label == usedVerb.HediffSource.def.label) ? usedVerb.HediffSource.def.label : usedVerb.verbProps.label, thingClass = typeof(ThingWithComps), category = ThingCategory.Item };

                    MI_GiveShortHash.Invoke(null, new object[] { tempThingDef, tempThingDef.GetType() });
                    Traverse.Create(tempThingDef).Field("verbs").SetValue(new List<VerbProperties>() { usedVerb.verbProps });

                    weaponDef = tempThingDef;
                }
                else
                {
                    if (usedVerb.verbProps.label == pawn.def.label)
                    {
                        weaponDef = pawn.def;
                    }
                    else
                    {
                        ThingDef tempThingDef = new ThingDef() { defName = "tempThingDef :: " + pawn.Label, label = usedVerb.verbProps.label, thingClass = typeof(ThingWithComps), category = ThingCategory.Item };

                        MI_GiveShortHash.Invoke(null, new object[] { tempThingDef, tempThingDef.GetType() });
                        Traverse.Create(tempThingDef).Field("verbs").SetValue(new List<VerbProperties>() { usedVerb.verbProps });

                        weaponDef = tempThingDef;
                    }

                }
            }
            return;
        }

        private static void Command_VerbTarget_ProcessInputPostfix(Command_VerbTarget __instance)
        {
            __instance.verb.CasterPawn.GetComp<VEF_Comp_Pawn_RangedVerbs>().SetCurRangedVerb(__instance.verb, null);
            return;
        }

        private static void FloatMenuUtility_GetAttackActionPostfix(Pawn pawn, LocalTargetInfo target, out string failStr, ref Action __result)
        {
            __result = VEF_FloatMenuUtility.GetAttackAction(pawn, target, out failStr);
            return;
        }

        private static void HealthCardUtility_GenerateSurgeryOptionPostfix(Thing thingForMedBills, RecipeDef recipe, ref FloatMenuOption __result)
        {
            Pawn pawn = (Pawn)thingForMedBills;
            if (pawn == null || pawn.story == null || !pawn.story.traits.HasTrait(TraitDefOf.Brawler))
            {
                return;
            }
            else if (recipe.addsHediff != null && recipe.addsHediff.HasComp(typeof(HediffComp_VerbGiver)))
            {
                bool flag = false;
                foreach (VerbProperties verb in recipe.addsHediff.CompProps<HediffCompProperties_VerbGiver>().verbs)
                {
                    if (!verb.IsMeleeAttack)
                    {
                        flag = true;
                        break;
                    }
                }
                if (flag)
                {
                    __result.Label = __result.Label + " " + "EquipWarningBrawler".Translate();
                }
            }
            return;
        }

        private static void Pawn_GetHealthScalePostfix(Pawn __instance, ref float __result)
        {
            float healthScaleOffset = 0f;
            float healthScaleFactor = 1f;

            foreach (Hediff hediff in __instance.health.hediffSet.hediffs)
            {
                if (hediff.TryGetComp<VEF_HediffComp_HealthModifier>() != null)
                {
                    healthScaleOffset += hediff.TryGetComp<VEF_HediffComp_HealthModifier>().Props.healthScaleOffset;
                    healthScaleFactor += hediff.TryGetComp<VEF_HediffComp_HealthModifier>().Props.healthScaleFactorOffset;
                }

                if (VEF_ModCompatibilityCheck.enabled_VariableHealthFramework)
                {
                    try
                    {
                        ((Action)(() =>
                        {
                            if (hediff.TryGetComp<VariableHealthFramework.VHF_HediffComp_HealthModifier>() != null)
                            {
                                healthScaleOffset += hediff.TryGetComp<VariableHealthFramework.VHF_HediffComp_HealthModifier>().Props.healthScaleOffset;
                                healthScaleFactor += hediff.TryGetComp<VariableHealthFramework.VHF_HediffComp_HealthModifier>().Props.healthScaleFactorOffset;
                            }
                        }))();
                    }
                    catch (TypeLoadException ex)
                    {
                        
                    }
                }
            }

            healthScaleOffset += __instance.health.capacities.GetLevel(VEF_DefOf.HealthModifier) - 1f;
            healthScaleFactor += __instance.health.capacities.GetLevel(VEF_DefOf.HealthModifierFactor) - 1f;

            __result = Mathf.Clamp((__result + healthScaleOffset) * (healthScaleFactor), 0.1f, float.MaxValue);
            return;
        }

        private static void HediffSet_CalculateBleedRatePostfix(HediffSet __instance, ref float __result)
        {
            __result *= __instance.pawn.health.capacities.GetLevel(VEF_DefOf.BleedRate);
            return;
        }

        private static void JobDriver_Wait_CheckForAutoAttackPostfix(JobDriver_Wait __instance)
        {
            if (__instance.pawn.Downed || __instance.pawn.stances.FullBodyBusy || __instance.pawn.story != null)
            {
                return;
            }
            if (((__instance.pawn.story != null && !__instance.pawn.story.WorkTagIsDisabled(WorkTags.Violent)) || __instance.pawn.story == null) && __instance.job.def == JobDefOf.Wait_Combat && (__instance.pawn.drafter == null || __instance.pawn.drafter.FireAtWill))
            {
                Verb currentEffectiveVerb = __instance.pawn.CurrentEffectiveVerb;
                if (currentEffectiveVerb != null && !currentEffectiveVerb.verbProps.IsMeleeAttack)
                {
                    TargetScanFlags targetScanFlags = TargetScanFlags.NeedLOSToPawns | TargetScanFlags.NeedLOSToNonPawns | TargetScanFlags.NeedThreat;
                    if (currentEffectiveVerb.IsIncendiary())
                    {
                        targetScanFlags |= TargetScanFlags.NeedNonBurning;
                    }
                    Thing thing = (Thing)AttackTargetFinder.BestShootTargetFromCurrentPosition(__instance.pawn, targetScanFlags, null, 0f, 9999f);
                    if (thing != null)
                    {
                        __instance.pawn.TryStartAttack(thing);
                        __instance.collideWithPawns = true;
                        return;
                    }
                }
            }
            return;
        }

        [HarmonyPriority(1200)]
        private static void Pawn_TryGetAttackVerbPostfix(Pawn __instance, ref Verb __result, ref Thing target)
        {
            if (__instance.TryGetComp<VEF_Comp_Pawn_RangedVerbs>() != null)
            {
                Verb tempVerb = __instance.TryGetComp<VEF_Comp_Pawn_RangedVerbs>().TryGetRangedVerb(target);
                if (tempVerb != null)
                {
                    __result = tempVerb;
                }
            }
            return;
        }

        private static void PawnAttackGizmoUtility_GetSquadAttackGizmoPostfix(Pawn pawn, ref Gizmo __result)
        {
            Command_Target command_Target = (Command_Target)__result;
            bool flag = false;
            List<object> selectedObjectsListForReading = Find.Selector.SelectedObjectsListForReading;
            if (selectedObjectsListForReading.Count > 1)
            {
                for (int i = 0; i < selectedObjectsListForReading.Count; i++)
                {
                    Pawn pawn2 = selectedObjectsListForReading[i] as Pawn;
                    if (pawn2 != null && pawn2.IsColonistPlayerControlled && pawn2.GetComp<VEF_Comp_Pawn_RangedVerbs>().CurRangedVerb != null && pawn2.GetComp<VEF_Comp_Pawn_RangedVerbs>().CurRangedVerb.IsStillUsableBy(pawn2) && (pawn2.GetComp<VEF_Comp_Pawn_RangedVerbs>().CurRangedVerb.EquipmentSource == null || pawn2.GetComp<VEF_Comp_Pawn_RangedVerbs>().CurRangedVerb.EquipmentSource != pawn2.equipment.Primary))
                    {
                        flag = true;
                    }
                }
            }
            if (flag || VEF_Comp_Pawn_RangedVerbs.ShouldUseSquadAttackGizmo())
            {
                command_Target.defaultLabel = "CommandSquadEquipmentAttack".Translate();
                command_Target.defaultDesc = "CommandSquadEquipmentAttackDesc".Translate();
                if (FloatMenuUtility.GetAttackAction(pawn, LocalTargetInfo.Invalid, out string str) == null)
                {
                    command_Target.Disable(str.CapitalizeFirst() + ".");
                }
                command_Target.action = delegate (Thing target)
                {
                    IEnumerable<Pawn> pawns = Find.Selector.SelectedObjects.Where(delegate (object x)
                    {
                        Pawn pawn3 = x as Pawn;
                        return pawn3 != null && pawn3.IsColonistPlayerControlled && pawn3.Drafted;
                    }).Cast<Pawn>();
                    foreach (Pawn pawn2 in pawns)
                    {
                        if (pawn2.equipment != null && pawn2.equipment.Primary != null && pawn2.GetComp<VEF_Comp_Pawn_RangedVerbs>().CurRangedVerb.EquipmentSource.def != pawn2.equipment.Primary.def)
                        {
                            pawn2.GetComp<VEF_Comp_Pawn_RangedVerbs>().SetCurRangedVerb(pawn2.equipment.PrimaryEq.PrimaryVerb, null);
                        }
                        string text;
                        FloatMenuUtility.GetAttackAction(pawn2, target, out text)?.Invoke();
                    }
                };
                __result = command_Target;
            }
            return;
        }

        private static IEnumerable<CodeInstruction> Pawn_DraftController_GetGizmosTranspiler(IEnumerable<CodeInstruction> codeInstructions)
        {
            MethodInfo MI_CurRangedVerb = AccessTools.Property(type: typeof(VEF_Comp_Pawn_RangedVerbs), name: nameof(VEF_Comp_Pawn_RangedVerbs.CurRangedVerb)).GetGetMethod();
            MethodInfo MI_GetComp = AccessTools.Method(type: typeof(ThingWithComps), name: "GetComp").MakeGenericMethod(typeof(VEF_Comp_Pawn_RangedVerbs));

            List<CodeInstruction> instructionList = codeInstructions.ToList();

            int count = 0;
            int count2 = 0;
            bool flag = false;
            bool flag2 = false;
            int index = -1;
            int index2 = -1;

            for (int i = 0; i < instructionList.Count; i++)
            {
                if (!flag && instructionList[i].opcode == OpCodes.Brfalse)
                {
                    count += 1;
                }

                if (!flag2)
                {
                    yield return instructionList[i];
                }
                else
                {
                    if (count2 < 2 && instructionList[i].opcode == OpCodes.Brfalse)
                    {
                        count2 += 1;
                    }
                    else if (count2 == 2)
                    {
                        flag2 = false;
                    }
                }

                if (!flag && count == 2)
                {
                    index2 = i;

                    flag = true;
                    index = i + 3;
                }
                if (i == index)
                {
                    yield return new CodeInstruction(opcode: OpCodes.Callvirt, operand: MI_GetComp);
                    yield return new CodeInstruction(opcode: OpCodes.Callvirt, operand: MI_CurRangedVerb);
                    yield return new CodeInstruction(opcode: OpCodes.Brfalse, operand: instructionList[index2].operand);
                    yield return new CodeInstruction(opcode: OpCodes.Ldarg_0);

                    flag2 = true;
                }
            }
        }

        private static void Pawn_HealthTracker_PreApplyDamagePostfix(Pawn_HealthTracker __instance, DamageInfo dinfo)
        {
            if(dinfo.Instigator != null)
            {
                Pawn pawn = (Pawn)VEF_ReflectionData.FI_Pawn_HealthTracker_pawn.GetValue(__instance);
                foreach (Hediff hediff in pawn.health.hediffSet.hediffs)
                {
                    if (hediff.TryGetComp<VEF_HediffComp_SmokepopDefense>() != null)
                    {
                        hediff.TryGetComp<VEF_HediffComp_SmokepopDefense>().TryTriggerSmokepopDefense(dinfo);
                    }
                    if (VEF_ModCompatibilityCheck.enabled_SmokepopDefenseFramework)
                    {
                        try
                        {
                            ((Action)(() =>
                            {
                                if (hediff.TryGetComp<SmokepopCompFramework.SCF_HediffComp_SmokepopDefense>() != null)
                                {
                                    hediff.TryGetComp<SmokepopCompFramework.SCF_HediffComp_SmokepopDefense>().TryTriggerSmokepopDefense(dinfo);
                                }
                            }))();
                        }
                        catch (TypeLoadException ex)
                        {

                        }
                    }
                }
            }
            return;
        }

        private static void SmokepopBelt_CheckPreAbsorbDamagePostfix(SmokepopBelt __instance, DamageInfo dinfo)
        {
            if (!dinfo.Def.isExplosive && dinfo.Def.harmsHealth && dinfo.Def.ExternalViolenceFor(__instance.Wearer))
            {
                if (dinfo.Instigator is Pawn instigatorPawn && instigatorPawn.GetComp<VEF_Comp_Pawn_RangedVerbs>().CurRangedVerb != null && !(dinfo.Weapon != null && instigatorPawn.GetComp<VEF_Comp_Pawn_RangedVerbs>().CurRangedVerb.EquipmentSource != null && dinfo.Weapon == instigatorPawn.GetComp<VEF_Comp_Pawn_RangedVerbs>().CurRangedVerb.EquipmentSource.def) && instigatorPawn.Position.DistanceTo(__instance.Wearer.Position) > 1f)
                {
                    IntVec3 position = __instance.Wearer.Position;
                    Map map = __instance.Wearer.Map;
                    float statValue = __instance.GetStatValue(StatDefOf.SmokepopBeltRadius, true);
                    DamageDef smoke = DamageDefOf.Smoke;
                    Thing instigator = null;
                    ThingDef gas_Smoke = ThingDefOf.Gas_Smoke;
                    GenExplosion.DoExplosion(position, map, statValue, smoke, instigator, -1, -1f, null, null, null, null, gas_Smoke, 1f, 1, false, null, 0f, 1, 0f, false);
                    __instance.Destroy(DestroyMode.Vanish);
                }
            }
            return;
        }

        private static void ThoughtWorker_IsCarryingRangedWeapon_CurrentStateInternalPostfix(ref ThoughtState __result, Pawn p)
        {
            FieldInfo FI_stageInex = AccessTools.Field(type: typeof(ThoughtState), name: "stageIndex");

            if (!__result.Active)
            {
                foreach (Verb verb in p.health.hediffSet.GetHediffsVerbs())
                {
                    if (!verb.IsMeleeAttack)
                    {
                        __result = true;
                        break;
                    }
                }
            }
            return;
        }

        private static void Verb_ShootOneUse_SelfConsume(Verb_ShootOneUse __instance)
        {
            if(__instance.HediffSource != null && __instance.HediffSource.pawn.health.hediffSet.HasHediff(__instance.HediffSource.def))
            {
                __instance.HediffSource.pawn.health.RemoveHediff(__instance.HediffSource);
            }
        }

        // Harmony Patches for Mod Compatibility
        // Diual Wield
        private static bool DualWield_Ext_Verb_OffhandTryStartCastOn(Verb instance)
        {
            if (instance.EquipmentSource == null || instance.EquipmentSource != instance.CasterPawn.equipment.Primary)
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        // Range Animal Framework
        private static IEnumerable<CodeInstruction> PrefixSlayerTranspiler(IEnumerable<CodeInstruction> codeInstructions)
        {
            yield return new CodeInstruction(opcode: OpCodes.Ldc_I4_1);
            yield return new CodeInstruction(opcode: OpCodes.Ret);
        }
    }
}