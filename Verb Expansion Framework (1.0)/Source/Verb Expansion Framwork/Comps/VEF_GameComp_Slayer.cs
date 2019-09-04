using System;
using System.Collections.Generic;
using System.Linq;
using Verse;
using RimWorld;
using UnityEngine;

namespace VerbExpansionFramework
{
    public class VEF_GameComp_Slayer : GameComponent
    {
        public VEF_GameComp_Slayer(Game game)
        {

        }

        public override void GameComponentTick()
        {
            foreach (VEF_KillInfo killInfo in thingsToKill)
            {
                if (killInfo.thing is Pawn pawn)
                {
                    pawn.Kill(new DamageInfo(killInfo.damageDef, 0f));
                }
                else
                {
                    killInfo.thing.Destroy(DestroyMode.KillFinalize);
                }
            }
            thingsToKill.Clear();
        }

        public List<VEF_KillInfo> thingsToKill = new List<VEF_KillInfo>();
    }
}
