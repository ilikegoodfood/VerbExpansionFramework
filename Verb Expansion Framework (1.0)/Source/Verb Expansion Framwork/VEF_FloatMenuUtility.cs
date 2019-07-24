using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;
using Verse.AI;

namespace VerbExpansionFramework
{
    class VEF_FloatMenuUtility
    {
        public static Action GetRangedAttackAction(Pawn pawn, LocalTargetInfo target, out string failStr)
        {
            failStr = string.Empty;
            Verb curRangedVerb = pawn.GetComp<VEF_Comp_Pawn_RangedVerbs>().CurRangedVerb;
            if (pawn.GetComp<VEF_Comp_Pawn_RangedVerbs>().CurRangedVerb == null)
            {
                return null;
            }
            if (!pawn.Drafted)
            {
                failStr = "IsNotDraftedLower".Translate(pawn.LabelShort, pawn);
            }
            else if (!pawn.IsColonistPlayerControlled)
            {
                failStr = "CannotOrderNonControlledLower".Translate();
            }
            else if (target.IsValid && !curRangedVerb.CanHitTarget(target))
            {
                if (!pawn.Position.InHorDistOf(target.Cell, curRangedVerb.verbProps.range))
                {
                    failStr = "OutOfRange".Translate();
                }
                float num = curRangedVerb.verbProps.EffectiveMinRange(target, pawn);
                if ((float)pawn.Position.DistanceToSquared(target.Cell) < num * num)
                {
                    failStr = "TooClose".Translate();
                }
                else
                {
                    failStr = "CannotHitTarget".Translate();
                }
            }
            else if (pawn.story.WorkTagIsDisabled(WorkTags.Violent))
            {
                failStr = "IsIncapableOfViolenceLower".Translate(pawn.LabelShort, pawn);
            }
            else
            {
                if (pawn != target.Thing)
                {
                    return delegate ()
                    {
                        Job job = new Job(JobDefOf.AttackStatic, target);
                        pawn.jobs.TryTakeOrderedJob(job, JobTag.Misc);
                    };
                }
                failStr = "CannotAttackSelf".Translate();
            }
            return null;
        }

        public static Action GetAttackAction(Pawn pawn, LocalTargetInfo target, out string failStr)
        {
            if (pawn.GetComp<VEF_Comp_Pawn_RangedVerbs>().CurRangedVerb != null)
            {
                return VEF_FloatMenuUtility.GetRangedAttackAction(pawn, target, out failStr);
            }
            return FloatMenuUtility.GetMeleeAttackAction(pawn, target, out failStr);
        }
    }
}
