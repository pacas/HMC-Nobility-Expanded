using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using Verse;
using VFEEmpire;

namespace HMC_NE_VFEE
{
    [StaticConstructorOnStartup]
    public static class VFEE_Patches
    {
        public static RoyalTitleDef? MinorHead;
        static VFEE_Patches()
        {
            Harmony harmonyInstance = new Harmony("hmc.pacas.empire.vfee");
            VFEE_DefOf.VFEE_HighStellarch = MinorHead;
            harmonyInstance.PatchAll();
            /*harmonyInstance.Patch(
                AccessTools.Method(typeof(EmpireTitleUtility), "CanInvite"), 
                new HarmonyMethod(typeof(CanInvitePatch), "Prefix")
                );*/
        }
    }
    
    [HarmonyPatch(typeof(EmpireTitleUtility), nameof(EmpireTitleUtility.CanInvite))]
    public static class CanInvitePatch
    {
        public static bool Prefix(RoyalTitleDef title, ref bool __result)
        {
            __result = title != VFEE_DefOf.Emperor;
            return false;
        }
    }


    /*
    [HarmonyPatch(typeof(WorldComponent_Hierarchy), "MakePawnFor")]
    public static class MakePawnForPatch
    {
        public static void Prefix(RoyalTitleDef title, WorldComponent_Hierarchy __instance)
        {
            Faction ofEmpire = Faction.OfEmpire;
            Log.Warning(string.Format("[VFEE] Title - {0}", title));
            PawnKindDef kindForHierarchy = title.GetModExtension<RoyalTitleDefExtension>().kindForHierarchy;
            Log.Warning(string.Format("[VFEE] PawnKind - {0}", kindForHierarchy));
            PawnGenerationRequest request = new PawnGenerationRequest(
                kindForHierarchy, ofEmpire, forceGenerateNewPawn: true, 
                allowDowned: false, developmentalStages:DevelopmentalStage.Adult
                );
            Log.Warning(string.Format("[VFEE] Request - {0}", request));
            Pawn pawn = PawnGenerator.GeneratePawn(request);
            pawn.health.hediffSet.hediffs.RemoveAll((Predicate<Hediff>)(hediff =>
                !hediff.def.AlwaysAllowMothball && !hediff.IsPermanent() &&
                (!(hediff is Hediff_MissingPart) || hediff.Bleeding) &&
                !hediff.def.allowMothballIfLowPriorityWorldPawn));
            Log.Warning(string.Format("[VFEE] Pawn - {0}", pawn));
            Find.WorldPawns.PassToWorld(pawn, PawnDiscardDecideMode.KeepForever);
            if (!pawn.TryMothball())
                Log.Warning(string.Format("[VFEE] Failed to mothball {0}. This may cause performance issues.", pawn));
            if (pawn.royalty.GetCurrentTitle(ofEmpire) != title)
            {
                Log.Warning(string.Format("[VFEE] Created {0} from title {1} but has title {2}", pawn,
                    title, pawn.royalty.GetCurrentTitle(ofEmpire)));
                pawn.royalty.SetTitle(ofEmpire, title, false, sendLetter: false);
            }

            __instance.TitleHolders.Add(pawn);
        }
    }*/

}