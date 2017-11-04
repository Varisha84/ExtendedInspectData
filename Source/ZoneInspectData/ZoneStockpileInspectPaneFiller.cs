using System;
using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;

namespace ZoneInspectData

{
    public class ZoneStockpileInspectPaneFiller
    {
        private readonly HashSet<ThingDef> thingDefinitions;
        private readonly Dictionary<ThingDef, int> summedUpThingsWithIcon;
        private readonly Dictionary<ThingDef, int> summedUpThingsWithoutIcon;
        private Zone_Stockpile lastZoneInspected;
        private Vector2 scrollPosition;

        private static readonly float ICON_WIDTH = 27f;
        private static readonly float DATAROW_HEIGHT = 28f;

        public ZoneStockpileInspectPaneFiller()
        {
            lastZoneInspected = null;
            scrollPosition = Vector2.zero;
            summedUpThingsWithIcon = new Dictionary<ThingDef, int>();
            summedUpThingsWithoutIcon = new Dictionary<ThingDef, int>();
            IEnumerable<TreeNode_ThingCategory> categories = ThingCategoryNodeDatabase.AllThingCategoryNodes;
            thingDefinitions = new HashSet<ThingDef>();

            foreach (TreeNode_ThingCategory tc in categories)
            {
                foreach (ThingDef td in tc.catDef.DescendantThingDefs)
                {
                    thingDefinitions.Add(td);
                }
            }
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

                Rect mainRect = new Rect(12f, 16f, rect.width - 24f, rect.height - 24f);
                float height = 4f + (float) (this.summedUpThingsWithIcon.Count  + this.summedUpThingsWithoutIcon.Count) * DATAROW_HEIGHT;
                Rect viewRect = new Rect(mainRect.x, mainRect.y, mainRect.width - 20f, height);
                Widgets.BeginScrollView(mainRect, ref this.scrollPosition, viewRect, true);
                
                float num = mainRect.y + 4f;
                float num2 = this.scrollPosition.y - DATAROW_HEIGHT;
                float num3 = this.scrollPosition.y + mainRect.height;
                DrawThings(mainRect, viewRect, ref num, ref num2, ref num3, summedUpThingsWithIcon, true);
                DrawThings(mainRect, viewRect, ref num, ref num2, ref num3, summedUpThingsWithoutIcon, false);
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

        private void DrawThings(Rect mainRect, Rect viewRect, ref float num, ref float num2, ref float num3, Dictionary<ThingDef, int> dict, bool drawIcon)
        {
            bool success = false;
            foreach (ThingDef tDef in dict.Keys)
            {
                if (num > num2 && num < num3)
                {
                    Rect rect2 = new Rect(mainRect.x + 6, num, viewRect.width, DATAROW_HEIGHT);
                    success = DrawDataRow(rect2, tDef, dict[tDef], drawIcon);
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

        private bool DrawDataRow(Rect rect, ThingDef tDef, int value, bool drawIcon)
        {
            bool result = true;
            try
            {
                if (drawIcon)
                {
                    Rect rect1 = new Rect(rect.x, rect.y, ICON_WIDTH, ICON_WIDTH);
                    Widgets.ThingIcon(rect1, tDef);
                }

                Rect rect2 = new Rect(rect.x + 35, rect.y, rect.width - ICON_WIDTH - 35, rect.height);
                GUI.color = Color.white;
                Widgets.Label(rect2, tDef.label + " x" + value);
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
            summedUpThingsWithIcon.Clear();
            summedUpThingsWithoutIcon.Clear();
            IEnumerator<Thing> thingIt = zone.AllContainedThings.GetEnumerator();

            foreach (Thing t in zone.AllContainedThings)
            {
                if (thingDefinitions.Contains(t.def))
                {
                    if (t.def.uiIcon != BaseContent.BadTex)
                    {
                        UpdateInDictionary(summedUpThingsWithIcon, t);
                    }
                    else
                    {
                        UpdateInDictionary(summedUpThingsWithoutIcon, t);
                    }
                }
            }
        }

        private void UpdateInDictionary(Dictionary<ThingDef, int> dict, Thing t)
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
        }
    }
}
