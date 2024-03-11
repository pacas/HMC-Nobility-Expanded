using System;
using System.Collections.Generic;
using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using Verse;

namespace NobilityExpanded
{
    [StaticConstructorOnStartup]
    public class RoyalTitlePermitWorker_DropResourcesSpecific : RoyalTitlePermitWorker_Targeted
    {
        private static readonly Texture2D CommandTex = ContentFinder<Texture2D>.Get("UI/Commands/CallAid");
        private Faction faction;

        public override void OrderForceTarget(LocalTargetInfo target) {
            CallResources(target.Cell);
        }

        public override IEnumerable<FloatMenuOption> GetRoyalAidOptions(
            Map map,
            Pawn pawn,
            Faction faction)
        {
            var workerDropResources = this;
            if (faction.HostileTo(Faction.OfPlayer)) {
                yield return new FloatMenuOption(
                    "CommandCallRoyalAidFactionHostile".Translate(faction.Named("FACTION")), null);
            } else {
                Action action = null;
                string description = workerDropResources.def.LabelCap + ": ";
                bool free;
                if (workerDropResources.FillAidOption(pawn, faction, ref description, out free))
                    action = () => BeginCallResources(pawn, faction, map, free);

                yield return new FloatMenuOption(description, action, faction.def.FactionIcon, faction.Color);
            }
        }

        public override IEnumerable<Gizmo> GetCaravanGizmos(Pawn pawn, Faction faction) {
            var workerDropResources = this;
            string description;
            bool disableNotEnoughFavor;
            if (workerDropResources.FillCaravanAidOption(pawn, faction, out description, out workerDropResources.free, out disableNotEnoughFavor)) {
                var commandAction = new Command_Action();
                commandAction.defaultLabel = workerDropResources.def.LabelCap + " (" + pawn.LabelShort + ")";
                commandAction.defaultDesc = description;
                commandAction.icon = CommandTex;
                commandAction.action = () => {
                    var caravan = pawn.GetCaravan();
                    var massUsage = caravan.MassUsage;
                    var itemsToDrop = def.royalAid.itemsToDrop;
                    for (var index = 0; index < itemsToDrop.Count; ++index)
                        massUsage += itemsToDrop[index].thingDef.BaseMass * itemsToDrop[index].count;
                    if (massUsage > (double)caravan.MassCapacity)
                        Find.WindowStack.Add(Dialog_MessageBox.CreateConfirmation(
                            "DropResourcesOverweightConfirm".Translate(),
                            CallResourcesToCaravan, true));
                    else
                        CallResourcesToCaravan();
                };
                
                if (faction.HostileTo(Faction.OfPlayer))
                    commandAction.Disable(
                        "CommandCallRoyalAidFactionHostile".Translate(faction.Named("FACTION")));
                if (disableNotEnoughFavor)
                    commandAction.Disable("CommandCallRoyalAidNotEnoughFavor".Translate());
                
                yield return commandAction;
            }
        }

        private void BeginCallResources(Pawn caller, Faction faction, Map map, bool free) {
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
            Dialog_ChooseResource.SetData(this, map, caller, faction, def, free);
            Find.WindowStack.Add(window);
        }

        private void CallResources(IntVec3 cell) {
            Dialog_ChooseResource.CallResources(cell);
        }

        private void CallResourcesToCaravan() {
            Dialog_ChooseResource.CallResourcesToCaravan();
        }
    }
}
