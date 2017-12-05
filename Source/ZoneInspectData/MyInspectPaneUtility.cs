using RimWorld;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace ZoneInspectData
{
    //This class was created to fix the issue that tab buttons over inspect panel are not displayed if inspect panel is resized.
    //This is mostly copied from RimWorld sources and is therefor quite critical if core classes update or interfaces change

    internal class MyInspectPaneUtility
    {
        private static readonly Texture2D InspectTabButtonFillTex = SolidColorMaterials.NewSolidColorTexture(new Color(0.07450981f, 0.08627451f, 0.105882354f, 1f));

        public static void ExtraOnGUI(IInspectPane pane)
        {
            if (pane.AnythingSelected)
            {
                if (KeyBindingDefOf.SelectNextInCell.KeyDownEvent)
                {
                    pane.SelectNextInCell();
                }
                pane.DrawInspectGizmos();
                MyInspectPaneUtility.DoMyTabs(pane);
            }
        }

        private static void DoMyTabs(IInspectPane pane)
        {
            try
            {
                float y = pane.PaneTopY - 30f;
                MainTabWindow_InspectZone_Stockpile myPane = pane as MainTabWindow_InspectZone_Stockpile;
                if (myPane != null)
                {
                    y = myPane.PaneTopYNew - 30f;
                }
                
                float num = InspectPaneUtility.PaneWidthFor(pane) - 72f;
                float width = 0f;
                bool flag = false;
                foreach (InspectTabBase current in pane.CurTabs)
                {
                    if (current.IsVisible)
                    {
                        Rect rect = new Rect(num, y, 72f, 30f);
                        width = num;
                        Text.Font = GameFont.Small;
                        if (Widgets.ButtonText(rect, current.labelKey.Translate(), true, false, true))
                        {
                            MyInspectPaneUtility.InterfaceToggleTab(current, pane);
                        }
                        bool flag2 = current.GetType() == pane.OpenTabType;
                        if (!flag2 && !current.TutorHighlightTagClosed.NullOrEmpty())
                        {
                            UIHighlighter.HighlightOpportunity(rect, current.TutorHighlightTagClosed);
                        }
                        if (flag2)
                        {
                            current.DoTabGUI();
                            pane.RecentHeight = 700f;
                            flag = true;
                        }
                        num -= 72f;
                    }
                }
                if (flag)
                {
                    GUI.DrawTexture(new Rect(0f, y, width, 30f), MyInspectPaneUtility.InspectTabButtonFillTex);
                }
            }
            catch (System.Exception ex)
            {
                Log.ErrorOnce(ex.ToString(), 742783);
            }
        }

        private static void InterfaceToggleTab(InspectTabBase tab, IInspectPane pane)
        {
            if (TutorSystem.TutorialMode && !MyInspectPaneUtility.IsOpen(tab, pane) && !TutorSystem.AllowAction("ITab-" + tab.tutorTag + "-Open"))
            {
                return;
            }
            MyInspectPaneUtility.ToggleTab(tab, pane);
        }

        private static void ToggleTab(InspectTabBase tab, IInspectPane pane)
        {
            if (MyInspectPaneUtility.IsOpen(tab, pane) || (tab == null && pane.OpenTabType == null))
            {
                pane.OpenTabType = null;
                SoundDefOf.TabClose.PlayOneShotOnCamera(null);
            }
            else
            {
                tab.OnOpen();
                pane.OpenTabType = tab.GetType();
                SoundDefOf.TabOpen.PlayOneShotOnCamera(null);
            }
        }

        private static bool IsOpen(InspectTabBase tab, IInspectPane pane)
        {
            return tab.GetType() == pane.OpenTabType;
        }
    }
}