using RimWorld;
using Verse;

namespace Rimworld.NE_Utilities
{
    public class CoinsProcessor: Tradeable
    {
        private bool Bugged
        {
            get
            {
                if (this.HasAnyThing)
                    return false;
                Log.ErrorOnce(this.ToString() + " is bugged. There will be no more logs about this.", 162112);
                return true;
            }
        }

        public bool IsCurrencyOrCoins
        {
            get
            {
                if (this.Bugged)
                    return false;

                if (this.ThingDef == ThingDefOf.Silver)
                    return true;

                var tags = this.ThingDef?.tradeTags;
                if (tags != null && tags.Contains("Currency"))
                    return true;

                return false;
                
            }
        }
    }
}
