using System;
using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;

namespace ZoneInspectData

{
    public class ZoneStockpileInspectPaneFiller
    {
        private static readonly float ICON_WIDTH = 27f;
        private static readonly float DATAROW_HEIGHT = 28f;

        //set of things to consider for listing (basically anything that can be set in zone settings filter)
        private readonly HashSet<ThingDef> thingDefinitions;
        
        //map thingdef to total stackcount 
        private readonly Dictionary<ThingDef, int> summedUpThings;

        //used for sorting
        private readonly List<ThingDef> summedUpThingsLabelList;

        private Zone_Stockpile lastZoneInspected;
        private Vector2 scrollPosition;

        //data used for drawing
        private Rect mainRect;
        private Rect viewRect;
        private float calculatedViewRectHeight;

        

        public ZoneStockpileInspectPaneFiller()
        {
            lastZoneInspected = null;
            scrollPosition = Vector2.zero;
            summedUpThings = new Dictionary<ThingDef, int>();
            summedUpThingsLabelList = new List<ThingDef>();

            IEnumerable<TreeNode_ThingCategory> categories = ThingCategoryNodeDatabase.AllThingCategoryNodes;
            thingDefinitions = new HashSet<ThingDef>();

            foreach (TreeNode_ThingCategory tc in categories)
            {
                foreach (ThingDef td in tc.catDef.DescendantThingDefs)
                {
                    thingDefinitions.Add(td);
                }
            }

            //init drawing data to reduce object handling each draw cycle
            mainRect = new Rect(16f, 46f, 0f, 0f);
            viewRect = new Rect(mainRect.x, mainRect.y, 0f, 0f);
        }

        public void DoPaneContentsFor(Zone_Stockpile zone, Rect rect)
        {
            if (lastZoneInspected != zone)
            {
                SumUpThings(zone);
            }

            try
            {
                GUI.BeginGroup(rect);
                Text.Anchor = TextAnchor.MiddleLeft;
                Text.WordWrap = false;
                Text.Font = GameFont.Small;

                mainRect.width = rect.width - 28f;
                mainRect.height = rect.height - 54f;
                viewRect.width = mainRect.width - 20f;
                viewRect.height = calculatedViewRectHeight;
                Widgets.BeginScrollView(mainRect, ref scrollPosition, viewRect, true);
                
                float num = mainRect.y;
                float num2 = scrollPosition.y - DATAROW_HEIGHT;
                float num3 = mainRect.y + scrollPosition.y + mainRect.height;
                DrawThings(mainRect, viewRect, ref num, ref num2, ref num3, summedUpThingsLabelList, summedUpThings);
                Widgets.EndScrollView();
            }
            catch (Exception ex)
            {
                Log.ErrorOnce(string.Concat(new object[]
                {
                    "Error in Mod ZoneInspectData: ZoneStockpileInspectPaneFiller#DoPaneContentsFor ",
                    Find.Selector.FirstSelectedObject,
                    ": ", ex.ToString()
                }), this.GetHashCode());
            }
            finally
            {
                Text.WordWrap = true;
                Text.Anchor = TextAnchor.UpperLeft;
                GUI.EndGroup();
            }
        }

        public void ResetData()
        {
            lastZoneInspected = null;
            scrollPosition = Vector2.zero;
        }

        private void DrawThings(Rect mainRect, Rect viewRect, ref float num, ref float num2, ref float num3, List<ThingDef> list, Dictionary<ThingDef, int> dict)
        {
            bool success = false;
            foreach (ThingDef tDef in list)
            {
                if (num > num2 && num < num3)
                {
                    Rect rect2 = new Rect(mainRect.x, num, viewRect.width, DATAROW_HEIGHT);
                    success = DrawDataRow(rect2, tDef, dict[tDef]);
                }
                else
                {
                    num += DATAROW_HEIGHT;
                }

                if (success)
                {
                    num += DATAROW_HEIGHT;
                }
            }
        }

        private bool DrawDataRow(Rect rect, ThingDef tDef, int value)
        {
            bool result = true;
            try
            {
                if (tDef.uiIcon != BaseContent.BadTex)
                {
                    Rect rect1 = new Rect(rect.x, rect.y, ICON_WIDTH, ICON_WIDTH);
                    Widgets.ThingIcon(rect1, tDef);
                }

                Rect rect2 = new Rect(rect.x + 35, rect.y, rect.width - ICON_WIDTH - 35, rect.height);
                GUI.color = Color.white;
                Widgets.Label(rect2, tDef.label + " x" + value);
                Widgets.DrawHighlightIfMouseover(rect);
                if (Widgets.ButtonInvisible(rect))
                {
                    Find.Selector.ClearSelection();
                    foreach (Thing t in lastZoneInspected.AllContainedThings)
                    {
                        if (t.def == tDef)
                        {
                            Find.Selector.Select(t, false, false);
                        }
                    }
                }
            } catch (Exception e)
            {
                result = false;
                Log.Error("EXCEPTION in drawing data row: " + tDef.label + " .. " + e.Message);
            }

            return result;
        }

        private void SumUpThings(Zone_Stockpile zone)
        {
            lastZoneInspected = zone;
            summedUpThings.Clear();
            summedUpThingsLabelList.Clear();

            foreach (Thing t in zone.AllContainedThings)
            {
                if (thingDefinitions.Contains(t.def))
                {
                    UpdateLookupData(summedUpThingsLabelList, summedUpThings, t);
                }
            }

            calculatedViewRectHeight = summedUpThings.Count * DATAROW_HEIGHT;
            summedUpThingsLabelList.Sort((ThingDef a, ThingDef b) => a.label.CompareTo(b.label));
        }

        private void UpdateLookupData(List<ThingDef> list, Dictionary<ThingDef, int> dict, Thing t)
        {
            int currentAmount;
            if (dict.TryGetValue(t.def, out currentAmount))
            {
                dict[t.def] = currentAmount + t.stackCount;
            }
            else
            {
                dict.Add(t.def, t.stackCount);
            }
            
            if (! list.Contains(t.def))
            {
                list.Add(t.def);
            }
        }
    }
}
