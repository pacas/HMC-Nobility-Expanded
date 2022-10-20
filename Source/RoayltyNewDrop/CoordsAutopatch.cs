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
            RoyaltyCoordsAutopatcherDef autopatcher = DefDatabase<RoyaltyCoordsAutopatcherDef>.GetNamed("Autopatcher");
            OrderedStuffDef stuffDefOrdered = DefDatabase<OrderedStuffDef>.GetNamedSilentFail(permit.defName + "Stuff");
            int index;
            int coordX;
            Vector2 newCoords;
            if ((stuffDefOrdered != null) && (stuffDefOrdered.dynamicCoords))
            {
                if (permit.defName.Contains("Plus"))
                {
                    RoyalTitlePermitDef basePermit = DefDatabase<RoyalTitlePermitDef>.GetNamed(permit.defName.Remove(permit.defName.Length-4));
                    coordX = 1;
                    index = autopatcher.loadOrder.IndexOf(basePermit);
                }
                else
                {
                    coordX = 0;
                    index = autopatcher.loadOrder.IndexOf(permit);
                }
                newCoords = new Vector2(coordX * 200f, (autopatcher.coordY + (float)index) * 50f);
                __result = newCoords + newCoords * new Vector2(0.25f, 0.35f);
            }
            else if (permit.defName.Contains("PermitTitle"))
            {
                index = autopatcher.loadOrder.IndexOf(permit);
                newCoords = new Vector2(100f, (autopatcher.coordY + (float)index) * 50f);
                __result = newCoords + newCoords * new Vector2(0.25f, 0.35f);
            }
            else if (permit.defName.Contains("PermitVanillaTitle"))
            {
                Vector2 vector2 = new Vector2(permit.uiPosition.x * 100f, permit.uiPosition.y * 50f);
                __result = vector2 + vector2 * new Vector2(0.25f, 0.35f);
            }
            else
            {
                Vector2 vector2 = new Vector2(permit.uiPosition.x * 200f, permit.uiPosition.y * 50f);
                __result = vector2 + vector2 * new Vector2(0.25f, 0.35f);
            }
            return false;
        }
    }
}