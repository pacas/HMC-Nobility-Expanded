using System.Collections.Generic;
using JetBrains.Annotations;
using Verse;

namespace NobilityExpanded
{
    public class OrderedStuffDef : Def
    {
        [CanBeNull] public string typeOfDrop;
        [CanBeNull] public string typeOfItem;
        [CanBeNull] public string typeOfQuality;
        [CanBeNull] public string ammoUsage;
        [CanBeNull] public string quality;
        [CanBeNull] public List<ThingDef> stuffList;
        [CanBeNull] public List<ThingDef> thingsToChoose;
        [CanBeNull] public List<PawnKindDef> pawnToChoose;
        [CanBeNull] public List<int> ammunition;
        [CanBeNull] public List<Gender> genders;
        public string column;
    }
}
