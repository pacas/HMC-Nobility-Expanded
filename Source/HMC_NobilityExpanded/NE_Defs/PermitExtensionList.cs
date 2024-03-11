using System.Collections.Generic;
using JetBrains.Annotations;
using Verse;

namespace NobilityExpanded
{
    public class PermitExtensionList : DefModExtension
    {
        // todo color + nulls
        public string type = "Permit";
        public string category;
        [CanBeNull] public int? column;
        [CanBeNull] public int? row;
        [CanBeNull] public List<ItemDataInfo> data;
        [CanBeNull] public string typeOfDrop;
        [CanBeNull] public string typeOfItem;
        [CanBeNull] public string typeOfQuality;
        [CanBeNull] public string ammoUsage;
        // todo remove
        [CanBeNull] public List<ThingDef> stuffList;
        [CanBeNull] public List<PawnKindDef> pawnToChoose;
    }
}
