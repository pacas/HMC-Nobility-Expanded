using System.Collections.Generic;
using RimWorld;
using RimWorld.Planet;
using Verse;

namespace NobilityExpanded
{
    [StaticConstructorOnStartup]
    public class RoyalTitlePermitWorker_DropResourcesUpdated : RoyalTitlePermitWorker_DropResourcesBase
    {
        public override void OrderForceTarget(LocalTargetInfo target) {
            CallResources(target.Cell);
        }

        private void CallResources(IntVec3 cell) {
            var extension = def.GetModExtension<PermitExtensionList>();
            var list = Utilities.ItemGenerator.GenerateItems(extension);
            if (!list.Any()) {
                Log.Error("Empty drop list");
                return;
            }
            
            var info = new ActiveDropPodInfo();
            info.innerContainer.TryAddRangeOrTransfer(list);
            DropPodUtility.MakeDropPodAt(cell, map, info);
            Messages.Message("MessagePermitTransportDrop".Translate(faction.Named("FACTION")),
                new LookTargets(cell, map), MessageTypeDefOf.NeutralEvent);
            caller.royalty.GetPermit(def, faction).Notify_Used();
            if (free)
                return;
            
            caller.royalty.TryRemoveFavor(faction, def.royalAid.favorCost);
        }

        protected override void CallResourcesToCaravan(Pawn caller, Faction faction, bool free) {
            var caravan = caller.GetCaravan();
            var extension = def.GetModExtension<PermitExtensionList>();
            var list = Utilities.ItemGenerator.GenerateItems(extension);
            if (!list.Any()) {
                Log.Error("Empty drop list");
                return;
            }

            foreach (var thing in list) {
                CaravanInventoryUtility.GiveThing(caravan, thing);
            }
            
            Messages.Message(
                "MessagePermitTransportDropCaravan".Translate(faction.Named("FACTION"), caller.Named("PAWN")),
                (WorldObject)caravan, MessageTypeDefOf.NeutralEvent);
            caller.royalty.GetPermit(def, faction).Notify_Used();
            if (free)
                return;
            caller.royalty.TryRemoveFavor(faction, def.royalAid.favorCost);
        }
    }
}
