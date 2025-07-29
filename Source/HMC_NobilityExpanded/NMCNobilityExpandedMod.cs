using HarmonyLib;
using Verse;

namespace NobilityExpanded
{
    [StaticConstructorOnStartup]
    public static class HarmonyPatches
    {
        static HarmonyPatches()
        {
            Harmony harmonyInstance = new Harmony("hmc.pacas.empire");
            harmonyInstance.PatchAll();
        }
    }
}
