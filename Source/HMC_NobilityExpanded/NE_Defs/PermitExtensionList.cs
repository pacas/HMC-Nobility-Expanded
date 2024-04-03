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
        
        [CanBeNull] public List<ItemDataInfo> itemData;
        [CanBeNull] public List<PawnDataInfo> pawnData;
        
        // todo remove
        [CanBeNull] public List<ThingDef> stuffList;
        [CanBeNull] public List<PawnKindDef> pawnToChoose;
    }
}
