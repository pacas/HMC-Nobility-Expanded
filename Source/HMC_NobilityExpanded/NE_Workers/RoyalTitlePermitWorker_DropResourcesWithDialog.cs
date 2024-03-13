using System.Collections.Generic;
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
        
        public override IEnumerable<Gizmo> GetCaravanGizmos(Pawn pawn, Faction faction)
        {
            var workerDropResources = this;
            string description;
            bool disableNotEnoughFavor;
            if (!workerDropResources.FillCaravanAidOption(pawn, faction, out description, out workerDropResources.free,
                    out disableNotEnoughFavor))
                yield break;
            
            var commandAction = new Command_Action
            {
                defaultLabel = workerDropResources.def.LabelCap + " (" + pawn.LabelShort + ")",
                defaultDesc = description,
                icon = CommandTex,
                action = () => {
                    var caravan = pawn.GetCaravan();
                    var massUsage = caravan.MassUsage;
                    var extension = def.GetModExtension<PermitExtensionList>();
                    var itemsToDrop = extension.data;
                    foreach (var item in itemsToDrop)
                        massUsage += item.thing.BaseMass * item.count;
                    
                    if (massUsage > (double)caravan.MassCapacity)
                        Find.WindowStack.Add(Dialog_MessageBox.CreateConfirmation(
                            "DropResourcesOverweightConfirm".Translate(),
                            () => CallResourcesToCaravanWithDialog(pawn, faction, free), true));
                    else
                        CallResourcesToCaravanWithDialog(pawn, faction, free);
                },
            };

            if (faction.HostileTo(Faction.OfPlayer))
                commandAction.Disable(
                    "CommandCallRoyalAidFactionHostile".Translate(faction.Named("FACTION")));
                
            if (disableNotEnoughFavor)
                commandAction.Disable("CommandCallRoyalAidNotEnoughFavor".Translate());
                
            yield return commandAction;
        }

        private void CallResources(IntVec3 cell) {
            Dialog_ChooseResource.CallResources(cell);
        }

        private void CallResourcesToCaravanWithDialog(Pawn caller, Faction faction, bool free) {
            var window = new Dialog_ChooseResource();
            Dialog_ChooseResource.SetData(this, map, caller, faction, def, free, false);
            Find.WindowStack.Add(window);
        }

        protected override void CallResourcesToCaravan(Pawn caller, Faction faction, bool free) {
            Dialog_ChooseResource.CallResourcesToCaravan(caller, faction, free);
        }
    }
}
