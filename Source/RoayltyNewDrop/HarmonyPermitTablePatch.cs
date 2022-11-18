using System;
using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using RimWorld.Planet;
using UnityEngine;
using Verse;

namespace RimWorld
{
    [StaticConstructorOnStartup]
    public static partial class HarmonyPatches
    {
        static HarmonyPatches()
        {
            Harmony harmonyInstance = new Harmony("hmc.pacas.empire");

            MethodInfo original = AccessTools.Method(typeof(PermitsCardCustomUtility), "DrawPosition");
            MethodInfo prefix = typeof(CoordsAutopatch).GetMethod("Prefix");
            harmonyInstance.Patch(original, new HarmonyMethod(prefix));
            
            original = AccessTools.Method(typeof(Dialog_InfoCard), "FillCard");
            prefix = typeof(FillCardPatch).GetMethod("Prefix");
            harmonyInstance.Patch(original, new HarmonyMethod(prefix));
            
            original = AccessTools.Method(typeof(StatsReportUtility), "Reset");
            prefix = typeof(StatsResetPatch).GetMethod("Prefix");
            harmonyInstance.Patch(original, new HarmonyMethod(prefix));
            
            original = AccessTools.Method(typeof(Dialog_InfoCard), "get_InitialSize");
            prefix = typeof(WindowSizePatch).GetMethod("Prefix");
            harmonyInstance.Patch(original, new HarmonyMethod(prefix));
        }
    }


    [HarmonyPatch(typeof(StatsReportUtility), "Reset")]
    public class StatsResetPatch
    {
        public static bool Prefix(
            Vector2 ___scrollPosition,
            Vector2 ___scrollPositionRightPanel,
            StatDrawEntry ___selectedEntry,
            ScrollPositioner ___scrollPositioner,
            StatDrawEntry ___mousedOverEntry,
            List<StatDrawEntry> ___cachedDrawEntries,
            QuickSearchWidget ___quickSearchWidget
        )
        {
            ___scrollPosition = new Vector2();
            ___scrollPositionRightPanel = new Vector2();
            ___selectedEntry = null;
            ___scrollPositioner.Arm(false);
            ___mousedOverEntry = null;
            ___cachedDrawEntries.Clear();
            ___quickSearchWidget.Reset();
            PermitsCardCustomUtility.selectedPermit = null;
            PermitsCardCustomUtility.selectedFaction = ModLister.RoyaltyInstalled ? Faction.OfEmpire : null;
            PermitsCardUtility.selectedFaction = ModLister.RoyaltyInstalled ? Faction.OfEmpire : null;
            return false;
        }
    }
    
    [HarmonyPatch(typeof(PermitsCardCustomUtility), "DrawPosition")]
    public class CoordsAutopatch
    {
        public static bool Prefix(ref RoyalTitlePermitDef permit, ref Vector2 __result)
        {
            OrderedStuffDef stuffDefOrdered = DefDatabase<OrderedStuffDef>.GetNamedSilentFail(permit.defName + "Stuff");
            int index;
            Vector2 newCoords;
            if (stuffDefOrdered != null)
            {
                RoyaltyCoordsTableDef categoryTable = DefDatabase<RoyaltyCoordsTableDef>.GetNamedSilentFail(
                    "CoordsTable" + PermitsCardCustomUtility.utility.curTab + "_" + stuffDefOrdered.column);
                if (categoryTable == null)
                    categoryTable = DefDatabase<RoyaltyCoordsTableDef>.GetNamed("CoordsTableColumn_" + stuffDefOrdered.column);
                
                index = categoryTable.loadOrder.IndexOf(permit);
                newCoords = new Vector2(categoryTable.coordX * 200f, index * 50f);
            }
            else
            {
                RoyaltyCoordsTableDef categoryTable =
                    DefDatabase<RoyaltyCoordsTableDef>.GetNamedSilentFail("CoordsTable" + PermitsCardCustomUtility.utility.curTab + "_0");
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
    public class FillCardPatch
    {
        public static bool Prefix(
            Dialog_InfoCard __instance, 
            ref Dialog_InfoCard.InfoCardTab ___tab,
            ref Thing ___thing,
            ref RoyalTitleDef ___titleDef,
            ref Faction ___faction,
            ref WorldObject ___worldObject,
            ref Pawn ___pawn,
            ref Def ___def,
            ref ThingDef ___stuff,
            ref Action ___executeAfterFillCardOnce,
            ref Rect cardRect
        )
        {
            if (___tab == Dialog_InfoCard.InfoCardTab.Stats)
            {
                if (___thing != null)
                {
                    Thing thing1 = ___thing;
                    if (___thing is MinifiedThing thing2)
                        thing1 = thing2.InnerThing;
                    StatsReportUtility.DrawStatsReport(cardRect, thing1);
                }
                else if (___titleDef != null)
                    StatsReportUtility.DrawStatsReport(cardRect, ___titleDef, ___faction, ___pawn);
                else if (___faction != null)
                    StatsReportUtility.DrawStatsReport(cardRect, ___faction);
                else if (___worldObject != null)
                    StatsReportUtility.DrawStatsReport(cardRect, ___worldObject);
                else if (___def is AbilityDef)
                    StatsReportUtility.DrawStatsReport(cardRect, (AbilityDef) ___def);
                else
                    StatsReportUtility.DrawStatsReport(cardRect, ___def, ___stuff);
            }
            else if (___tab == Dialog_InfoCard.InfoCardTab.Character)
                CharacterCardUtility.DrawCharacterCard(cardRect, (Pawn) ___thing);
            else if (___tab == Dialog_InfoCard.InfoCardTab.Health)
            {
                cardRect.yMin += 8f;
                HealthCardUtility.DrawPawnHealthCard(cardRect, (Pawn) ___thing, false, false, null);
            }
            else if (___tab == Dialog_InfoCard.InfoCardTab.Records)
                RecordsCardUtility.DrawRecordsCard(cardRect, (Pawn) ___thing);
            else if (___tab == Dialog_InfoCard.InfoCardTab.Permits)
                PermitsCardCustomUtility.DrawRecordsCard(cardRect, (Pawn) ___thing);
            if (___executeAfterFillCardOnce == null)
                return false;
            ___executeAfterFillCardOnce();
            ___executeAfterFillCardOnce = null;
            return false;
        }
    }

    [HarmonyPatch(typeof(Dialog_InfoCard), "get_InitialSize")]
    public class WindowSizePatch
    {
        public static bool Prefix(ref Vector2 __result) {
            __result = new Vector2(1050f, 870f);
            return false;
        }
    }
}
