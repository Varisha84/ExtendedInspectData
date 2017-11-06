using System;
using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;

namespace ZoneInspectData

{
    public class ZoneGrowingInspectPaneFiller
    {
        public static readonly int GROWINGZONE_SINGLE_SELECT_ADDITIONAL_HEIGHT = 172;
        private static readonly int GROWINGZONE_SINGLE_SELECT_INFOHEIGHT = 72;
        private static readonly int GROWINGZONE_SINGLE_SELECT_GRAPHHEIGHT = 140;
        private static readonly int GROWINGZONE_GRAPH_REFRESHRATE = 250;
        private static readonly Color AXIS_LABEL_COLOR = new Color(0.7f, 0.7f, 0.7f); //see RimWorld.SimpleCurveDrawer#DrawCurveMeasures color2

        private Zone_Growing lastZoneInspected;
        private int[] growRatesAbsolute;
        private int totalPlantedCount;
        private int totalHarvestableCount;
        private List<SimpleCurveDrawInfo> curves;
        private int tickCount;

        private SimpleCurveDrawInfo growthValueDrawInfo;
        private SimpleCurveDrawInfo harvestableMarkerDrawInfo;
        private SimpleCurveDrawerStyle curveDrawerStyle;
        private FloatRange graphSection;

        private static readonly Texture2D emptyCellCountIcon = ContentFinder<Texture2D>.Get("UI/Icons/Medical/TendedNeed", true);
        private static readonly Texture2D nonEmptyCellCountIcon = ContentFinder<Texture2D>.Get("UI/Icons/Medical/TendedWell", true);
        private static readonly Texture2D harvestableIcon = ContentFinder<Texture2D>.Get("UI/Designators/Harvest", true);
        private static readonly Texture2D nonHarvestableIcon = ContentFinder<Texture2D>.Get("UI/Widgets/FillChangeArrowRight", true);



        public ZoneGrowingInspectPaneFiller()
        {
            lastZoneInspected = null;
            totalPlantedCount = 0;
            curves = new List<SimpleCurveDrawInfo>();
            graphSection = new FloatRange(0f, 101f);
            growRatesAbsolute = new int[101];
            for (int i=0; i<101; i++)
            {
                growRatesAbsolute[i] = 0;
            }

            growthValueDrawInfo = new SimpleCurveDrawInfo();
            growthValueDrawInfo.color = Color.yellow;
            curveDrawerStyle = new SimpleCurveDrawerStyle();
            curveDrawerStyle.DrawPoints = false;
            curveDrawerStyle.DrawBackground = false;
            curveDrawerStyle.DrawBackgroundLines = false;
            curveDrawerStyle.UseAntiAliasedLines = true;
            curveDrawerStyle.OnlyPositiveValues = true;
            curveDrawerStyle.DrawMeasures = true;
            curveDrawerStyle.MeasureLabelsXCount = 10;
            curveDrawerStyle.MeasureLabelsYCount = 5;
            curveDrawerStyle.PointsRemoveOptimization = true;
            curveDrawerStyle.UseFixedSection = true;
            curveDrawerStyle.FixedSection = graphSection;
            harvestableMarkerDrawInfo = new SimpleCurveDrawInfo();
        }

        public void DoPaneContentsFor(Zone_Growing zone, Rect rect)
        {
            if ((tickCount > GROWINGZONE_GRAPH_REFRESHRATE) || (lastZoneInspected != zone))
            {
                tickCount = 0;
                GatherData(zone);
            }

            tickCount++;
            DrawGraph(rect);
        }

        private void DrawGraph(Rect rect)
        {
            try
            {
                GUI.BeginGroup(rect);
                //-20f for x due to adjustments when displaying measures
                Rect graphRect = new Rect(-20f, 100f, rect.width - 24f, GROWINGZONE_SINGLE_SELECT_GRAPHHEIGHT);
                Rect yAxisLabelRect = new Rect(12f, graphRect.yMin - 12, rect.width - 24, 20);
                Rect xAxisLabelRect = new Rect(12f, graphRect.yMax-6, rect.width - 36, 20);
                Rect infoRect = new Rect(12f, xAxisLabelRect.yMax, rect.width - 24f, GROWINGZONE_SINGLE_SELECT_INFOHEIGHT - 12f);

                //draw graph and labels
                Text.Anchor = TextAnchor.MiddleLeft;
                Text.Font = GameFont.Tiny;
                GUI.color = AXIS_LABEL_COLOR;
                Widgets.Label(yAxisLabelRect, "Value".Translate() + " (%)");
                Text.Anchor = TextAnchor.UpperRight;
                Widgets.Label(xAxisLabelRect, "PercentGrowth".Translate(new object[] { "%" }));
                SimpleCurveDrawer.DrawCurves(graphRect, this.curves, curveDrawerStyle, null);

                //draw infos
                Text.Anchor = TextAnchor.MiddleLeft;
                Text.Font = GameFont.Tiny;
                GUI.color = AXIS_LABEL_COLOR;
                //Draw total empty cells
                Rect iconRect1 = new Rect(infoRect.x + 20f, infoRect.y + 5f, 20f, 20f);
                GUI.DrawTexture(iconRect1, emptyCellCountIcon);
                Rect emptyCellRectLabel = new Rect(iconRect1.xMax + 6f, infoRect.y, 60f, 30f);
                Widgets.Label(emptyCellRectLabel, "x" + (lastZoneInspected.Cells.Count - totalPlantedCount));
                
                //Draw total cells with plants
                Rect iconRect2 = new Rect(emptyCellRectLabel.xMax, iconRect1.y, 20f, 20f);
                GUI.DrawTexture(iconRect2, nonEmptyCellCountIcon);
                Rect nonEmptyCellRectLabel = new Rect(iconRect2.xMax + 6f, iconRect2.y - 5f, 60f, 30f);
                Widgets.Label(nonEmptyCellRectLabel, "x" + totalPlantedCount);

                //Draw non harvestable cells (growth value limit not reached)
                Rect iconRect3 = new Rect(nonEmptyCellRectLabel.xMax, iconRect1.y, 12f, 20f);
                Rect iconRect4 = new Rect(iconRect3.xMax + 1f, iconRect1.y, 12f, 20f);
                GUI.DrawTexture(iconRect3, nonHarvestableIcon);
                GUI.DrawTexture(iconRect4, nonHarvestableIcon);
                Rect nonHarvestableCellRectLabel = new Rect(iconRect4.xMax + 6f, iconRect3.y - 5f, 60f, 30f);
                Widgets.Label(nonHarvestableCellRectLabel, "x" + (totalPlantedCount - totalHarvestableCount));

                //Draw harvestable cells (growth value limit reached)
                Rect iconRect5 = new Rect(nonHarvestableCellRectLabel.xMax, iconRect1.y, 24f, 24f);
                GUI.DrawTexture(iconRect5, harvestableIcon);
                Rect harvestableCellRectLabel = new Rect(iconRect5.xMax + 6f, iconRect5.y - 3f, 60f, 30f);
                Widgets.Label(harvestableCellRectLabel, "x" + totalHarvestableCount);
            }
            catch (Exception ex)
            {
                Log.ErrorOnce(string.Concat(new object[]
                {
                    "Error in Mod ZoneInspectData: ZoneGrowingInspectPaneFiller#DrawGraph ",
                    Find.Selector.FirstSelectedObject,
                    ": ", ex.ToString()
                }), this.GetHashCode());
            }
            finally
            {
                Text.Anchor = TextAnchor.UpperLeft;
                GUI.EndGroup();
            }
        }

        public void ResetData()
        {
            lastZoneInspected = null;
            totalPlantedCount = 0;
            totalHarvestableCount = 0;
            curves.Clear();
            for (int i = 0; i < 101; i++)
            {
                growRatesAbsolute[i] = 0;
            }
        }

        private void GatherData(Zone_Growing zone)
        {
            ResetData();
            lastZoneInspected = zone;

            
            Plant plant;
            int growthRate;
            int maxCountIndex = 0;
            float harvestMinGrowth = zone.GetPlantDefToGrow().plant.harvestMinGrowth * 100;

            foreach (Thing t in zone.AllContainedThings)
            {
                if (t.def == zone.GetPlantDefToGrow())
                {
                    plant = t as Plant;
                    if (plant != null)
                    {                        
                        growthRate = (int) (plant.Growth * 100);
                        growRatesAbsolute[growthRate]++;
                        totalPlantedCount++;

                        if (growRatesAbsolute[maxCountIndex] < growRatesAbsolute[growthRate])
                        {
                            maxCountIndex = growthRate;
                        }

                        if (growthRate >= harvestMinGrowth)
                        {
                            totalHarvestableCount++;
                        }
                    }
                }
            }

            growthValueDrawInfo.curve = new SimpleCurve();
            float maxYValue = 0;
            if (totalPlantedCount > 0)
            {
                for (int i = 0; i < growRatesAbsolute.Length; i++)
                {
                    growthValueDrawInfo.curve.Add(new CurvePoint((float)i, 100 * growRatesAbsolute[i] / totalPlantedCount), false);
                }
                maxYValue = 100 * growRatesAbsolute[maxCountIndex] / totalPlantedCount + 5;
            } else
            {
                growthValueDrawInfo.curve.Add(new CurvePoint(0f, 0f), false);
                growthValueDrawInfo.curve.Add(new CurvePoint(100f, 0f), false);
                maxYValue = 5;
            }

            
            harvestableMarkerDrawInfo.curve = new SimpleCurve();
            harvestableMarkerDrawInfo.color = Color.white;
            harvestableMarkerDrawInfo.curve.Add(-5, -5f, false);
            harvestableMarkerDrawInfo.curve.Add(harvestMinGrowth, -5f, false);
            harvestableMarkerDrawInfo.curve.Add(harvestMinGrowth, Math.Min(100, maxYValue), false);
            harvestableMarkerDrawInfo.curve.Add(harvestMinGrowth, -5f, false);

            curves.Add(growthValueDrawInfo);
            curves.Add(harvestableMarkerDrawInfo);
        }
    }
}
