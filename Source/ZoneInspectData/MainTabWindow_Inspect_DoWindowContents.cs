using HarmonyLib;
using RimWorld;
using Verse;
using UnityEngine;
using System.Linq;
using System.Collections.Generic;

namespace ExtendedInspectData
{
    [HarmonyPatch(typeof(MainTabWindow_Inspect), "DoWindowContents")]
    internal class MainTabWindow_Inspect_DoWindowContents
    {

        private static ZoneStockpileInspectPaneFiller zoneStockpileInspectPanelFiller = new ZoneStockpileInspectPaneFiller();
        private static ZoneGrowingInspectPaneFiller zoneGrowingInspectPanelFiller = new ZoneGrowingInspectPaneFiller();
        private static PlantGrowingInspectPaneFiller plantGrowingInspectPanelFiller = new PlantGrowingInspectPaneFiller();
        private static BuildingStorageInspectPaneFiller storageInspectPanelFiller = new BuildingStorageInspectPaneFiller();

        //private static Vector2 requestedSize;


        static void Postfix(MainTabWindow_Inspect __instance, Rect inRect)
        {
            SetHeightOffsets();

            if (Find.Selector.NumSelected == 1)
            {
                Zone_Stockpile selStockpileZone = ((ISelectable)Find.Selector.SelectedZone) as Zone_Stockpile;
                Zone_Growing selGrowingZone = ((ISelectable)Find.Selector.SelectedZone) as Zone_Growing;
                Building_Storage storage = ((ISelectable)Find.Selector.SingleSelectedObject) as Building_Storage;

                plantGrowingInspectPanelFiller.ResetData();

                //single selection
                if (selStockpileZone != null)
                {
                    zoneStockpileInspectPanelFiller.DoPaneContentsFor(selStockpileZone, inRect);
                    storageInspectPanelFiller.ResetData();
                    zoneGrowingInspectPanelFiller.ResetData();
                }
                else if (selGrowingZone != null)
                {
                    zoneGrowingInspectPanelFiller.DoPaneContentsFor(new List<Zone_Growing>() { selGrowingZone }, inRect);
                    storageInspectPanelFiller.ResetData();
                    zoneStockpileInspectPanelFiller.ResetData();
                }
                else if (storage != null)
                {
                    storageInspectPanelFiller.DoPaneContentsFor(new List<Building_Storage>() { storage }, inRect);
                    zoneStockpileInspectPanelFiller.ResetData();
                    zoneGrowingInspectPanelFiller.ResetData();
                }
                else
                {
                    zoneStockpileInspectPanelFiller.ResetData();
                    zoneGrowingInspectPanelFiller.ResetData();
                    storageInspectPanelFiller.ResetData();
                }
            }
            else if (Find.Selector.NumSelected > 1)
            {
                //multiple things selected, check if all are growing zones
                List<object> things = Find.Selector.SelectedObjects.FindAll(thing => (thing as Zone_Growing) != null);
                if (things.Count == Find.Selector.NumSelected)
                {
                    zoneGrowingInspectPanelFiller.DoPaneContentsFor(things.Cast<Zone_Growing>().ToList(), inRect);
                    storageInspectPanelFiller.ResetData();
                    zoneStockpileInspectPanelFiller.ResetData();
                    plantGrowingInspectPanelFiller.ResetData();
                }
                else
                {
                    things = Find.Selector.SelectedObjects.FindAll(thing => (thing as Building_Storage) != null);
                    if (things.Count == Find.Selector.NumSelected)
                    {
                        storageInspectPanelFiller.DoPaneContentsFor(things.Cast<Building_Storage>().ToList(), inRect);
                        zoneStockpileInspectPanelFiller.ResetData();
                        zoneGrowingInspectPanelFiller.ResetData();
                        plantGrowingInspectPanelFiller.ResetData();
                    }
                    else
                    {
                        things = Find.Selector.SelectedObjects.FindAll(thing => (thing as Building_PlantGrower) != null);
                        if (things.Count == Find.Selector.NumSelected)
                        {
                            plantGrowingInspectPanelFiller.DoPaneContentsFor(things.Cast<Building_PlantGrower>().ToList(), inRect);
                            storageInspectPanelFiller.ResetData();
                            zoneStockpileInspectPanelFiller.ResetData();
                            zoneGrowingInspectPanelFiller.ResetData();
                        }
                    }
                }
            }
            else
            {
                zoneStockpileInspectPanelFiller.ResetData();
                zoneGrowingInspectPanelFiller.ResetData();
                storageInspectPanelFiller.ResetData();
                plantGrowingInspectPanelFiller.ResetData();
            }
        }

        //Moved out of RequestedTabSize due to issues with MainTabWindow_Inspect.SelectNextInCell()
        private static void SetHeightOffsets()
        {
            List<object> things = Find.Selector.SelectedObjects;

            int lines = 0;

            // When selecting something and then clicking onto empty space,
            //  things will be empty before dialog MainTabWindow closes
            if (!things.NullOrEmpty())
            {
                string inspectString = (things[0] as ISelectable).GetInspectString();
                lines = inspectString.Length - inspectString.Replace("\n", string.Empty).Length;
            }

            if (things.Count == 1 && things[0] is Zone_Growing)
            {
                PlantGrowingInspectPaneFiller.HeightOffset = lines + 1f;
            }
            else
            {
                PlantGrowingInspectPaneFiller.HeightOffset = 0f;
            }

            if (things.Count == 1 && things[0] is Building_Storage)
            {
                BuildingStorageInspectPaneFiller.HeightOffset = lines + 1f;
            }
            else
            {
                BuildingStorageInspectPaneFiller.HeightOffset = 0f;
            }
        }  
    }
}
