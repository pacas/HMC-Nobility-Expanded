using System.Collections.Generic;
using Verse;

namespace NobilityExpanded
{
    public class OrderedStuffDef : Def
    {
        public string typeOfDrop;
        public string typeOfItem;
        public string typeOfQuality;
        public string ammoUsage;
        public string quality;
        public List<ThingDef> stuffList;
        public List<ThingDef> thingsToChoose;
        public List<PawnKindDef> pawnToChoose;
        public List<int> ammunition;
        public string column;
    }
}
