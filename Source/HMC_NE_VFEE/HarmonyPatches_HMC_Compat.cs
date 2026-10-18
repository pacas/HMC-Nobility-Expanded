using HarmonyLib;
using RimWorld;
using Verse;
using VFEEmpire;

namespace HMC_NE_VFEE
{
    [StaticConstructorOnStartup]
    public static class VFEE_Patches
    {
        static VFEE_Patches()
        {
            Harmony harmonyInstance = new Harmony("hmc.pacas.empire.vfee");
			harmonyInstance.PatchAll();
        }
    }
    
    [HarmonyPatch(typeof(EmpireTitleUtility), "CanInvite")]
    public static class CanInvitePatch
    {
        // VFEE itself only excludes Emperor and HighStellarch, heads of the houses and the lowest titles are added here
        private static readonly HashSet<string> NonInvitableTitles = new HashSet<string>
        {
            "Emperor",
            "Stellarch",
            "VFEE_HighStellarch",
            "MinorHead",
            "Freeholder",
            "Yeoman",
            "Acolyte",
        };

        public static void Postfix(RoyalTitleDef title, ref bool __result)
        {
            if (title != null && NonInvitableTitles.Contains(title.defName))
                __result = false;
        }
    }
}