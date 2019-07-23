using System;
using RimWorld;
using Verse;
using Verse.AI;
using UnityEngine;

namespace VerbExpansionFramework
{
    class VEF_JobGiver_ManhunterRanged : JobGiver_Manhunter
    {

        protected override Job TryGiveJob(Pawn pawn)
        {
            UpdateEnemyTarget(pawn);
            Thing enemyTarget = pawn.mindState.enemyTarget;

            if (enemyTarget == null)
            {
                // Log.Message("There is no valid target. Returning null");
                return null;
            }

            bool allowManualCastWeapons = !pawn.IsColonist;
            Verb verb = pawn.TryGetAttackVerb(enemyTarget, allowManualCastWeapons);
            // Log.Message("allowManualCastWeapons: " + allowManualCastWeapons.ToString());
            // Log.Message("attackVerb: " + verb.ToString());

            if (verb == null)
            {
                Log.Error("pawn " + pawn.Label + " does not have attack verb");
            }
            else if (verb.IsMeleeAttack)
            {
                // Log.Message("verb is melee attack. Passing to RimWorld.JobGiver_Manhunter ...");
            }
            else
            {
                // Log.Message("verb is ranged attack");

                bool isInCover = CoverUtility.CalculateOverallBlockChance(pawn, enemyTarget.Position, pawn.Map) > 0.01f;
                bool positionIsStandable = pawn.Position.Standable(pawn.Map);
                bool canHitEnemy = verb.CanHitTarget(enemyTarget);
                bool enemyNear = (pawn.Position - enemyTarget.Position).LengthHorizontalSquared < 25;
                if ((isInCover && positionIsStandable && canHitEnemy) || (enemyNear && canHitEnemy))
                {
                    // Log.Message("Can hit enemy from current position.");
                    return new Job(JobDefOf.Wait_Combat, ExpiryInterval_ShooterSucceeded.RandomInRange, true);
                }
                IntVec3 intVec;
                if (!this.TryFindShootingPosition(pawn, out intVec))
                {
                    return null;
                }
                if (intVec == pawn.Position)
                {
                    return new Job(JobDefOf.Wait_Combat, VEF_JobGiver_ManhunterRanged.ExpiryInterval_ShooterSucceeded.RandomInRange, true);
                }
                return new Job(JobDefOf.Goto, intVec)
                {
                    expiryInterval = VEF_JobGiver_ManhunterRanged.ExpiryInterval_ShooterSucceeded.RandomInRange,
                    checkOverrideOnExpire = true
                };
            }

            return null;
        }

        private void UpdateEnemyTarget(Pawn pawn)
        {
            Thing enemyTarget = pawn.mindState.enemyTarget;
            if (enemyTarget != null && (enemyTarget.Destroyed || Find.TickManager.TicksGame - pawn.mindState.lastEngageTargetTick > 400 || !pawn.CanReach(enemyTarget, PathEndMode.Touch, Danger.Deadly, false, TraverseMode.ByPawn) || (float)(pawn.Position - enemyTarget.Position).LengthHorizontalSquared > this.targetKeepRadius * this.targetKeepRadius || ((IAttackTarget)enemyTarget).ThreatDisabled(pawn)))
            {
                enemyTarget = null;
            }
            if (pawn.TryGetAttackVerb(null, !pawn.IsColonist) == null)
            {
                // Log.Message("FindEnbemyTarget cannot find valid attack verb. Assigning null");
                pawn.mindState.enemyTarget = null;
                return;
            }
            // Log.Message("Viable attack verb found");

            enemyTarget = (Thing)AttackTargetFinder.BestAttackTarget(pawn, TargetScanFlags.NeedThreat, (Thing x) => x is Pawn && x.def.race.intelligence >= Intelligence.ToolUser, 0f, 9999f, default(IntVec3), float.MaxValue, true, true);
            if (enemyTarget != null)
            {
                // Log.Message("UpdateEnemyTarget has found valid Pawn enemyTarget. Target is " + enemyTarget.Label);            
            }
            else
            {
                // Log.Message("UpdateEnemyTarget has not found valid Pawn enemyTarget. Searching for valid Building enemyTarget");
                enemyTarget = (Thing)AttackTargetFinder.BestAttackTarget(pawn, TargetScanFlags.NeedLOSToPawns | TargetScanFlags.NeedLOSToNonPawns | TargetScanFlags.NeedReachable | TargetScanFlags.NeedThreat, (Thing t) => t is Building, 0f, 70f, default(IntVec3), float.MaxValue, false, true);
            }

            if (enemyTarget == null)
            {
                // Log.Message("UpdateEnemyTarget found no target. Assigning null");
                pawn.mindState.enemyTarget = null;
            }
            else
            {
                // Log.Message("UpdateEnemyTarget found " + enemyTarget.Label + " to be a valid target");
            }

            if (enemyTarget is Pawn && enemyTarget.Faction == Faction.OfPlayer && pawn.Position.InHorDistOf(enemyTarget.Position, 40f))
            {
                Find.TickManager.slower.SignalForceNormalSpeed();
            }
            pawn.mindState.enemyTarget = enemyTarget;
            return;
        }

        protected bool TryFindShootingPosition(Pawn pawn, out IntVec3 dest)
        {
            Thing enemyTarget = pawn.mindState.enemyTarget;
            bool allowManualCastWeapons = !pawn.IsColonist;
            Verb verb = pawn.TryGetAttackVerb(enemyTarget, allowManualCastWeapons);
            if (verb == null)
            {
                dest = IntVec3.Invalid;
                return false;
            }
            return CastPositionFinder.TryFindCastPosition(new CastPositionRequest
            {
                caster = pawn,
                target = enemyTarget,
                verb = verb,
                maxRangeFromTarget = verb.verbProps.range,
                wantCoverFromTarget = (verb.verbProps.range > 5f)
            }, out dest);
        }

        private static readonly IntRange ExpiryInterval_ShooterSucceeded = new IntRange(450, 550);

        private float targetKeepRadius = 65f;
    }
}
