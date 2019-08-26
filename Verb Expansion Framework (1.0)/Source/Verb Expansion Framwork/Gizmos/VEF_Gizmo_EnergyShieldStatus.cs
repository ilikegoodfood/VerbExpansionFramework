using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;
using UnityEngine;

namespace VerbExpansionFramework
{
    [StaticConstructorOnStartup]
    class VEF_Gizmo_EnergyShieldStatus : Gizmo
    {
        public VEF_Gizmo_EnergyShieldStatus()
        {
            this.order = -100f;
        }

        // Token: 0x06002730 RID: 10032 RVA: 0x0012A76B File Offset: 0x00128B6B
        public override float GetWidth(float maxWidth)
        {
            return 140f;
        }

        // Token: 0x06002731 RID: 10033 RVA: 0x0012A774 File Offset: 0x00128B74
        public override GizmoResult GizmoOnGUI(Vector2 topLeft, float maxWidth)
        {
            Rect overRect = new Rect(topLeft.x, topLeft.y, this.GetWidth(maxWidth), 75f);
            Find.WindowStack.ImmediateWindow(984688, overRect, WindowLayer.GameUI, delegate
            {
                Rect rect = overRect.AtZero().ContractedBy(6f);
                Rect rect2 = rect;
                rect2.height = overRect.height / 2f;
                Text.Font = GameFont.Tiny;
                Widgets.Label(rect2, "Racial Shield of " + this.shield.Pawn.def.LabelCap);
                Rect rect3 = rect;
                rect3.yMin = overRect.height / 2f;
                float fillPercent = this.shield.Energy / Mathf.Max(1f, this.shield.Props.energyMax);
                Widgets.FillableBar(rect3, fillPercent, VEF_Gizmo_EnergyShieldStatus.FullShieldBarTex, VEF_Gizmo_EnergyShieldStatus.EmptyShieldBarTex, false);
                Text.Font = GameFont.Small;
                Text.Anchor = TextAnchor.MiddleCenter;
                Widgets.Label(rect3, (this.shield.Energy * 100f).ToString("F0") + " / " + (this.shield.Props.energyMax * 100f).ToString("F0"));
                Text.Anchor = TextAnchor.UpperLeft;
            }, true, false, 1f);
            return new GizmoResult(GizmoState.Clear);
        }

        // Token: 0x0400161B RID: 5659
        public VEF_ThingComp_ShieldDefense shield;

        // Token: 0x0400161C RID: 5660
        private static readonly Texture2D FullShieldBarTex = SolidColorMaterials.NewSolidColorTexture(new Color(0.2f, 0.2f, 0.24f));

        // Token: 0x0400161D RID: 5661
        private static readonly Texture2D EmptyShieldBarTex = SolidColorMaterials.NewSolidColorTexture(Color.clear);
    }
}
