using Harmony;
using RimWorld;
using Verse;
using System.Collections.Generic;

namespace ExtendedInspectData
{
    [HarmonyPatch(typeof(MainTabWindow_Inspect), "PaneTopY", MethodType.Getter)]
    internal class MainTabWindow_Inspect_PaneTopY
    {
       
        static bool Prefix(MainTabWindow_Inspect __instance, ref float __result)
        {
            List<object> things = Find.Selector.SelectedObjects.FindAll(thing => (thing as Zone_Growing) != null);

            if ((things.Count > 0) && (things.Count == Find.Selector.NumSelected))
            {
                __result = (float)UI.screenHeight - __instance.RequestedTabSize.y - 35f;
                return false;
            }
            else
            {
                things = Find.Selector.SelectedObjects.FindAll(thing => (thing as Building_Storage) != null);
                if ((things.Count > 0) && (things.Count == Find.Selector.NumSelected))
                {
                    __result = (float)UI.screenHeight - __instance.RequestedTabSize.y - 35f;
                    return false;
                }
            }

            return true;
        }
    }
}
