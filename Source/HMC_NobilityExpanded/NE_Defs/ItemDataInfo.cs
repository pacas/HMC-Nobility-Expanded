using System.Collections.Generic;
using JetBrains.Annotations;
using Verse;

namespace NobilityExpanded
{
    public class ItemDataInfo
    {
        // items inside
        public ThingDef thing;
        public int count;
        [CanBeNull] public List<ItemDataInfo> additionalItems;
        
        // -- additional info --
        // ammoCount > 0 - drop ammo for guns
        public int ammoCount = 0;
        // item made with this material
        [CanBeNull] public ThingDef stuff;
        // quality
        [CanBeNull] public string quality;
        public string qualityType = "Specific";
    }
}
