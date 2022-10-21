using System.Collections.Generic;
using JetBrains.Annotations;
using Verse;

namespace RimWorld
{
    public class RoyaltyCoordsTableDef : Def
    {
        public int coordX;
        [ItemCanBeNull] public List<RoyalTitlePermitDef> loadOrder;
    }
}