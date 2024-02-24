using System;
using System.Collections.Generic;
using CombatExtended;
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
        private OrderedStuffDef stuffDefOrdered;
        private List<ThingDef> resourceChoices;
        public static ThingDef chosenThing;

        public override void OrderForceTarget(LocalTargetInfo target)
        {
            CallResources(target.Cell);
        }

        public override IEnumerable<FloatMenuOption> GetRoyalAidOptions(
            Map map,
            Pawn pawn,
            Faction faction)
        {
            var workerDropResources = this;
            if (faction.HostileTo(Faction.OfPlayer))
            {
                yield return new FloatMenuOption(
                    "CommandCallRoyalAidFactionHostile".Translate(faction.Named("FACTION")), null);
            }
            else
            {
                Action action = null;
                string description = workerDropResources.def.LabelCap + ": ";
                bool free;
                if (workerDropResources.FillAidOption(pawn, faction, ref description, out free))
                    action = () => BeginCallResources(pawn, faction, map, free);

                yield return new FloatMenuOption(description, action, faction.def.FactionIcon, faction.Color);
            }
        }

        public override IEnumerable<Gizmo> GetCaravanGizmos(Pawn pawn, Faction faction)
        {
            var workerDropResources = this;
            string description;
            bool disableNotEnoughFavor;
            if (workerDropResources.FillCaravanAidOption(pawn, faction, out description, out workerDropResources.free,
                    out disableNotEnoughFavor))
            {
                var commandAction1 = new Command_Action();
                commandAction1.defaultLabel = workerDropResources.def.LabelCap + " (" + pawn.LabelShort + ")";
                commandAction1.defaultDesc = description;
                commandAction1.icon = CommandTex;
                commandAction1.action = () =>
                {
                    var caravan = pawn.GetCaravan();
                    var massUsage = caravan.MassUsage;
                    var itemsToDrop = def.royalAid.itemsToDrop;
                    for (var index = 0; index < itemsToDrop.Count; ++index)
                        massUsage += itemsToDrop[index].thingDef.BaseMass * itemsToDrop[index].count;
                    if (massUsage > (double)caravan.MassCapacity)
                        Find.WindowStack.Add(Dialog_MessageBox.CreateConfirmation(
                            "DropResourcesOverweightConfirm".Translate(),
                            () => CallResourcesToCaravan(pawn, faction, free), true));
                    else
                        CallResourcesToCaravan(pawn, faction, free);
                };
                var commandAction2 = commandAction1;
                if (faction.HostileTo(Faction.OfPlayer))
                    commandAction2.Disable(
                        "CommandCallRoyalAidFactionHostile".Translate(faction.Named("FACTION")));
                if (disableNotEnoughFavor)
                    commandAction2.Disable("CommandCallRoyalAidNotEnoughFavor".Translate());
                yield return commandAction2;
            }
        }

        private void BeginCallResources(Pawn caller, Faction faction, Map map, bool free)
        {
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
            Find.Targeter.BeginTargeting(this);
        }

        private void CallResources(IntVec3 cell)
        {
            var list = new List<Thing>();
            for (var index = 0; index < def.royalAid.itemsToDrop.Count; ++index)
            {
                List<Thing> things = GenerateItems(index);
                for (var i = 0; i < things.Count; ++i)
                {
                    list.Add(things[i]);
                }
            }
            if (!list.Any())
                return;
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

        private void CallResourcesToCaravan(Pawn caller, Faction faction, bool free)
        {
            var caravan = caller.GetCaravan();
            for (var index = 0; index < def.royalAid.itemsToDrop.Count; ++index)
            {
                List<Thing> things = GenerateItems(index);
                for (var i = 0; i < things.Count; ++i)
                {
                    CaravanInventoryUtility.GiveThing(caravan, things[i]);
                }
            }
            Messages.Message(
                "MessagePermitTransportDropCaravan".Translate(faction.Named("FACTION"), caller.Named("PAWN")),
                (WorldObject)caravan, MessageTypeDefOf.NeutralEvent);
            caller.royalty.GetPermit(def, faction).Notify_Used();
            if (free)
                return;
            caller.royalty.TryRemoveFavor(faction, def.royalAid.favorCost);
        }

        private List<Thing> GenerateItems(int index)
        {
            resourceChoices = DefDatabase<OrderedStuffDef>.GetNamed(def.defName + "Stuff").stuffList;
            Find.WindowStack.Add(new Dialog_ChooseResource(resourceChoices));
            List<Thing> things = new List<Thing>();
            Thing thing = ThingMaker.MakeThing(chosenThing);
            thing.stackCount = def.royalAid.itemsToDrop[index].count;
            things.Add(thing);
            return things;
        }
    }
}
