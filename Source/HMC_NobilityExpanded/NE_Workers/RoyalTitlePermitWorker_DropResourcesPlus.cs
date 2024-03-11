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
    public class RoyalTitlePermitWorker_DropResourcesPlus : RoyalTitlePermitWorker_Targeted
    {
        private static readonly Texture2D CommandTex = ContentFinder<Texture2D>.Get("UI/Commands/CallAid");
        private Faction faction;
        private OrderedStuffDef stuffDefOrdered;
        private System.Random random = new System.Random();

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
                var commandAction = new Command_Action();
                commandAction.defaultLabel = workerDropResources.def.LabelCap + " (" + pawn.LabelShort + ")";
                commandAction.defaultDesc = description;
                commandAction.icon = CommandTex;
                commandAction.action = () =>
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
                if (faction.HostileTo(Faction.OfPlayer))
                    commandAction.Disable(
                        "CommandCallRoyalAidFactionHostile".Translate(faction.Named("FACTION")));
                if (disableNotEnoughFavor)
                    commandAction.Disable("CommandCallRoyalAidNotEnoughFavor".Translate());
                yield return commandAction;
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
            stuffDefOrdered = DefDatabase<OrderedStuffDef>.GetNamed(def.defName + "Stuff");
            List<Thing> things = new List<Thing>();
            Thing thing;
            ThingDef item;
            var randomIndex = 0;
            switch (stuffDefOrdered.typeOfItem)
            {
                case "Specific":
                    item = def.royalAid.itemsToDrop[index].thingDef;
                    break;
                case "Random":
                    randomIndex = random.Next(stuffDefOrdered.thingsToChoose.Count);
                    item = stuffDefOrdered.thingsToChoose[randomIndex];
                    break;
                default:
                    item = def.royalAid.itemsToDrop[index].thingDef;
                    break;
            }
            switch (stuffDefOrdered.typeOfDrop)
            {
                case "Stuff":
                    thing = GenerateThingStuff(index, item);
                    break;
                case "Quality":
                    thing = GenerateThingQuality(item);
                    break;
                case "StuffQuality":
                    thing = GenerateThingStuffQuality(index, item);
                    break;
                case "Pure":
                    thing = ThingMaker.MakeThing(item);
                    break;
                default:
                    thing = GenerateThingStuff(index, item);
                    break;
            }
            thing.stackCount = def.royalAid.itemsToDrop[index].count;
            if (stuffDefOrdered.ammoUsage == "Gun")
            {
                AmmoSetDef ammoUser = thing.TryGetComp<CompAmmoUser>().Props.ammoSet;
                if (!ammoUser.ammoTypes.NullOrEmpty())
                {
                    AmmoDef ammo = ammoUser.ammoTypes[0].ammo;
                    Thing ammoThing = ThingMaker.MakeThing(ammo);
                    ammoThing.stackCount = stuffDefOrdered.ammunition[randomIndex];
                    things.Add(ammoThing);
                }
            }
            things.Add(thing);
            return things;
        }
        private Thing GenerateThingStuffQuality(int index, ThingDef item)
        {
            var stuff = stuffDefOrdered.stuffList[index];
            switch (stuffDefOrdered.typeOfQuality)
            {
                case "Specific":
                    return new ThingStuffPairWithQuality(item, stuff, NE_Utility.GenerateQualityFromString(stuffDefOrdered.quality)).MakeThing();
                case "Range":
                    return new ThingStuffPairWithQuality(item, stuff, NE_Utility.GenerateQualityFromStringRange(stuffDefOrdered.quality)).MakeThing();
                default:
                    return new ThingStuffPairWithQuality(item, stuff, NE_Utility.GenerateQualityFromString(stuffDefOrdered.quality)).MakeThing();
            }
        }
        
        private Thing GenerateThingStuff(int index, ThingDef item)
        {
            var stuff = stuffDefOrdered.stuffList[index];
            return ThingMaker.MakeThing(item, stuff);
        }
        
        private Thing GenerateThingQuality(ThingDef item)
        {
            Thing thing = ThingMaker.MakeThing(item);
            CompQuality comp = thing.TryGetComp<CompQuality>();
            switch (stuffDefOrdered.typeOfQuality)
            {
                case "Specific":
                    comp.SetQuality(NE_Utility.GenerateQualityFromString(stuffDefOrdered.quality), ArtGenerationContext.Outsider);
                    break;
                case "Range":
                    comp.SetQuality(NE_Utility.GenerateQualityFromStringRange(stuffDefOrdered.quality), ArtGenerationContext.Outsider);
                    break;
                default:
                    comp.SetQuality(NE_Utility.GenerateQualityFromString(stuffDefOrdered.quality), ArtGenerationContext.Outsider);
                    break;
            }
            return thing;
        }
    }
}
