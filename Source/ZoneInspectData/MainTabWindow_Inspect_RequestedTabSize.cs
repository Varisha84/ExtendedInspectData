using HarmonyLib;
using RimWorld;
using Verse;
using UnityEngine;
using System;
using System.Collections.Generic;

namespace ExtendedInspectData
{
    [HarmonyPatch(typeof(MainTabWindow_Inspect), "RequestedTabSize", MethodType.Getter)]
    internal class MainTabWindow_Inspect_RequestedTabSize
    {
        
        static Vector2 Postfix(Vector2 __result)
        {
            var requestedSize = __result;
            List<object> things = Find.Selector.SelectedObjects.FindAll(thing => (thing as Zone_Growing) != null);

            if ((things.Count > 0) && (things.Count == Find.Selector.NumSelected))
            {
                if (things.Count == 1) {
                    requestedSize = new Vector2(requestedSize.x, Math.Max(requestedSize.y, ZoneGrowingInspectPaneFiller.SingleZoneHeightOffset + ZoneGrowingInspectPaneFiller.RequiredSingleSelectionHeight));
                } else {
                    requestedSize = new Vector2(requestedSize.x, Math.Max(requestedSize.y, ZoneGrowingInspectPaneFiller.MultiZoneHeightOffset + ZoneGrowingInspectPaneFiller.RequiredMultiSelectionHeight));
                }
            }
            else
            {
                things = Find.Selector.SelectedObjects.FindAll(thing => (thing as Building_Storage) != null);
                if ((things.Count > 0) && (things.Count == Find.Selector.NumSelected))
                {
                    requestedSize = new Vector2(requestedSize.x, Math.Max(requestedSize.y, BuildingStorageInspectPaneFiller.HeightOffset + BuildingStorageInspectPaneFiller.RequiredHeight));
                }
                else
                {
                    things = Find.Selector.SelectedObjects.FindAll(thing => (thing as Building_PlantGrower) != null);
                    if ((things.Count > 0) && (things.Count == Find.Selector.NumSelected))
                    {
                        requestedSize = new Vector2(requestedSize.x, Math.Max(requestedSize.y, PlantGrowingInspectPaneFiller.HeightOffset + PlantGrowingInspectPaneFiller.RequiredHeight));
                    }
                }
            }

            return requestedSize;
        }
    }
}
