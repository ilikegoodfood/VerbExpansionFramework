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
            Log.Message("VEF :: Performing Hamrony Patches");
            HarmonyInstance.DEBUG = false;
            HarmonyInstance harmony = HarmonyInstance.Create(id: "com.framework.expansion.verb");
            harmony.Patch(original: AccessTools.Method(type: typeof(Alert_BrawlerHasRangedWeapon), name: nameof(Alert_BrawlerHasRangedWeapon.GetReport)), prefix: null, postfix: new HarmonyMethod(type: patchType, name: nameof(Alert_BrawlerHasRangedWeapon_GetReportPostfix)));
            harmony.Patch(original: AccessTools.Method(type: typeof(AttackTargetsCache), name: nameof(AttackTargetsCache.GetPotentialTargetsFor)), prefix: null, postfix: new HarmonyMethod(type: patchType, name: nameof(AttackTargetsCache_GetPotentialTargetsForPostfix)));
            harmony.Patch(original: AccessTools.Constructor(type: typeof(BattleLogEntry_ExplosionImpact), parameters: new Type[] { typeof(Thing), typeof(Thing), typeof(ThingDef), typeof(ThingDef), typeof(DamageDef) }), prefix: new HarmonyMethod(type: patchType, name: nameof(BattleLogEntry_WeaponDefGrammarPrefix)), postfix: null);
            harmony.Patch(original: AccessTools.Constructor(type: typeof(BattleLogEntry_RangedFire), parameters: new Type[] { typeof(Thing), typeof(Thing), typeof(ThingDef), typeof(ThingDef), typeof(bool) }), prefix: new HarmonyMethod(type: patchType, name: nameof(BattleLogEntry_WeaponDefGrammarPrefix)), postfix: null);
            harmony.Patch(original: AccessTools.Constructor(type: typeof(BattleLogEntry_RangedImpact), parameters: new Type[] { typeof(Thing), typeof(Thing), typeof(Thing), typeof(ThingDef), typeof(ThingDef), typeof(ThingDef) }), prefix: new HarmonyMethod(type: patchType, name: nameof(BattleLogEntry_WeaponDefGrammarPrefix)), postfix: null);
            harmony.Patch(original: AccessTools.Method(type: typeof(Command_VerbTarget), name: nameof(Command_VerbTarget.ProcessInput)), prefix: null, postfix: new HarmonyMethod(type: patchType, name: nameof(Command_VerbTarget_ProcessInputPostfix)));
            harmony.Patch(original: AccessTools.Method(type: typeof(FloatMenuUtility), name: nameof(FloatMenuUtility.GetAttackAction)), prefix: null, postfix: new HarmonyMethod(type: patchType, name: nameof(FloatMenuUtility_GetAttackActionPostfix)));
            harmony.Patch(original: AccessTools.Method(type: typeof(HealthCardUtility), name: "GenerateSurgeryOption"), prefix: null, postfix: new HarmonyMethod(type: patchType, name: nameof(HealthCardUtility_GenerateSurgeryOptionPostfix)));
            harmony.Patch(original: AccessTools.Method(type: typeof(HediffSet), name: "CalculateBleedRate"), prefix: null, postfix: new HarmonyMethod(type: patchType, name: nameof(HediffSet_CalculateBleedRatePostfix)));
            harmony.Patch(original: AccessTools.Method(type: typeof(JobDriver_Wait), name: "CheckForAutoAttack"), prefix: null, postfix: new HarmonyMethod(type: patchType, name: nameof(JobDriver_Wait_CheckForAutoAttackPostfix)));
            harmony.Patch(original: AccessTools.Method(type: typeof(PawnAttackGizmoUtility), name: "GetSquadAttackGizmo"), prefix: null, postfix: new HarmonyMethod(type: patchType, name: nameof(PawnAttackGizmoUtility_GetSquadAttackGizmo)));
            harmony.Patch(original: MB_Pawn_DraftController_GetGizmo(), prefix: null, postfix: null, transpiler: new HarmonyMethod(type: patchType, name: nameof(Pawn_DraftController_GetGizmosTranspiler)));
            harmony.Patch(original: AccessTools.Method(type: typeof(Pawn), name: nameof(Pawn.TryGetAttackVerb)), prefix: null, postfix: new HarmonyMethod(type: patchType, name: nameof(Pawn_TryGetAttackVerbPostfix)));
            harmony.Patch(original: AccessTools.Method(type: typeof(ThoughtWorker_IsCarryingRangedWeapon), name: "CurrentStateInternal"), prefix: null, postfix: new HarmonyMethod(type: patchType, name: nameof(ThoughtWorker_IsCarryingRangedWeapon_CurrentStateInternalPostfix)));
        }

        private static void Alert_BrawlerHasRangedWeapon_GetReportPostfix(ref AlertReport __result)
        {
            Log.Message("Postfixing");
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

            Pawn pawn = initiator as Pawn;

            if (initiator != null)
            {
                Verb usedVerb = pawn.GetComp<VEF_Comp_Pawn_RangedVerbs>().CurRangedVerb;

                if (usedVerb == null)
                {
                    Thing enemyTarget = pawn.mindState.enemyTarget;
                    float targetKeepRadius = 65f;
                    if (enemyTarget != null && (enemyTarget.Destroyed || Find.TickManager.TicksGame - pawn.mindState.lastEngageTargetTick > 400 || !pawn.CanReach(enemyTarget, PathEndMode.Touch, Danger.Deadly, false, TraverseMode.ByPawn) || (float)(pawn.Position - enemyTarget.Position).LengthHorizontalSquared > targetKeepRadius * targetKeepRadius || ((IAttackTarget)enemyTarget).ThreatDisabled(pawn)))
                    {
                        enemyTarget = null;
                    }
                    usedVerb = pawn.GetComp<VEF_Comp_Pawn_RangedVerbs>().TryGetRangedVerb(enemyTarget);
                }
                if (usedVerb.EquipmentCompSource != null)
                {
                    return;
                }
                else if (usedVerb.HediffCompSource != null)
                {
                    ThingDef tempThingDef = new ThingDef() { defName = "tempThingDef :: " + usedVerb.HediffCompSource.parent.def.label, label = usedVerb.HediffCompSource.parent.def.label, thingClass = typeof(ThingWithComps), category = ThingCategory.Item };

                    MI_GiveShortHash.Invoke(null, new object[] { tempThingDef, tempThingDef.GetType() });
                    Traverse.Create(tempThingDef).Field("verbs").SetValue(new List<VerbProperties>() { usedVerb.verbProps });

                    weaponDef = tempThingDef;
                }
                else
                {
                    weaponDef = pawn.def;
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
            __result = VEF_FloatMenuUtility.GetRangedAttackAction(pawn, target, out failStr);
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

        private static void PawnAttackGizmoUtility_GetSquadAttackGizmo(ref Gizmo __result)
        {
            Command_Target command_Target = (Command_Target)__result;
            if (VEF_Comp_Pawn_RangedVerbs.ShouldUseSquadAttackGizmo())
            {
                command_Target.defaultLabel = "CommandSquadEquipmentAttack".Translate();
                command_Target.defaultDesc = "CommandSquadEquipmentAttackDesc".Translate();
                command_Target.action = delegate (Thing target)
                {
                    IEnumerable<Pawn> pawns = Find.Selector.SelectedObjects.Where(delegate (object x)
                    {
                        Pawn pawn3 = x as Pawn;
                        return pawn3 != null && pawn3.IsColonistPlayerControlled && pawn3.Drafted;
                    }).Cast<Pawn>();
                    foreach (Pawn pawn2 in pawns)
                    {
                        if (pawn2.equipment.Primary != null)
                        {
                            pawn2.GetComp<VEF_Comp_Pawn_RangedVerbs>().SetCurRangedVerb(pawn2.equipment.PrimaryEq.PrimaryVerb, null);
                        }
                        string text;
                        Action attackAction = FloatMenuUtility.GetAttackAction(pawn2, target, out text);
                        if (attackAction != null)
                        {
                            attackAction();
                        }
                    }
                };
            }
            __result = command_Target;
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

        [HarmonyPriority(1200)]
        private static void Pawn_TryGetAttackVerbPostfix(Pawn __instance, ref Verb __result, ref Thing target)
        {
            Verb tempVerb = __instance.GetComp<VEF_Comp_Pawn_RangedVerbs>().TryGetRangedVerb(target);
            if (tempVerb != null)
            {
                __result = tempVerb;
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
    }
}