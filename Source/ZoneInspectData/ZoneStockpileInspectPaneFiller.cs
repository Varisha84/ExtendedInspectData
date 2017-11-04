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
        private readonly Dictionary<ThingDef, int> summedUpThings;
        private Zone_Stockpile lastZoneInspected;
        private Vector2 scrollPosition;

        public ZoneStockpileInspectPaneFiller()
        {
            lastZoneInspected = null;
            scrollPosition = Vector2.zero;
            summedUpThings = new Dictionary<ThingDef, int>();
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
                float height = 4f + (float)this.summedUpThings.Count * 28f;
                Rect viewRect = new Rect(mainRect.x, mainRect.y, mainRect.width - 20f, height);
                Widgets.BeginScrollView(mainRect, ref this.scrollPosition, viewRect, true);
                float num = mainRect.y + 4f;
                float num2 = this.scrollPosition.y - 28f;
                float num3 = this.scrollPosition.y + mainRect.height;
                bool success = false;

                foreach (ThingDef tDef in summedUpThings.Keys)
                {
                    if (num > num2 && num < num3)
                    {
                        Rect rect2 = new Rect(mainRect.x + 6, num, viewRect.width, 28f);
                        success = DrawDataRow(rect2, tDef);
                    } else
                    {
                        num += 28f;
                    }

                    if (success)
                    {
                        num += 28f;
                    }
                }
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

        private bool DrawDataRow(Rect rect, ThingDef tDef)
        {
            bool result = true;
            try
            {
                Rect rect1 = new Rect(rect.x, rect.y, 27f, 27f);
                Widgets.ThingIcon(rect1, tDef);
                Rect rect2 = new Rect(rect1.x + 35, rect.y, rect.width - rect1.width - 35, rect.height);
                GUI.color = Color.white;
                Widgets.Label(rect2, tDef.label + " x" + summedUpThings[tDef]);
            } catch (Exception e)
            {
                result = false;
            }

            return result;
        }

        private void SumUpThings(Zone_Stockpile zone)
        {
            lastZoneInspected = zone;
            summedUpThings.Clear();
            IEnumerator<Thing> thingIt = zone.AllContainedThings.GetEnumerator();
            int currentAmount;

            foreach (Thing t in zone.AllContainedThings)
            {
                currentAmount = 0;

                if ((t.def.uiIcon != BaseContent.BadTex) && (thingDefinitions.Contains(t.def)))
                {
                    if (summedUpThings.TryGetValue(t.def, out currentAmount))
                    {
                        summedUpThings[t.def] = currentAmount + t.stackCount;
                    }
                    else
                    {
                        summedUpThings.Add(t.def, t.stackCount);
                    }
                }
            }
        }
    }
}
