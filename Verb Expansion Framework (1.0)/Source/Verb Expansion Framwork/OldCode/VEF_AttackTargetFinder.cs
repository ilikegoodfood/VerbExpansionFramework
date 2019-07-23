using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;
using Verse.AI;
using Verse.AI.Group;
using UnityEngine;

namespace VerbExpansionFramework
{
    public static class VEF_AttackTargetFinder
    {
        public static IAttackTarget BestAttackTarget(IAttackTargetSearcher searcher, TargetScanFlags flags, Predicate<Thing> validator = null, float minDist = 0f, float maxDist = 9999f, IntVec3 locus = default(IntVec3), float maxTravelRadiusFromLocus = 3.40282347E+38f, bool canBash = false, bool canTakeTargetsCloserThanEffectiveMinRange = true)
        {
            Log.Message("Using VEF_AttackTargetFinder");
            Thing searcherThing = searcher.Thing;
            Pawn searcherPawn = searcher as Pawn;
            Verb verb = searcher.CurrentEffectiveVerb;
            Log.Message("searcher is " + searcherPawn.Label);
            Log.Message("Attack Verb is " + verb.ToString());
            if (verb == null)
            {
                Log.Error("BestAttackTarget with " + searcher.ToStringSafe<IAttackTargetSearcher>() + " who has no attack verb.", false);
                return null;
            }
            bool onlyTargetMachines = verb.IsEMP();
            float minDistSquared = minDist * minDist;
            float num = maxTravelRadiusFromLocus + verb.verbProps.range;
            float maxLocusDistSquared = num * num;
            Func<IntVec3, bool> losValidator = null;
            if ((byte)(flags & TargetScanFlags.LOSBlockableByGas) != 0)
            {
                losValidator = delegate (IntVec3 vec3)
                {
                    Gas gas = vec3.GetGas(searcherThing.Map);
                    return gas == null || !gas.def.gas.blockTurretTracking;
                };
            }
            Predicate<IAttackTarget> innerValidator = delegate (IAttackTarget t)
            {
                Log.Message("Beginning of innerValidator - Validating for " + t.Thing.Label);
                Thing thing = t.Thing;
                if (t == searcher)
                {
                    Log.Message("IAttackTarget is Searcher - Invalid target");
                    return false;
                }
                if (minDistSquared > 0f && (float)(searcherThing.Position - thing.Position).LengthHorizontalSquared < minDistSquared)
                {
                    Log.Message("Verb has minDistance and IAttackTarget is within minimum distance - Invalid target");
                    return false;
                }
                if (!canTakeTargetsCloserThanEffectiveMinRange)
                {
                    Log.Message("Searcher cannot attack targets closer that verb.verbProps.EffectiveMinRange");
                    float num2 = verb.verbProps.EffectiveMinRange(thing, searcherThing);
                    if (num2 > 0f && (float)(searcherThing.Position - thing.Position).LengthHorizontalSquared < num2 * num2)
                    {
                        Log.Message("Distance between Searcher and IAttackTarget is greater than 0, but less than verb.verbProps.EffectiveMinimumRange - Invalid target");
                        return false;
                    }
                }
                if (maxTravelRadiusFromLocus < 9999f && (float)(thing.Position - locus).LengthHorizontalSquared > maxLocusDistSquared)
                {
                    Log.Message("Seacrher has limited permissable movement and IAttackTarget is outside of that distance - Invalid target");
                    return false;
                }
                if (!searcherThing.HostileTo(thing))
                {
                    Log.Message("Searcher is not hostile towards IAttackTarget - Invalid target");
                    return false;
                }
                if (validator != null && !validator(thing))
                {
                    Log.Message("Searcher has validator and validator failed. Validator checked for " + validator.GetType().ToString());
                    return false;
                }
                if (searcherPawn != null)
                {
                    Log.Message("Searcher is Pawn - Get Lord");
                    Lord lord = searcherPawn.GetLord();
                    if (lord != null && !lord.LordJob.ValidateAttackTarget(searcherPawn, thing))
                    {
                        Log.Message("Lord is in incorrect state - Return false");
                        return false;
                    }
                }
                if ((byte)(flags & TargetScanFlags.NeedLOSToAll) != 0 && !searcherThing.CanSee(thing, losValidator))
                {
                    Log.Message("Searher needs LOS to some or all targets, but cannot see target.");
                    if (t is Pawn)
                    {
                        Log.Message("IAttackTarget is Pawn");
                        if ((byte)(flags & TargetScanFlags.NeedLOSToPawns) != 0)
                        {
                            Log.Message("Searcher needs LOS to Pawn - Invalid target");
                            return false;
                        }
                    }
                    else if ((byte)(flags & TargetScanFlags.NeedLOSToNonPawns) != 0)
                    {
                        Log.Message("IAttackTarget is nonPawn");
                        Log.Message("Searcher needs LOS to NonPawns - Invalid target");
                        return false;
                    }
                }
                if ((byte)(flags & TargetScanFlags.NeedThreat) != 0 && t.ThreatDisabled(searcher))
                {
                    Log.Message("Searcher needs Threat, IAttackTarget is not threat - Invalid target");
                    return false;
                }
                Pawn pawn = t as Pawn;
                if (onlyTargetMachines && pawn != null && pawn.RaceProps.IsFlesh)
                {
                    Log.Message("Verb is EMP and target is flesh - Invalid target");
                    return false;
                }
                if ((byte)(flags & TargetScanFlags.NeedNonBurning) != 0 && thing.IsBurning())
                {
                    Log.Message("Verb is incendiary and target is already burning - Invalid target");
                    return false;
                }
                if (searcherThing.def.race != null && searcherThing.def.race.intelligence >= Intelligence.Humanlike)
                {
                    Log.Message("Searcher is ToolUser - Validating use of Explosives");
                    CompExplosive compExplosive = thing.TryGetComp<CompExplosive>();
                    if (compExplosive != null && compExplosive.wickStarted)
                    {
                        Log.Message("Explosive is already lit - Invalid verb");
                        return false;
                    }
                }
                if (thing.def.size.x == 1 && thing.def.size.z == 1)
                {
                    if (thing.Position.Fogged(thing.Map))
                    {
                        Log.Message("IAttackTarget is in Fog - Invalid target");
                        return false;
                    }
                }
                else
                {
                    bool flag2 = false;
                    CellRect.CellRectIterator iterator = thing.OccupiedRect().GetIterator();
                    while (!iterator.Done())
                    {
                        if (!iterator.Current.Fogged(thing.Map))
                        {
                            Log.Message("IAttackTarget is not in Fog - return true");
                            flag2 = true;
                            break;
                        }
                        iterator.MoveNext();
                    }
                    if (!flag2)
                    {
                        return false;
                    }
                }
                Log.Message("IAttackTarget is valid target");
                return true;
            };
            if (HasRangedAttack(searcher))
            {
                Log.Message(searcher.Thing.Label + " has ranged verb.");
                tmpTargets.Clear();
                tmpTargets.AddRange(searcherThing.Map.attackTargetsCache.GetPotentialTargetsFor(searcher));
                Log.Message("initializing tmpTargets");
                if ((byte)(flags & TargetScanFlags.NeedReachable) != 0)
                {
                    Log.Message("TargetScanFlags.NeedReachable");
                    Predicate<IAttackTarget> oldValidator = innerValidator;
                    innerValidator = ((IAttackTarget t) => oldValidator(t) && CanReach(searcherThing, t.Thing, canBash));
                }
                bool flag = false;

                if (tmpTargets.Count == 0)
                {
                    Log.Message("There are no tmpTargets - return null");
                    return null;
                }
                else
                {
                    Log.Message("There are " + tmpTargets.Count + " tmpTargets");
                }

                for (int i = 0; i < tmpTargets.Count; i++)
                {
                    Log.Message("Iterating tmpTargets to check for targets that can be hit from current location. Iteration " + i);
                    IAttackTarget attackTarget = tmpTargets[i];
                    if (attackTarget.Thing.Position.InHorDistOf(searcherThing.Position, maxDist) && innerValidator(attackTarget) && CanShootAtFromCurrentPosition(attackTarget, searcher, verb))
                    {
                        Log.Message("tempTarget " + i + " can be hit from current position.");
                        flag = true;
                        break;
                    }
                    Log.Message("tempTarget " + i + " cannot be hit from current position.");
                }
                IAttackTarget result = null;
                if (flag)
                {
                    Log.Message("A target can be shot from current position. Clearing tempTargets outside of range and picking one.");
                    tmpTargets.RemoveAll((IAttackTarget x) => !x.Thing.Position.InHorDistOf(searcherThing.Position, maxDist) || !innerValidator(x));
                    result = GetRandomShootingTargetByScore(tmpTargets, searcher, verb);
                    Log.Message("Target that can be shot from present location is: " + result.Thing.ToString());
                }
                else
                {
                    Log.Message("No target can be hit from current position. Refining validator...");
                    Predicate<Thing> validator2;
                    if ((byte)(flags & TargetScanFlags.NeedReachableIfCantHitFromMyPos) != 0 && (byte)(flags & TargetScanFlags.NeedReachable) == 0)
                    {
                        Log.Message("Using validator2 where (byte)TargetScanFalgs flags are not 0");
                        validator2 = ((Thing t) => innerValidator((IAttackTarget)t) && (CanReach(searcherThing, t, canBash) || CanShootAtFromCurrentPosition((IAttackTarget)t, searcher, verb)));
                    }
                    else
                    {
                        Log.Message("Using validator2 where (byte)TargetScanFlags flags are 0");
                        validator2 = ((Thing t) => innerValidator((IAttackTarget)t));
                    }
                    Log.Message("--- MARKER ---");
                    result = (IAttackTarget)GenClosest.ClosestThing_Global(searcherThing.Position, tmpTargets, maxDist, validator2, null);
                    if (result == null)
                    {
                        Log.Message("Taregt selection complete. Clearing tmpTargets. There is no valid target");
                    }
                    else
                    {
                        Log.Message("Taregt selection complete. Clearing tmpTargets. Final target is: " + result.Thing.ToString());
                    }
                }
                tmpTargets.Clear();
                return result;
            }
            if (searcherPawn != null && searcherPawn.mindState.duty != null && searcherPawn.mindState.duty.radius > 0f && !searcherPawn.InMentalState)
            {
                Predicate<IAttackTarget> oldValidator = innerValidator;
                innerValidator = ((IAttackTarget t) => oldValidator(t) && t.Thing.Position.InHorDistOf(searcherPawn.mindState.duty.focus.Cell, searcherPawn.mindState.duty.radius));
            }
            IntVec3 position = searcherThing.Position;
            Map map = searcherThing.Map;
            ThingRequest thingReq = ThingRequest.ForGroup(ThingRequestGroup.AttackTarget);
            PathEndMode peMode = PathEndMode.Touch;
            Pawn searcherPawn2 = searcherPawn;
            Danger maxDanger = Danger.Deadly;
            bool canBash2 = canBash;
            TraverseParms traverseParams = TraverseParms.For(searcherPawn2, maxDanger, TraverseMode.ByPawn, canBash2);
            float maxDist2 = maxDist;
            Predicate<Thing> validator3 = (Thing x) => innerValidator((IAttackTarget)x);
            int searchRegionsMax = (maxDist <= 800f) ? 40 : -1;
            IAttackTarget attackTarget2 = (IAttackTarget)GenClosest.ClosestThingReachable(position, map, thingReq, peMode, traverseParams, maxDist2, validator3, null, 0, searchRegionsMax, false, RegionType.Set_Passable, false);
            if (attackTarget2 != null && PawnUtility.ShouldCollideWithPawns(searcherPawn))
            {
                IAttackTarget attackTarget3 = FindBestReachableMeleeTarget(innerValidator, searcherPawn, maxDist, canBash);
                if (attackTarget3 != null)
                {
                    float lengthHorizontal = (searcherPawn.Position - attackTarget2.Thing.Position).LengthHorizontal;
                    float lengthHorizontal2 = (searcherPawn.Position - attackTarget3.Thing.Position).LengthHorizontal;
                    if (Mathf.Abs(lengthHorizontal - lengthHorizontal2) < 50f)
                    {
                        attackTarget2 = attackTarget3;
                    }
                }
            }
            return attackTarget2;
        }

        private static bool CanReach(Thing searcher, Thing target, bool canBash)
        {
            Pawn pawn = searcher as Pawn;
            if (pawn != null)
            {
                if (!pawn.CanReach(target, PathEndMode.Touch, Danger.Some, canBash, TraverseMode.ByPawn))
                {
                    return false;
                }
            }
            else
            {
                TraverseMode mode = (!canBash) ? TraverseMode.NoPassClosedDoors : TraverseMode.PassDoors;
                if (!searcher.Map.reachability.CanReach(searcher.Position, target, PathEndMode.Touch, TraverseParms.For(mode, Danger.Deadly, false)))
                {
                    return false;
                }
            }
            return true;
        }

        private static IAttackTarget FindBestReachableMeleeTarget(Predicate<IAttackTarget> validator, Pawn searcherPawn, float maxTargDist, bool canBash)
        {
            maxTargDist = Mathf.Min(maxTargDist, 30f);
            IAttackTarget reachableTarget = null;
            Func<IntVec3, IAttackTarget> bestTargetOnCell = delegate (IntVec3 x)
            {
                List<Thing> thingList = x.GetThingList(searcherPawn.Map);
                for (int i = 0; i < thingList.Count; i++)
                {
                    Thing thing = thingList[i];
                    IAttackTarget attackTarget = thing as IAttackTarget;
                    if (attackTarget != null)
                    {
                        if (validator(attackTarget))
                        {
                            if (ReachabilityImmediate.CanReachImmediate(x, thing, searcherPawn.Map, PathEndMode.Touch, searcherPawn))
                            {
                                if (searcherPawn.CanReachImmediate(thing, PathEndMode.Touch) || searcherPawn.Map.attackTargetReservationManager.CanReserve(searcherPawn, attackTarget))
                                {
                                    return attackTarget;
                                }
                            }
                        }
                    }
                }
                return null;
            };
            searcherPawn.Map.floodFiller.FloodFill(searcherPawn.Position, delegate (IntVec3 x)
            {
                if (!x.Walkable(searcherPawn.Map))
                {
                    return false;
                }
                if ((float)x.DistanceToSquared(searcherPawn.Position) > maxTargDist * maxTargDist)
                {
                    return false;
                }
                if (!canBash)
                {
                    Building_Door building_Door = x.GetEdifice(searcherPawn.Map) as Building_Door;
                    if (building_Door != null && !building_Door.CanPhysicallyPass(searcherPawn))
                    {
                        return false;
                    }
                }
                return !PawnUtility.AnyPawnBlockingPathAt(x, searcherPawn, true, false, false);
            }, delegate (IntVec3 x)
            {
                for (int i = 0; i < 8; i++)
                {
                    IntVec3 intVec = x + GenAdj.AdjacentCells[i];
                    if (intVec.InBounds(searcherPawn.Map))
                    {
                        IAttackTarget attackTarget = bestTargetOnCell(intVec);
                        if (attackTarget != null)
                        {
                            reachableTarget = attackTarget;
                            break;
                        }
                    }
                }
                return reachableTarget != null;
            }, int.MaxValue, false, null);
            return reachableTarget;
        }

        private static bool HasRangedAttack(IAttackTargetSearcher t)
        {
            Verb currentEffectiveVerb = t.CurrentEffectiveVerb;
            return currentEffectiveVerb != null && !currentEffectiveVerb.verbProps.IsMeleeAttack;
        }

        private static bool CanShootAtFromCurrentPosition(IAttackTarget target, IAttackTargetSearcher searcher, Verb verb)
        {
            return verb != null && verb.CanHitTargetFrom(searcher.Thing.Position, target.Thing);
        }

        private static IAttackTarget GetRandomShootingTargetByScore(List<IAttackTarget> targets, IAttackTargetSearcher searcher, Verb verb)
        {
            Pair<IAttackTarget, float> pair;
            if (GetAvailableShootingTargetsByScore(targets, searcher, verb).TryRandomElementByWeight((Pair<IAttackTarget, float> x) => x.Second, out pair))
            {
                return pair.First;
            }
            return null;
        }

        private static List<Pair<IAttackTarget, float>> GetAvailableShootingTargetsByScore(List<IAttackTarget> rawTargets, IAttackTargetSearcher searcher, Verb verb)
        {
            availableShootingTargets.Clear();
            if (rawTargets.Count == 0)
            {
                return availableShootingTargets;
            }
            tmpTargetScores.Clear();
            tmpCanShootAtTarget.Clear();
            float num = 0f;
            IAttackTarget attackTarget = null;
            for (int i = 0; i < rawTargets.Count; i++)
            {
                tmpTargetScores.Add(float.MinValue);
                tmpCanShootAtTarget.Add(false);
                if (rawTargets[i] != searcher)
                {
                    bool flag = CanShootAtFromCurrentPosition(rawTargets[i], searcher, verb);
                    tmpCanShootAtTarget[i] = flag;
                    if (flag)
                    {
                        float shootingTargetScore = GetShootingTargetScore(rawTargets[i], searcher, verb);
                        tmpTargetScores[i] = shootingTargetScore;
                        if (attackTarget == null || shootingTargetScore > num)
                        {
                            attackTarget = rawTargets[i];
                            num = shootingTargetScore;
                        }
                    }
                }
            }
            if (num < 1f)
            {
                if (attackTarget != null)
                {
                    availableShootingTargets.Add(new Pair<IAttackTarget, float>(attackTarget, 1f));
                }
            }
            else
            {
                float num2 = num - 30f;
                for (int j = 0; j < rawTargets.Count; j++)
                {
                    if (rawTargets[j] != searcher)
                    {
                        if (tmpCanShootAtTarget[j])
                        {
                            float num3 = tmpTargetScores[j];
                            if (num3 >= num2)
                            {
                                float second = Mathf.InverseLerp(num - 30f, num, num3);
                                availableShootingTargets.Add(new Pair<IAttackTarget, float>(rawTargets[j], second));
                            }
                        }
                    }
                }
            }
            return availableShootingTargets;
        }

        private static float GetShootingTargetScore(IAttackTarget target, IAttackTargetSearcher searcher, Verb verb)
        {
            float num = 60f;
            num -= Mathf.Min((target.Thing.Position - searcher.Thing.Position).LengthHorizontal, 40f);
            if (target.TargetCurrentlyAimingAt == searcher.Thing)
            {
                num += 10f;
            }
            if (searcher.LastAttackedTarget == target.Thing && Find.TickManager.TicksGame - searcher.LastAttackTargetTick <= 300)
            {
                num += 40f;
            }
            num -= CoverUtility.CalculateOverallBlockChance(target.Thing.Position, searcher.Thing.Position, searcher.Thing.Map) * 10f;
            Pawn pawn = target as Pawn;
            if (pawn != null && pawn.RaceProps.Animal && pawn.Faction != null && !pawn.IsFighting())
            {
                num -= 50f;
            }
            num += FriendlyFireBlastRadiusTargetScoreOffset(target, searcher, verb);
            return num + FriendlyFireConeTargetScoreOffset(target, searcher, verb);
        }
        
        private static float FriendlyFireBlastRadiusTargetScoreOffset(IAttackTarget target, IAttackTargetSearcher searcher, Verb verb)
        {
            if (verb.verbProps.ai_AvoidFriendlyFireRadius <= 0f)
            {
                return 0f;
            }
            Map map = target.Thing.Map;
            IntVec3 position = target.Thing.Position;
            int num = GenRadial.NumCellsInRadius(verb.verbProps.ai_AvoidFriendlyFireRadius);
            float num2 = 0f;
            for (int i = 0; i < num; i++)
            {
                IntVec3 intVec = position + GenRadial.RadialPattern[i];
                if (intVec.InBounds(map))
                {
                    bool flag = true;
                    List<Thing> thingList = intVec.GetThingList(map);
                    for (int j = 0; j < thingList.Count; j++)
                    {
                        if (thingList[j] is IAttackTarget && thingList[j] != target)
                        {
                            if (flag)
                            {
                                if (!GenSight.LineOfSight(position, intVec, map, true, null, 0, 0))
                                {
                                    break;
                                }
                                flag = false;
                            }
                            float num3;
                            if (thingList[j] == searcher)
                            {
                                num3 = 40f;
                            }
                            else if (thingList[j] is Pawn)
                            {
                                num3 = ((!thingList[j].def.race.Animal) ? 18f : 7f);
                            }
                            else
                            {
                                num3 = 10f;
                            }
                            if (searcher.Thing.HostileTo(thingList[j]))
                            {
                                num2 += num3 * 0.6f;
                            }
                            else
                            {
                                num2 -= num3;
                            }
                        }
                    }
                }
            }
            return num2;
        }
        
        private static float FriendlyFireConeTargetScoreOffset(IAttackTarget target, IAttackTargetSearcher searcher, Verb verb)
        {
            Pawn pawn = searcher.Thing as Pawn;
            if (pawn == null)
            {
                return 0f;
            }
            if (pawn.RaceProps.intelligence < Intelligence.ToolUser)
            {
                return 0f;
            }
            if (pawn.RaceProps.IsMechanoid)
            {
                return 0f;
            }
            Verb_Shoot verb_Shoot = verb as Verb_Shoot;
            if (verb_Shoot == null)
            {
                return 0f;
            }
            ThingDef defaultProjectile = verb_Shoot.verbProps.defaultProjectile;
            if (defaultProjectile == null)
            {
                return 0f;
            }
            if (defaultProjectile.projectile.flyOverhead)
            {
                return 0f;
            }
            Map map = pawn.Map;
            ShotReport report = ShotReport.HitReportFor(pawn, verb, (Thing)target);
            float a = VerbUtility.CalculateAdjustedForcedMiss(verb.verbProps.forcedMissRadius, report.ShootLine.Dest - report.ShootLine.Source);
            float radius = Mathf.Max(a, 1.5f);
            IntVec3 dest2 = report.ShootLine.Dest;
            IEnumerable<IntVec3> source = from dest in GenRadial.RadialCellsAround(dest2, radius, true)
                                            where dest.InBounds(map)
                                            select dest;
            IEnumerable<ShootLine> source2 = from dest in source
                                                select new ShootLine(report.ShootLine.Source, dest);
            IEnumerable<IntVec3> source3 = source2.SelectMany((ShootLine line) => line.Points().Concat(line.Dest).TakeWhile((IntVec3 pos) => pos.CanBeSeenOverFast(map)));
            IEnumerable<IntVec3> enumerable = source3.Distinct<IntVec3>();
            float num = 0f;
            foreach (IntVec3 c in enumerable)
            {
                float num2 = VerbUtility.InterceptChanceFactorFromDistance(report.ShootLine.Source.ToVector3Shifted(), c);
                if (num2 > 0f)
                {
                    List<Thing> thingList = c.GetThingList(map);
                    for (int i = 0; i < thingList.Count; i++)
                    {
                        Thing thing = thingList[i];
                        if (thing is IAttackTarget && thing != target)
                        {
                            float num3;
                            if (thing == searcher)
                            {
                                num3 = 40f;
                            }
                            else if (thing is Pawn)
                            {
                                num3 = ((!thing.def.race.Animal) ? 18f : 7f);
                            }
                            else
                            {
                                num3 = 10f;
                            }
                            num3 *= num2;
                            if (searcher.Thing.HostileTo(thing))
                            {
                                num3 *= 0.6f;
                            }
                            else
                            {
                                num3 *= -1f;
                            }
                            num += num3;
                        }
                    }
                }
            }
            return num;
        }
        
        public static IAttackTarget BestShootTargetFromCurrentPosition(IAttackTargetSearcher searcher, TargetScanFlags flags, Predicate<Thing> validator = null, float minDistance = 0f, float maxDistance = 9999f)
        {
            Verb currentEffectiveVerb = searcher.CurrentEffectiveVerb;
            if (currentEffectiveVerb == null)
            {
                Log.Error("BestShootTargetFromCurrentPosition with " + searcher.ToStringSafe<IAttackTargetSearcher>() + " who has no attack verb.", false);
                return null;
            }
            return BestAttackTarget(searcher, flags, validator, Mathf.Max(minDistance, currentEffectiveVerb.verbProps.minRange), Mathf.Min(maxDistance, currentEffectiveVerb.verbProps.range), default(IntVec3), float.MaxValue, false, false);
        }
        
        public static bool CanSee(this Thing seer, Thing target, Func<IntVec3, bool> validator = null)
        {
            ShootLeanUtility.CalcShootableCellsOf(tempDestList, target);
            for (int i = 0; i < tempDestList.Count; i++)
            {
                if (GenSight.LineOfSight(seer.Position, tempDestList[i], seer.Map, true, validator, 0, 0))
                {
                    return true;
                }
            }
            ShootLeanUtility.LeanShootingSourcesFromTo(seer.Position, target.Position, seer.Map, tempSourceList);
            for (int j = 0; j < tempSourceList.Count; j++)
            {
                for (int k = 0; k < tempDestList.Count; k++)
                {
                    if (GenSight.LineOfSight(tempSourceList[j], tempDestList[k], seer.Map, true, validator, 0, 0))
                    {
                        return true;
                    }
                }
            }
            return false;
        }
        
        public static void DebugDrawAttackTargetScores_Update()
        {
            IAttackTargetSearcher attackTargetSearcher = Find.Selector.SingleSelectedThing as IAttackTargetSearcher;
            if (attackTargetSearcher == null)
            {
                return;
            }
            if (attackTargetSearcher.Thing.Map != Find.CurrentMap)
            {
                return;
            }
            Verb currentEffectiveVerb = attackTargetSearcher.CurrentEffectiveVerb;
            if (currentEffectiveVerb == null)
            {
                return;
            }
            tmpTargets.Clear();
            List<Thing> list = attackTargetSearcher.Thing.Map.listerThings.ThingsInGroup(ThingRequestGroup.AttackTarget);
            for (int i = 0; i < list.Count; i++)
            {
                tmpTargets.Add((IAttackTarget)list[i]);
            }
            List<Pair<IAttackTarget, float>> availableShootingTargetsByScore = GetAvailableShootingTargetsByScore(tmpTargets, attackTargetSearcher, currentEffectiveVerb);
            for (int j = 0; j < availableShootingTargetsByScore.Count; j++)
            {
                GenDraw.DrawLineBetween(attackTargetSearcher.Thing.DrawPos, availableShootingTargetsByScore[j].First.Thing.DrawPos);
            }
        }
        
        public static void DebugDrawAttackTargetScores_OnGUI()
        {
            IAttackTargetSearcher attackTargetSearcher = Find.Selector.SingleSelectedThing as IAttackTargetSearcher;
            if (attackTargetSearcher == null)
            {
                return;
            }
            if (attackTargetSearcher.Thing.Map != Find.CurrentMap)
            {
                return;
            }
            Verb currentEffectiveVerb = attackTargetSearcher.CurrentEffectiveVerb;
            if (currentEffectiveVerb == null)
            {
                return;
            }
            List<Thing> list = attackTargetSearcher.Thing.Map.listerThings.ThingsInGroup(ThingRequestGroup.AttackTarget);
            Text.Anchor = TextAnchor.MiddleCenter;
            Text.Font = GameFont.Tiny;
            for (int i = 0; i < list.Count; i++)
            {
                Thing thing = list[i];
                if (thing != attackTargetSearcher)
                {
                    string text;
                    Color red;
                    if (!CanShootAtFromCurrentPosition((IAttackTarget)thing, attackTargetSearcher, currentEffectiveVerb))
                    {
                        text = "out of range";
                        red = Color.red;
                    }
                    else
                    {
                        text = GetShootingTargetScore((IAttackTarget)thing, attackTargetSearcher, currentEffectiveVerb).ToString("F0");
                        red = new Color(0.25f, 1f, 0.25f);
                    }
                    Vector2 screenPos = thing.DrawPos.MapToUIPosition();
                    GenMapUI.DrawThingLabel(screenPos, text, red);
                }
            }
            Text.Anchor = TextAnchor.UpperLeft;
            Text.Font = GameFont.Small;
        }
        
        private const float FriendlyFireScoreOffsetPerHumanlikeOrMechanoid = 18f;
        
        private const float FriendlyFireScoreOffsetPerAnimal = 7f;
        
        private const float FriendlyFireScoreOffsetPerNonPawn = 10f;
        
        private const float FriendlyFireScoreOffsetSelf = 40f;
        
        private static List<IAttackTarget> tmpTargets = new List<IAttackTarget>();
        
        private static List<Pair<IAttackTarget, float>> availableShootingTargets = new List<Pair<IAttackTarget, float>>();
        
        private static List<float> tmpTargetScores = new List<float>();
        
        private static List<bool> tmpCanShootAtTarget = new List<bool>();
        
        private static List<IntVec3> tempDestList = new List<IntVec3>();
        
        private static List<IntVec3> tempSourceList = new List<IntVec3>();
    }
}