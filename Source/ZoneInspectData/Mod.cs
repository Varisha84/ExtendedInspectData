// Mod
using HarmonyLib;
using System.Reflection;
using Verse;

[StaticConstructorOnStartup]
internal static class Mod
{
    public static readonly Assembly Assembly;

    public static readonly Harmony Harmony;

    static Mod()
    {
        Assembly = Assembly.GetExecutingAssembly();
        Harmony = new Harmony("ExtendedInspectData");
        Harmony.PatchAll();
    }
}
