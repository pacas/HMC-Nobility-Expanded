using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;

namespace NobilityExpanded.Utilities
{
    public static class VarsExposer
    {
        public static Vector2 permitOptionSpacing = new Vector2(0.25f, 0.35f);
        public static string permitCategory = "PermitCategory_";
        public static string stuffPostfix = "Stuff";
        public static string coordsTable = "CoordsTable";
        public static string spacing = "          ";
        
        public static List<ThingDef> GetPermitStuffList(RoyalTitlePermitDef permit) {
            var stuffDefOrdered = DefDatabase<OrderedStuffDef>.GetNamedSilentFail(permit.defName + "Stuff");
            var extension = permit.GetModExtension<PermitExtensionList>();
            return stuffDefOrdered != null ? stuffDefOrdered.stuffList : extension?.stuffList;
        }
        
        // todo rework
        public static List<PawnKindDef> GetPermitPawnList(RoyalTitlePermitDef permit) {
            var stuffDefOrdered = DefDatabase<OrderedStuffDef>.GetNamedSilentFail(permit.defName + "Stuff");
            var extension = permit.GetModExtension<PermitExtensionList>();
            return stuffDefOrdered != null ? stuffDefOrdered.pawnToChoose : extension?.pawnToChoose;
        }
    }
}
