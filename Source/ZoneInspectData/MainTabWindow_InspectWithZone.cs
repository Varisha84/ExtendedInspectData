using RimWorld;
using Verse;
using UnityEngine;
using System.Linq;
using System.Collections.Generic;

namespace ZoneInspectData
{
    [StaticConstructorOnStartup]
    public class MainTabWindow_InspectZone_Stockpile : MainTabWindow_Inspect
    {
        private ZoneStockpileInspectPaneFiller zoneStockpileInspectPanelFiller;
        private ZoneGrowingInspectPaneFiller zoneGrowingInspectPanelFiller;
        private BuildingStorageInspectPaneFiller storageInspectPanelFiller;


        //To change default height of the inspect window in certain cases. Does not change width as
        //RimWorld.InspectGizmoGrid.DrawInspectGizmoGridFor uses static distance to inspect pane that cannot be 
        //changed easily for now (it call: InspectPaneUtility.PaneSize.x + 20f). This is out of scope yet.
        public override Vector2 RequestedTabSize
        {
            get
            {
                Vector2 baseSize = base.RequestedTabSize;

                List<object> things = Find.Selector.SelectedObjects.FindAll(thing => (thing as Zone_Growing) != null);
                if (things.Count == Find.Selector.NumSelected)
                {
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
            storageInspectPanelFiller = new BuildingStorageInspectPaneFiller();
        }

        public override void DoWindowContents(Rect inRect)
        {
            base.DoWindowContents(inRect);
            bool flagReset = false;

            if (Find.Selector.NumSelected == 1)
            {
                Zone_Stockpile selStockpileZone = ((ISelectable)Find.Selector.SelectedZone) as Zone_Stockpile;
                Zone_Growing selGrowingZone = ((ISelectable)Find.Selector.SelectedZone) as Zone_Growing;
                Building_Storage storage = ((ISelectable)Find.Selector.SingleSelectedObject) as Building_Storage;

                //single selection
                if (selStockpileZone != null)
                {
                    zoneStockpileInspectPanelFiller.DoPaneContentsFor(selStockpileZone, inRect);
                }
                else if (selGrowingZone != null)
                {
                    zoneGrowingInspectPanelFiller.DoPaneContentsFor(new List<Zone_Growing>() { selGrowingZone }, inRect);
                }
                else if (storage != null)
                {
                    storageInspectPanelFiller.DoPaneContentsFor(new List<Building_Storage>() { storage }, inRect);
                } else
                {
                    flagReset = true;
                }
            } else if (Find.Selector.NumSelected > 1)
            {
                //multiple things selected, check if all are growing zones
                List<object> things = Find.Selector.SelectedObjects.FindAll(thing => (thing as Zone_Growing) != null);
                if (things.Count == Find.Selector.NumSelected)
                {
                    zoneGrowingInspectPanelFiller.DoPaneContentsFor(things.Cast<Zone_Growing>().ToList(), inRect);
                }
                else
                {
                    things = Find.Selector.SelectedObjects.FindAll(thing => (thing as Building_Storage) != null);
                    if (things.Count == Find.Selector.NumSelected)
                    {
                        storageInspectPanelFiller.DoPaneContentsFor(things.Cast<Building_Storage>().ToList(), inRect);
                    }
                }
            } else
            {
                flagReset = true;
            }

            if (flagReset)
            {
                zoneStockpileInspectPanelFiller.ResetData();
                zoneGrowingInspectPanelFiller.ResetData();
                storageInspectPanelFiller.ResetData();
            }
        }
    }
}
