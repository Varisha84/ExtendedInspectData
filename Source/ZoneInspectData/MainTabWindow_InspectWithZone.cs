using RimWorld;
using Verse;
using UnityEngine;
using System.Linq;
using System.Collections.Generic;
using System;

namespace ZoneInspectData
{
    [StaticConstructorOnStartup]
    public class MainTabWindow_InspectZone_Stockpile : MainTabWindow_Inspect
    {
        private ZoneStockpileInspectPaneFiller zoneStockpileInspectPanelFiller;
        private ZoneGrowingInspectPaneFiller zoneGrowingInspectPanelFiller;
        private BuildingStorageInspectPaneFiller storageInspectPanelFiller;

        private Vector2 requestedSize;

        //To change default height of the inspect window in certain cases. Does not change width as
        //RimWorld.InspectGizmoGrid.DrawInspectGizmoGridFor uses static distance to inspect pane that cannot be 
        //changed easily for now (it call: InspectPaneUtility.PaneSize.x + 20f). This is out of scope yet.
        public override Vector2 RequestedTabSize
        {
            get
            {
                requestedSize = base.RequestedTabSize;

                List<object> things = Find.Selector.SelectedObjects.FindAll(thing => (thing as Zone_Growing) != null);
                string inspectString;

                if ((things.Count > 0) && (things.Count == Find.Selector.NumSelected))
                {
                    inspectString = ((Zone_Growing)things[0]).GetInspectString();
                    int lines = inspectString.Length - inspectString.Replace("\n", string.Empty).Length;

                    if (things.Count == 1)
                    {
                        zoneGrowingInspectPanelFiller.HeightOffset = lines + 1f;
                    }
                    else
                    {
                        zoneGrowingInspectPanelFiller.HeightOffset = 0f;
                    }

                    requestedSize= new Vector2(requestedSize.x, Math.Max(requestedSize.y, zoneGrowingInspectPanelFiller.HeightOffset + zoneGrowingInspectPanelFiller.RequiredHeight));
                }
                else
                {
                    things = Find.Selector.SelectedObjects.FindAll(thing => (thing as Building_Storage) != null);
                    if ((things.Count > 0) && (things.Count == Find.Selector.NumSelected))
                    {
                        inspectString = ((Building_Storage)things[0]).GetInspectString();
                        int lines = inspectString.Length - inspectString.Replace("\n", string.Empty).Length;

                        if (things.Count == 1)
                        {
                            storageInspectPanelFiller.HeightOffset = lines + 1f;
                        }
                        else
                        {                          
                            storageInspectPanelFiller.HeightOffset = 0f;                          
                        }

                        requestedSize = new Vector2(requestedSize.x, Math.Max(requestedSize.y, storageInspectPanelFiller.HeightOffset + storageInspectPanelFiller.RequiredHeight));
                    }
                }

                return requestedSize;
            }
        }

        public float PaneTopYNew
        {
            get
            {
                Log.Message("requested Y: " + requestedSize.y + " .. Tab Y: " + ((float)UI.screenHeight - requestedSize.y - 35f));
                return (float)UI.screenHeight - requestedSize.y - 35f;
            }
        }

        public MainTabWindow_InspectZone_Stockpile() : base()
        {
            zoneStockpileInspectPanelFiller = new ZoneStockpileInspectPaneFiller();
            zoneGrowingInspectPanelFiller = new ZoneGrowingInspectPaneFiller();
            storageInspectPanelFiller = new BuildingStorageInspectPaneFiller();
        }

        public override void ExtraOnGUI()
        {
            Log.Message("ExtraOnGui");
            base.ExtraOnGUI();
            MyInspectPaneUtility.ExtraOnGUI(this);
            if (this.AnythingSelected && Find.DesignatorManager.SelectedDesignator != null)
            {
                Find.DesignatorManager.SelectedDesignator.DoExtraGuiControls(0f, this.PaneTopYNew);
            }
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
