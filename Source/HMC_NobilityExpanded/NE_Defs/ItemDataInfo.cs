using JetBrains.Annotations;
using Verse;

namespace NobilityExpanded
{
    public class ItemDataInfo
    {
        public ThingDef thing;
        public int count;
        [CanBeNull] public ThingDef stuff;
    }
}
