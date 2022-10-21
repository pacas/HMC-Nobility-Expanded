using System.Reflection;
using HarmonyLib;
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
            MethodInfo original = AccessTools.Method(typeof(PermitsCardUtility), "DrawPosition");
            MethodInfo prefix = typeof(CoordsAutopatch).GetMethod("Prefix");
            harmonyInstance.Patch(original, new HarmonyMethod(prefix));
        }
    }
    
    [HarmonyPatch(typeof(PermitsCardUtility), "DrawPosition")]
    public class CoordsAutopatch
    {
        public static bool Prefix(ref RoyalTitlePermitDef permit, ref Vector2 __result)
        {
            OrderedStuffDef stuffDefOrdered = DefDatabase<OrderedStuffDef>.GetNamedSilentFail(permit.defName + "Stuff");
            int index;
            Vector2 newCoords;
            if (stuffDefOrdered != null)
            {
                RoyaltyCoordsTableDef autopatcher = DefDatabase<RoyaltyCoordsTableDef>.GetNamed("CoordsTableColumn_" + stuffDefOrdered.column);
                index = autopatcher.loadOrder.IndexOf(permit);
                newCoords = new Vector2(autopatcher.coordX * 200f, index * 50f);
            }
            else if (permit.defName.Contains("PermitTitle"))
            {
                RoyaltyCoordsTableDef autopatcher = DefDatabase<RoyaltyCoordsTableDef>.GetNamed("CoordsTableColumn_0");
                index = autopatcher.loadOrder.IndexOf(permit);
                newCoords = new Vector2(100f, index * 50f);
            }
            else
            {
                newCoords = new Vector2(permit.uiPosition.x * 400f, permit.uiPosition.y * 50f);
            }
            __result = newCoords + newCoords * new Vector2(0.25f, 0.35f);
            return false;
        }
    }
}