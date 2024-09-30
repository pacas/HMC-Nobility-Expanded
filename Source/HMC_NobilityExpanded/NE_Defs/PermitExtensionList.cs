using System.Collections.Generic;
using JetBrains.Annotations;
using Verse;

namespace NobilityExpanded
{
    public class PermitExtensionList : DefModExtension
    {
        public string type = "Permit";
        public string category;
        [CanBeNull] public int? column;
        [CanBeNull] public int? row;

        public bool randomItem = false;
        public bool chooseTag = false;
        [CanBeNull] public List<ItemDataInfo> itemData;
        [CanBeNull] public List<PawnDataInfo> pawnData;
    }
}
