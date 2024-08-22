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
        public static bool Prefix(RoyalTitleDef title, ref bool __result)
        {
            RoyalTitleDef Emperor = DefDatabase<RoyalTitleDef>.GetNamedSilentFail("Emperor");
            RoyalTitleDef Stellarch = DefDatabase<RoyalTitleDef>.GetNamedSilentFail("Stellarch");
            RoyalTitleDef August = DefDatabase<RoyalTitleDef>.GetNamedSilentFail("VFEE_HighStellarch");
            RoyalTitleDef MinorHead = DefDatabase<RoyalTitleDef>.GetNamedSilentFail("MinorHead");
            RoyalTitleDef Freeholder = DefDatabase<RoyalTitleDef>.GetNamedSilentFail("Freeholder");
            RoyalTitleDef Yeoman = DefDatabase<RoyalTitleDef>.GetNamedSilentFail("Yeoman");
            RoyalTitleDef Acolyte = DefDatabase<RoyalTitleDef>.GetNamedSilentFail("Acolyte");
            __result = title != Emperor && title != Stellarch && title != August && title != MinorHead && title != Freeholder && title != Yeoman && title != Acolyte;
            return false;
        }
    }
}