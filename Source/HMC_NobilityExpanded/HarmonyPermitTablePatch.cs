using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using RimWorld;
using UnityEngine;
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

    [HarmonyPatch(typeof(StatsReportUtility), nameof(StatsReportUtility.Reset))]
    public static class StatsResetPatch
    {
        public static bool Prefix(
            Vector2 ___scrollPosition,
            Vector2 ___scrollPositionRightPanel,
            StatDrawEntry ___selectedEntry,
            ScrollPositioner ___scrollPositioner,
            StatDrawEntry ___mousedOverEntry,
            List<StatDrawEntry> ___cachedDrawEntries,
            List<string> ___cachedEntryValues,
            QuickSearchWidget ___quickSearchWidget
        )
        {
            ___scrollPosition = new Vector2();
            ___scrollPositionRightPanel = new Vector2();
            ___selectedEntry = null;
            ___scrollPositioner.Arm(false);
            ___mousedOverEntry = null;
            ___cachedDrawEntries.Clear();
            ___cachedEntryValues.Clear();
            ___quickSearchWidget.Reset();
            PermitsCardCustomUtility.selectedPermit = null;
            PermitsCardCustomUtility.selectedFaction = !ModLister.RoyaltyInstalled || Current.ProgramState != ProgramState.Playing ? null : Faction.OfEmpire;
            PermitsCardUtility.selectedFaction = !ModLister.RoyaltyInstalled || Current.ProgramState != ProgramState.Playing ? null : Faction.OfEmpire;
            return false;
        }
    }
    
    [HarmonyPatch(typeof(PermitsCardCustomUtility), "DrawPosition")]
    public static class CoordsAutopatch
    {
        public static bool Prefix(ref RoyalTitlePermitDef permit, ref Vector2 __result)
        {
            OrderedStuffDef stuffDefOrdered = DefDatabase<OrderedStuffDef>.GetNamedSilentFail(
                permit.defName + NobilitySupportUtility.StuffPostfix
                );
            int index;
            Vector2 newCoords;
            if (stuffDefOrdered != null)
            {
                RoyaltyCoordsTableDef categoryTable = DefDatabase<RoyaltyCoordsTableDef>.GetNamedSilentFail(
                    NobilitySupportUtility.CoordsTable + 
                    PermitsCardCustomUtility.UtilityClass.curTab + "_" + stuffDefOrdered.column
                    );
                if (categoryTable == null)
                    categoryTable = DefDatabase<RoyaltyCoordsTableDef>.GetNamed(
                        NobilitySupportUtility.CoordsTableColumn + stuffDefOrdered.column
                        );
                
                index = categoryTable.loadOrder.IndexOf(permit);
                newCoords = new Vector2(categoryTable.coordX * 200f, index * 50f);
            }
            else
            {
                RoyaltyCoordsTableDef categoryTable =
                    DefDatabase<RoyaltyCoordsTableDef>.GetNamedSilentFail(
                        NobilitySupportUtility.CoordsTable + PermitsCardCustomUtility.UtilityClass.curTab + "_0"
                        );
                // todo case 
                if (permit.permitPointCost == 99) {
                    index = categoryTable.loadOrder.IndexOf(permit);
                    newCoords = new Vector2(60f, index * 50f + 5f);
                }
                else if (permit.permitPointCost == 98) {
                    index = categoryTable.loadOrder.IndexOf(permit);
                    newCoords = new Vector2(120f, index * 50f + 5f);
                } 
                else if (permit.permitPointCost == 90) {
                    index = categoryTable.loadOrder.IndexOf(permit);
                    newCoords = new Vector2(120f, index * 50f + 5f);
                } else {
                    newCoords = new Vector2(permit.uiPosition.x * 400f, permit.uiPosition.y * 50f);
                }
            }
            __result = newCoords + newCoords * new Vector2(0.25f, 0.35f);
            return false;
        }
    }
    
    [HarmonyPatch(typeof(Dialog_InfoCard), "FillCard")]
    public static class FillCardPatch
    {
        private static readonly MethodInfo VanillaPermit = AccessTools.Method(typeof(PermitsCardUtility), nameof(PermitsCardUtility.DrawRecordsCard));
        private static readonly MethodInfo NobilityPermit = AccessTools.Method(typeof(PermitsCardCustomUtility), nameof(PermitsCardCustomUtility.DrawRecordsCard));
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            foreach(var inst in instructions)
            {
                if(inst.Calls(VanillaPermit))
                {
                    inst.operand = NobilityPermit;
                }
                yield return inst;
            }
        }
    }

    [HarmonyPatch(typeof(Dialog_InfoCard), "get_InitialSize")]
    public static class WindowSizePatch
    {
        public static bool Prefix(ref Vector2 __result) {
            __result = new Vector2(1050f, 880f);
            return false;
        }
    }
}
