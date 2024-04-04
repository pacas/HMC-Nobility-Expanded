using System;
using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;

namespace NobilityExpanded
{
    [StaticConstructorOnStartup]
    public class RoyalTitlePermitWorker_DropBuildings : RoyalTitlePermitWorker_Targeted
    {
        private static readonly Texture2D CommandTex = ContentFinder<Texture2D>.Get("UI/Commands/CallAid");
        private Faction faction;
        private System.Random random = new System.Random();

        public override void OrderForceTarget(LocalTargetInfo target) {
            CallResources(target.Cell);
        }

        public override IEnumerable<FloatMenuOption> GetRoyalAidOptions(Map map, Pawn pawn, Faction faction) {
            var workerDropResources = this;
            if (faction.HostileTo(Faction.OfPlayer))
            {
                yield return new FloatMenuOption(
                    "CommandCallRoyalAidFactionHostile".Translate(faction.Named("FACTION")), 
                    null);
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
            Find.Targeter.BeginTargeting(this);
        }

        private void CallResources(IntVec3 cell) {
            var list = new List<Thing>();
            var extension = def.GetModExtension<PermitExtensionList>();
            if (extension?.itemData == null) {
                Log.Error("Cannot find mod extension");
                return;
            }
            
            int randomIndex = random.Next(extension.itemData.Count);
            ItemDataInfo data = extension.itemData[randomIndex];
            MinifiedThing minifiedBuilding = ThingMaker.MakeThing(data.thing, data.stuff).MakeMinified();
            minifiedBuilding.stackCount = data.count;
            list.Add(minifiedBuilding);
            
            var ammo = Utilities.ItemGenerator.GenerateAmmoForTurrets(data);
            if (ammo != null) {
                list.Add(ammo);
            }

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
    }
}
