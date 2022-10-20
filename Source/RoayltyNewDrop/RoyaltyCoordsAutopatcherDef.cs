using System.Collections.Generic;
using JetBrains.Annotations;
using Verse;

namespace RimWorld
{
    public class RoyaltyCoordsAutopatcherDef : Def
    {
        public int coordY;
        [ItemCanBeNull] public List<RoyalTitlePermitDef> loadOrder;
    }
}