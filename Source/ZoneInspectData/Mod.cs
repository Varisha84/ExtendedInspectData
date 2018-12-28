// Mod
using Harmony;
using System.Reflection;
using Verse;

[StaticConstructorOnStartup]
internal static class Mod
{
    public static readonly Assembly Assembly;

    public static readonly HarmonyInstance Harmony;

    static Mod()
    {
        Assembly = Assembly.GetExecutingAssembly();
        Harmony = HarmonyInstance.Create("ExtendedInspectData");
        Harmony.PatchAll();
    }
}
