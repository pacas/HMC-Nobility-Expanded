using System.Collections.Generic;
using JetBrains.Annotations;
using Verse;

namespace NobilityExpanded
{
    public class ItemDataInfo
    {
        public ThingDef thing;
        public int count;
        public int ammoCount = 0;
        public string dropType = "Specific";
        [CanBeNull] public ThingDef stuff;
        [CanBeNull] public string quality;
        [CanBeNull] public List<ItemDataInfo> additionalItems;
    }
}
