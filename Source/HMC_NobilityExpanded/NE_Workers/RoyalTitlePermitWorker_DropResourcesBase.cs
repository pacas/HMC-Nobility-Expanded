using System;
using System.Collections.Generic;
using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using Verse;

namespace NobilityExpanded
{
    public class RoyalTitlePermitWorker_DropResourcesBase : RoyalTitlePermitWorker_Targeted
    {
        protected static readonly Texture2D CommandTex = ContentFinder<Texture2D>.Get("UI/Commands/CallAid");
        protected Faction faction;

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
                            () => CallResourcesToCaravan(pawn, faction, free), true));
                    else
                        CallResourcesToCaravan(pawn, faction, free);
                },
            };

            if (faction.HostileTo(Faction.OfPlayer))
                commandAction.Disable(
                    "CommandCallRoyalAidFactionHostile".Translate(faction.Named("FACTION")));
                
            if (disableNotEnoughFavor)
                commandAction.Disable("CommandCallRoyalAidNotEnoughFavor".Translate());
                
            yield return commandAction;
        }
        
        public virtual void BeginCallResources(Pawn caller, Faction faction, Map map, bool free) {
            targetingParameters = new TargetingParameters {
                canTargetLocations = true,
                canTargetBuildings = false,
                canTargetPawns = false,
            };
            
            this.caller = caller;
            this.map = map;
            this.faction = faction;
            this.free = free;
            targetingParameters.validator = target =>
                (def.royalAid.targetingRange <= 0.0 || target.Cell.DistanceTo(caller.Position) <=
                    (double)def.royalAid.targetingRange) && target.Cell.Walkable(map) && !target.Cell.Fogged(map);
            Find.Targeter.BeginTargeting(this);
        }

        protected virtual void CallResourcesToCaravan(Pawn caller, Faction faction, bool free) {
            
        }
    }
}
