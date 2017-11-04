using RimWorld;
using Verse;
using UnityEngine;
using System;
using System.Collections.Generic;

namespace StorageAreaContent
{
    [StaticConstructorOnStartup]
    public class MainTabWindow_InspectZone_Stockpile : MainTabWindow_Inspect
    {
        private ZoneStockpileInspectPaneFiller zoneInspectPanelFiller;

        public MainTabWindow_InspectZone_Stockpile() : base()
        {
            Log.Message("New instance");
            zoneInspectPanelFiller = new ZoneStockpileInspectPaneFiller();
            
        }

        public override void DoWindowContents(Rect inRect)
        {
            base.DoWindowContents(inRect);

            Zone_Stockpile selZone = ((ISelectable)Find.Selector.FirstSelectedObject) as Zone_Stockpile;
            if (selZone != null)
            {
                Rect rect = inRect.AtZero();
                rect.yMin += 26f;
                DoZoneInspectContents(selZone, rect);
            } else
            {
                zoneInspectPanelFiller.ResetData();
            }
        }

        public void DoZoneInspectContents(Zone_Stockpile zone, Rect rect)
        {
            zoneInspectPanelFiller.DoPaneContentsFor(zone, rect);
        }
    }
}
