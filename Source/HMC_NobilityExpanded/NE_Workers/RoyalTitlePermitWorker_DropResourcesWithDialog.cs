using RimWorld;
using RimWorld.Planet;
using Verse;

namespace NobilityExpanded
{
    [StaticConstructorOnStartup]
    public class RoyalTitlePermitWorker_DropResourcesWithDialog : RoyalTitlePermitWorker_DropResourcesBase
    {
        public override void OrderForceTarget(LocalTargetInfo target) {
            CallResources(target.Cell);
        }
        
        public override void BeginCallResources(Pawn caller, Faction faction, Map map, bool free) {
            targetingParameters = new TargetingParameters();
            targetingParameters.canTargetLocations = true;
            targetingParameters.canTargetBuildings = false;
            targetingParameters.canTargetPawns = false;
            this.caller = caller;
            this.map = map;
            this.faction = faction;
            this.free = free;
            targetingParameters.validator = target =>
                (def.royalAid.targetingRange <= 0.0 || target.Cell.DistanceTo(caller.Position) <=
                    (double)def.royalAid.targetingRange) && target.Cell.Walkable(map) && !target.Cell.Fogged(map);
            var window = new Dialog_ChooseResource();
            Dialog_ChooseResource.SetData(this, map, caller, faction, def, free, true);
            Find.WindowStack.Add(window);
        }
        
        private void CallResources(IntVec3 cell) {
            Dialog_ChooseResource.CallResources(cell);
        }

        protected override void BeginCallResourcesToCaravan(Pawn caller, Faction faction, bool free) {
            var window = new Dialog_ChooseResource();
            Dialog_ChooseResource.SetData(this, map, caller, faction, def, free, false);
            Find.WindowStack.Add(window);
        }
    }
}
