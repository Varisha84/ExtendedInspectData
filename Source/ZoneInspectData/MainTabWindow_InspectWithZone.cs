using RimWorld;
using Verse;
using UnityEngine;
using System;
using System.Collections.Generic;

namespace ZoneInspectData
{
    [StaticConstructorOnStartup]
    public class MainTabWindow_InspectZone_Stockpile : MainTabWindow_Inspect
    {
        private ZoneStockpileInspectPaneFiller zoneStockpileInspectPanelFiller;
        private ZoneGrowingInspectPaneFiller zoneGrowingInspectPanelFiller;


        //To change default height of the inspect window in certain cases. Does not change width as
        //RimWorld.InspectGizmoGrid.DrawInspectGizmoGridFor uses static distance to inspect pane that cannot be 
        //changed easily for now (it call: InspectPaneUtility.PaneSize.x + 20f). This is out of scope yet.
        public override Vector2 RequestedTabSize
        {
            get
            {
                Vector2 baseSize = base.RequestedTabSize;

                Zone_Growing selGrowingZone = ((ISelectable)Find.Selector.FirstSelectedObject) as Zone_Growing;
                if ((Find.Selector.NumSelected == 1) && (selGrowingZone != null)) {
                    //get default size
                    return new Vector2(baseSize.x, baseSize.y + ZoneGrowingInspectPaneFiller.GROWINGZONE_SINGLE_SELECT_ADDITIONAL_HEIGHT);
                }

                return baseSize;
            }
        }

        public MainTabWindow_InspectZone_Stockpile() : base()
        {
            zoneStockpileInspectPanelFiller = new ZoneStockpileInspectPaneFiller();
            zoneGrowingInspectPanelFiller = new ZoneGrowingInspectPaneFiller();
        }

        public override void DoWindowContents(Rect inRect)
        {
            base.DoWindowContents(inRect);

            if (Find.Selector.NumSelected == 1)
            {
                Zone_Stockpile selStockpileZone = ((ISelectable)Find.Selector.SelectedZone) as Zone_Stockpile;
                Zone_Growing selGrowingZone = ((ISelectable)Find.Selector.SelectedZone) as Zone_Growing;

                //single selection
                if (selStockpileZone != null)
                {
                    Rect rect = inRect.AtZero();
                    rect.yMin += 26f;
                    zoneStockpileInspectPanelFiller.DoPaneContentsFor(selStockpileZone, rect);
                }
                else if (selGrowingZone != null)
                {
                    zoneGrowingInspectPanelFiller.DoPaneContentsFor(selGrowingZone, inRect);
                }
                else
                {
                    zoneStockpileInspectPanelFiller.ResetData();
                    zoneGrowingInspectPanelFiller.ResetData();
                }
            } else
            {
                //multiple selection
            }
        }
    }
}
