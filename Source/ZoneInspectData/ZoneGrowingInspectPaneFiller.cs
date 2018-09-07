﻿using System;
using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;

namespace ZoneInspectData

{
    [StaticConstructorOnStartup]
    public class ZoneGrowingInspectPaneFiller
    {
        private static readonly float TITLE_OFFSET = 40f;
        private static readonly float TOP_BOTTOM_OFFSET = 18f;
        private static readonly int GROWINGZONE_SINGLE_SELECT_INFOHEIGHT = 72;
        private static readonly int GROWINGZONE_SINGLE_SELECT_GRAPHHEIGHT = 140;
        private static readonly int GROWINGZONE_SINGLE_SELECT_YAXIS_LABEL_HEIGHT = 15;
        private static readonly float MULTIPLE_INFO_DATAROW_HEIGHT = 28f;
        private static readonly float REQUIRED_HEIGHT = GROWINGZONE_SINGLE_SELECT_GRAPHHEIGHT + GROWINGZONE_SINGLE_SELECT_INFOHEIGHT + GROWINGZONE_SINGLE_SELECT_YAXIS_LABEL_HEIGHT;
        private static readonly int GROWINGZONE_GRAPH_REFRESHRATE = Verse.GenTicks.TickRareInterval * 2;
        private static readonly Color AXIS_LABEL_COLOR = new Color(0.7f, 0.7f, 0.7f); //see RimWorld.SimpleCurveDrawer#DrawCurveMeasures color2

        private List<SimpleCurveDrawInfo> curves;
        private List<SingleZoneGrowingData> singleZoneDataList;
        private int lastTick;

        private SimpleCurveDrawInfo growthValueDrawInfo;
        private SimpleCurveDrawInfo harvestableMarkerDrawInfo;
        private SimpleCurveDrawerStyle curveDrawerStyle;
        private FloatRange graphSection;
        private Vector2 scrollPosition;

        private static readonly Texture2D emptyCellCountIcon = ContentFinder<Texture2D>.Get("UI/Icons/Medical/TendedNeed", true);
        private static readonly Texture2D nonEmptyCellCountIcon = ContentFinder<Texture2D>.Get("UI/Icons/Medical/TendedWell", true);
        private static readonly Texture2D nonHarvestableIcon = ContentFinder<Texture2D>.Get("UI/Widgets/FillChangeArrowRight", true);
        private static readonly Texture2D harvestableIcon = ContentFinder<Texture2D>.Get("UI/Designators/Harvest", true);
        private static readonly Texture2D fullyGrown = Widgets.CheckboxOnTex;

        private bool sortHarvestableDesc;
        private bool sortFullyGrownDesc;
        private float heightOffset;

        public float HeightOffset
        {
            get
            {
                return heightOffset;
            }

            set
            {
                this.heightOffset = TITLE_OFFSET + value * 18;
            }
        }

        public float RequiredHeight
        {
            get
            {
                return REQUIRED_HEIGHT + (TOP_BOTTOM_OFFSET * 2);
            }
        }


        public ZoneGrowingInspectPaneFiller()
        {
            curves = new List<SimpleCurveDrawInfo>();
            graphSection = new FloatRange(0f, 101f);
            singleZoneDataList = new List<SingleZoneGrowingData>() {new SingleZoneGrowingData()};
            scrollPosition = Vector2.zero;
            sortHarvestableDesc = false;
            sortFullyGrownDesc = false;
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

        public void DoPaneContentsFor(List<Zone_Growing> zones, Rect rect)
        {
            if (((Find.TickManager.TicksGame - lastTick) > GROWINGZONE_GRAPH_REFRESHRATE) || 
                (singleZoneDataList.Count == 0) ||
                (singleZoneDataList.Count != zones.Count) ||
                ((singleZoneDataList.Count > 0) && (singleZoneDataList[0].zone != zones[0])))
            {
                lastTick = Find.TickManager.TicksGame;
                GatherData(zones, singleZoneDataList);
            }

            if (singleZoneDataList.Count == 1)
            {
                DrawSingleSelectionInfo(rect);
            } else if (singleZoneDataList.Count > 1)
            {
                DrawMultipleSelectionInfo(rect);
            }
            
        }

        public void ResetData()
        {
            singleZoneDataList.Clear();
            curves.Clear();
            this.HeightOffset = 0f;
        }

        private void DrawSingleSelectionInfo(Rect rect)
        {
            //Draw Graph
            try
            {
                SingleZoneGrowingData singleZoneData = singleZoneDataList[0];
                GUI.BeginGroup(rect);
                //-20f for x due to adjustments when displaying measures
                Rect yAxisLabelRect = new Rect(12f, heightOffset + TOP_BOTTOM_OFFSET + 10f, rect.width - 12f, GROWINGZONE_SINGLE_SELECT_YAXIS_LABEL_HEIGHT);
                Rect graphRect = new Rect(-20f, yAxisLabelRect.yMax, rect.width - 24f, GROWINGZONE_SINGLE_SELECT_GRAPHHEIGHT);
                Rect xAxisLabelRect = new Rect(12f, graphRect.yMax - 6f, rect.width - 36f, 20f);
                Rect infoRect = new Rect(40f, xAxisLabelRect.yMax, graphRect.width - 80f, GROWINGZONE_SINGLE_SELECT_INFOHEIGHT);

                //draw graph and labels
                Text.Anchor = TextAnchor.MiddleLeft;
                Text.Font = GameFont.Tiny;
                GUI.color = AXIS_LABEL_COLOR;
                Widgets.Label(yAxisLabelRect, "Value".Translate() + " (%)");
                Text.Anchor = TextAnchor.UpperRight;
                Widgets.Label(xAxisLabelRect, "PercentGrowth".Translate(new object[] { "%" }));
                SimpleCurveDrawer.DrawCurves(graphRect, this.curves, curveDrawerStyle, null);

                //draw infos
                Text.Anchor = TextAnchor.MiddleCenter;
                Text.Font = GameFont.Tiny;
                GUI.color = AXIS_LABEL_COLOR;
                float singleInfoWidth = infoRect.width / 5f;

                //Draw total empty cells
                Rect iconRect1 = new Rect(infoRect.x + (singleInfoWidth/2) -10f, infoRect.y + 5f, 20f, 20f);
                GUI.DrawTexture(iconRect1, emptyCellCountIcon);
                Rect emptyCellRectLabel = new Rect(infoRect.x, iconRect1.yMax + 10f, singleInfoWidth, 20f);
                Widgets.Label(emptyCellRectLabel, "x" + (singleZoneData.zone.Cells.Count - singleZoneData.totalPlantedCount));

                //Draw total cells with plants
                Rect iconRect2 = new Rect(iconRect1.x + singleInfoWidth, iconRect1.y, 20f, 20f);
                GUI.DrawTexture(iconRect2, nonEmptyCellCountIcon);
                Rect nonEmptyCellRectLabel = new Rect(emptyCellRectLabel.xMax, emptyCellRectLabel.y, singleInfoWidth, 20f);
                Widgets.Label(nonEmptyCellRectLabel, "x" + singleZoneData.totalPlantedCount);

                //Draw non harvestable cells (growth value limit not reached)
                Rect iconRect3 = new Rect(iconRect2.x + singleInfoWidth -1 , iconRect1.y, 10f, 20f);
                Rect iconRect4 = new Rect(iconRect3.xMax + 2f, iconRect1.y, 10f, 20f);
                GUI.DrawTexture(iconRect3, nonHarvestableIcon);
                GUI.DrawTexture(iconRect4, nonHarvestableIcon);
                Rect nonHarvestableCellRectLabel = new Rect(nonEmptyCellRectLabel.xMax, emptyCellRectLabel.y, singleInfoWidth, 20f);
                Widgets.Label(nonHarvestableCellRectLabel, "x" + (singleZoneData.totalPlantedCount - singleZoneData.harvestablePlants.Count));

                //Draw harvestable cells (growth value limit reached)
                Rect iconRect5 = new Rect(iconRect3.x + singleInfoWidth, iconRect1.y, 20f, 20f);
                GUI.DrawTexture(iconRect5, harvestableIcon);
                if (Mouse.IsOver(iconRect5))
                {
                    Widgets.DrawBox(iconRect5);
                }
                Rect harvestableCellRectLabel = new Rect(nonHarvestableCellRectLabel.xMax, emptyCellRectLabel.y, singleInfoWidth, 20f);
                Widgets.Label(harvestableCellRectLabel, "x" + singleZoneData.harvestablePlants.Count);
                if (Widgets.ButtonInvisible(iconRect5))
                {
                    Find.Selector.ClearSelection();
                    foreach (Thing t in singleZoneData.harvestablePlants)
                    {
                        if (!t.Destroyed)
                        {
                            Find.Selector.Select(t, false);
                        }
                    }
                }

                //Draw fully grown cells (growth value >= 100%)
                Rect iconRect6 = new Rect(iconRect5.x + singleInfoWidth, iconRect1.y, 20f, 20f);
                GUI.DrawTexture(iconRect6, fullyGrown);
                if (Mouse.IsOver(iconRect6))
                {
                    Widgets.DrawBox(iconRect6);
                }
                Rect fullyGrownCellRectLabel = new Rect(harvestableCellRectLabel.xMax, emptyCellRectLabel.y, singleInfoWidth, 20f);
                Widgets.Label(fullyGrownCellRectLabel, "x" + singleZoneData.fullyGrownPlants.Count);
                if (Widgets.ButtonInvisible(iconRect6))
                {
                    Find.Selector.ClearSelection();
                    foreach (Thing t in singleZoneData.fullyGrownPlants)
                    {
                        if (!t.Destroyed) {
                            Find.Selector.Select(t, false);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Log.ErrorOnce(string.Concat(new object[]
                {
                    "Error in Mod ZoneInspectData: ZoneGrowingInspectPaneFiller#DrawGraph ",
                    Find.Selector.FirstSelectedObject,
                    ": ", ex.StackTrace
                }), this.GetHashCode());
            }
            finally
            {
                Text.Anchor = TextAnchor.UpperLeft;
                GUI.EndGroup();
            }
        }

        private void DrawMultipleSelectionInfo(Rect rect)
        {
            try
            {
                GUI.BeginGroup(rect);
                Text.Anchor = TextAnchor.MiddleLeft;
                Text.Font = GameFont.Small;
                //Draw header
                float iconHeaderWidth = rect.width * 0.55f;
                float singleInfoWidth = iconHeaderWidth / 5f;

                Rect labelHeaderRect = new Rect(16f, heightOffset + TOP_BOTTOM_OFFSET, rect.width - iconHeaderWidth - 16f, 20f);
                Widgets.Label(labelHeaderRect, "PlantVerb".Translate());
                Rect iconHeaderRect = new Rect(rect.width - iconHeaderWidth - 30, labelHeaderRect.y, iconHeaderWidth -20 , labelHeaderRect.height);
                Rect iconRect1 = new Rect(iconHeaderRect.x + (singleInfoWidth / 2) - 10f, iconHeaderRect.y, 20f, labelHeaderRect.height);
                GUI.DrawTexture(iconRect1, emptyCellCountIcon);
                
                Rect iconRect2 = new Rect(iconRect1.x + singleInfoWidth, iconRect1.y, iconRect1.width, labelHeaderRect.height);
                GUI.DrawTexture(iconRect2, nonEmptyCellCountIcon);
                Rect iconRect3 = new Rect(iconRect2.x + singleInfoWidth - 1, iconRect1.y, iconRect1.width / 2, labelHeaderRect.height);
                Rect iconRect4 = new Rect(iconRect3.xMax + 2f, iconRect1.y, iconRect1.width / 2, labelHeaderRect.height);
                GUI.DrawTexture(iconRect3, nonHarvestableIcon);
                GUI.DrawTexture(iconRect4, nonHarvestableIcon);
                Rect iconRect5 = new Rect(iconRect3.x + singleInfoWidth, iconRect1.y, iconRect1.width, labelHeaderRect.height);
                GUI.DrawTexture(iconRect5, harvestableIcon);
                if (Widgets.ButtonInvisible(iconRect5))
                {
                    sortHarvestableDesc = !sortHarvestableDesc;
                    sortFullyGrownDesc = false;
                    UpdateSorting();
                }
                Rect iconRect6 = new Rect(iconRect5.x + singleInfoWidth, iconRect1.y, iconRect1.width, labelHeaderRect.height);
                GUI.DrawTexture(iconRect6, fullyGrown);
                if (Widgets.ButtonInvisible(iconRect6))
                {
                    sortHarvestableDesc = false;
                    sortFullyGrownDesc = !sortFullyGrownDesc;
                    UpdateSorting();
                }

                if (sortHarvestableDesc)
                {
                    Widgets.DrawBox(iconRect5);
                }
                else if (sortFullyGrownDesc)
                {
                    Widgets.DrawBox(iconRect6);
                }

                //draw scroll view
                DrawScrollView(rect, labelHeaderRect, iconHeaderRect, ref singleInfoWidth);
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
                Text.Anchor = TextAnchor.UpperLeft;
                GUI.EndGroup();
            }
        }

        private void DrawScrollView(Rect rect, Rect labelHeaderRect, Rect iconHeaderRect, ref float singleInfoWidth)
        {
            Text.WordWrap = true;
            float calculatedViewHeight = singleZoneDataList.Count * MULTIPLE_INFO_DATAROW_HEIGHT;
            Rect mainRect = new Rect(labelHeaderRect.x, labelHeaderRect.yMax + 15, rect.width - 28f, rect.height - labelHeaderRect.yMax - 18f);
            Rect viewRect = new Rect(mainRect.x, mainRect.y, mainRect.width - 20f, calculatedViewHeight);
            Widgets.DrawLineHorizontal(labelHeaderRect.x, mainRect.y - 5, mainRect.width);
            Widgets.BeginScrollView(mainRect, ref scrollPosition, viewRect, true);
            GUI.color = Color.white;

            float num = mainRect.y;
            float num2 = scrollPosition.y - MULTIPLE_INFO_DATAROW_HEIGHT;
            float num3 = mainRect.y + scrollPosition.y + mainRect.height;
            DrawZoneInfos(mainRect, viewRect, iconHeaderRect, ref singleInfoWidth, ref num, ref num2, ref num3, singleZoneDataList);
            Widgets.EndScrollView();
        }

        private void DrawZoneInfos(Rect mainRect, Rect viewRect, Rect headerRect, ref float singleInfoWidth, ref float num, ref float num2, ref float num3, List<SingleZoneGrowingData> list)
        {
            bool success = false;

            foreach (SingleZoneGrowingData data in list)
            {
                if (num > num2 && num < num3)
                {
                    Rect rect1 = new Rect(mainRect.x, num, viewRect.width, MULTIPLE_INFO_DATAROW_HEIGHT);
                    success = DrawZoneInfo(rect1, headerRect, ref singleInfoWidth, data);
                }
                else
                {
                    num += MULTIPLE_INFO_DATAROW_HEIGHT;
                }

                if (success)
                {
                    num += MULTIPLE_INFO_DATAROW_HEIGHT;
                }
            }
        }

        private bool DrawZoneInfo(Rect rect, Rect headerRect, ref float singleInfoWidth, SingleZoneGrowingData data)
        {
            bool result = true;
            try
            {
                Rect rect1 = new Rect(rect.x, rect.y, headerRect.xMin, rect.height);
                GUI.color = Color.white;
                //write plant name
                Text.Anchor = TextAnchor.MiddleLeft;
                Text.Font = GameFont.Small;
                Widgets.Label(rect1, data.zone.GetPlantDefToGrow().label);
                //write numbers
                Text.Anchor = TextAnchor.MiddleCenter;
                Text.Font = GameFont.Tiny;
                Rect emptyCellRectLabel = new Rect(headerRect.x, rect1.y, singleInfoWidth, rect.height);
                Rect nonEmptyCellRectLabel = new Rect(emptyCellRectLabel.xMax, emptyCellRectLabel.y, singleInfoWidth, rect.height);
                Rect nonHarvestableCellRectLabel = new Rect(nonEmptyCellRectLabel.xMax, emptyCellRectLabel.y, singleInfoWidth, rect.height);
                Rect harvestableCellRectLabel = new Rect(nonHarvestableCellRectLabel.xMax, emptyCellRectLabel.y, singleInfoWidth, rect.height);
                Rect fullyGrownCellRectLabel = new Rect(harvestableCellRectLabel.xMax, emptyCellRectLabel.y, singleInfoWidth, rect.height);
                Widgets.Label(emptyCellRectLabel, "x" + (data.zone.Cells.Count - data.totalPlantedCount));
                Widgets.Label(nonEmptyCellRectLabel, "x" + data.totalPlantedCount);
                Widgets.Label(nonHarvestableCellRectLabel, "x" + (data.totalPlantedCount - data.harvestablePlants.Count));
                Widgets.Label(harvestableCellRectLabel, "x" + data.harvestablePlants.Count);
                Widgets.Label(fullyGrownCellRectLabel, "x" + data.fullyGrownPlants.Count);
                //additional feature
                Widgets.DrawHighlightIfMouseover(rect);
                if (Widgets.ButtonInvisible(rect)) {
                    Find.Selector.ClearSelection();
                    Find.Selector.Select(data.zone);
                }
            }
            catch (Exception e)
            {
                result = false;
                Log.Error("EXCEPTION in drawing data row: " + data.zone.GetPlantDefToGrow() + " .. " + e.Message);
            }

            return result;
        }

        private void GatherData(List<Zone_Growing> zones, List<SingleZoneGrowingData> dataList)
        {
            ResetData();
            SingleZoneGrowingData newData;
            foreach (Zone_Growing zone in zones)
            {
                GatherSingleZoneData(zone, out newData);
                dataList.Add(newData);
            }

            UpdateSorting();
        }

        private void GatherSingleZoneData(Zone_Growing zone, out SingleZoneGrowingData data)
        {
            data = new SingleZoneGrowingData();
            data.zone = zone;
            
            Plant plant;
            int growthRate;
            float harvestMinGrowth = zone.GetPlantDefToGrow().plant.harvestMinGrowth * 100;

            //analyze growth values
            foreach (Thing t in zone.AllContainedThings)
            {
                if (t.def == zone.GetPlantDefToGrow())
                {
                    plant = t as Plant;
                    if (plant != null)
                    {                        
                        growthRate = (int) (plant.Growth * 100);
                        data.growRatesAbsolute[growthRate]++;
                        data.totalPlantedCount++;

                        if (growthRate >= 100)
                        {
                            data.fullyGrownPlants.Add(t);
                        }

                        if (growthRate >= harvestMinGrowth)
                        {
                            data.harvestablePlants.Add(t);
                        }
                    }
                }
            }

            //add curve points
            growthValueDrawInfo.curve = new SimpleCurve();
            float maxYValue = 0;
            if (data.totalPlantedCount > 0)
            {
                for (int i = 0; i < data.growRatesAbsolute.Length; i++)
                {
                    growthValueDrawInfo.curve.Add(new CurvePoint((float)i, 100 * data.growRatesAbsolute[i] / data.totalPlantedCount), false);
                }
                maxYValue = 100 * data.growthRateMaxCount / data.totalPlantedCount + 5;
            }
            else
            {
                growthValueDrawInfo.curve.Add(new CurvePoint(0f, 0f), false);
                growthValueDrawInfo.curve.Add(new CurvePoint(100f, 0f), false);
                maxYValue = 5;
            }

            //draw vertical marker
            harvestableMarkerDrawInfo.color = Color.white;
            harvestableMarkerDrawInfo.curve = new SimpleCurve();
            harvestableMarkerDrawInfo.curve.Add(-5, -5f, false);
            harvestableMarkerDrawInfo.curve.Add(harvestMinGrowth, -5f, false);
            harvestableMarkerDrawInfo.curve.Add(harvestMinGrowth, Math.Min(100, maxYValue), false);
            harvestableMarkerDrawInfo.curve.Add(harvestMinGrowth, -5f, false);

            curves.Add(growthValueDrawInfo);
            curves.Add(harvestableMarkerDrawInfo);
        }

        private void UpdateSorting()
        {
            //sort list
            if (sortHarvestableDesc)
            {
                singleZoneDataList.SortByDescending((SingleZoneGrowingData x) => x.harvestablePlants.Count, (SingleZoneGrowingData x) => x.zone.GetPlantDefToGrow().label);
            }
            else if (sortFullyGrownDesc)
            {
                singleZoneDataList.SortByDescending((SingleZoneGrowingData x) => x.fullyGrownPlants.Count, (SingleZoneGrowingData x) => x.zone.GetPlantDefToGrow().label);
            }
            else
            {
                singleZoneDataList.Sort((SingleZoneGrowingData a, SingleZoneGrowingData b) => a.zone.GetPlantDefToGrow().label.CompareTo(b.zone.GetPlantDefToGrow().label));
            }
        }
    }
}
