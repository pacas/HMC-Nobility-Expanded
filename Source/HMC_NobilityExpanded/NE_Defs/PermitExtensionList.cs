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
        [CanBeNull] public string typeOfDrop;
        [CanBeNull] public string typeOfItem;
        [CanBeNull] public string typeOfQuality;
        [CanBeNull] public string ammoUsage;
        [CanBeNull] public string quality;
        [CanBeNull] public List<ThingDef> stuffList;
        [CanBeNull] public List<ThingDefCountClass> thingsToChoose;
        [CanBeNull] public List<PawnKindDef> pawnToChoose;
        [CanBeNull] public List<int> ammunition;
        [CanBeNull] public List<Gender> genders;
    }
}
