using HarmonyLib;
using Rimworld.NE_Utilities;
using RimWorld;
using System.Collections.Generic;
using System.Reflection;

namespace NobilityExpanded
{
    [HarmonyPatch(typeof(Tradeable), "InitPriceDataIfNeeded")]
    public static class PriceCoinsCurrencyPatch
    {
        private static readonly MethodInfo VanillaCheck = AccessTools.Method(typeof(Tradeable), "get_IsCurrency");
        private static readonly MethodInfo UpdatedCheck = AccessTools.Method(typeof(CoinsProcessor), "get_IsCurrencyOrCoins");
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            foreach (var inst in instructions)
            {
                if (inst.Calls(VanillaCheck))
                {
                    inst.operand = UpdatedCheck;
                }

                yield return inst;
            }
        }
    }
}
