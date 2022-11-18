using System;
using System.Collections.Generic;
using Verse;

namespace RimWorld
{
    public class RoyalTitlePermitWorker_CallPawns : RoyalTitlePermitWorker_Targeted
    {
        private System.Random random = new System.Random();

        public override IEnumerable<FloatMenuOption> GetRoyalAidOptions(
            Map map,
            Pawn pawn,
            Faction faction)
        {
            RoyalTitlePermitWorker_CallPawns permitWorkerCallAid = this;
            string reason;
            if (permitWorkerCallAid.AidDisabled(map, pawn, faction, out reason))
                yield return new FloatMenuOption((permitWorkerCallAid.def.LabelCap + ": " + reason), null);
            else if (NeutralGroupIncidentUtility.AnyBlockingHostileLord(pawn.MapHeld, faction))
            {
                yield return new FloatMenuOption(
                    (permitWorkerCallAid.def.LabelCap + ": " + "HostileVisitorsPresent".Translate()), null);
            }
            else
            {
                Action action = null;
                string description = (permitWorkerCallAid.def.LabelCap + ": ");
                bool free;
                if (permitWorkerCallAid.FillAidOption(pawn, faction, ref description, out free))
                    action = (() => BeginCallAid(pawn, map, free));
                yield return new FloatMenuOption(description, action, faction.def.FactionIcon, faction.Color);
            }
        }

        private void BeginCallAid(
            Pawn caller,
            Map map,
            bool free)
        {
            targetingParameters = new TargetingParameters();
            targetingParameters.canTargetLocations = true;
            targetingParameters.canTargetSelf = false;
            targetingParameters.canTargetPawns = false;
            targetingParameters.canTargetFires = false;
            targetingParameters.canTargetBuildings = false;
            targetingParameters.canTargetItems = false;
            targetingParameters.validator = (target =>
                (def.royalAid.targetingRange <= 0.0 ||
                 target.Cell.DistanceTo(caller.Position) <= (double)def.royalAid.targetingRange) &&
                !target.Cell.Fogged(map) && DropCellFinder.CanPhysicallyDropInto(target.Cell, map, true) &&
                target.Cell.GetEdifice(map) == null && !target.Cell.Impassable(map));
            this.caller = caller;
            this.map = map;
            this.free = free;
            Find.Targeter.BeginTargeting(this);
        }

        public override void OrderForceTarget(LocalTargetInfo target) =>
            CallAid(caller, map, target.Cell, free);

        private void CallAid(
            Pawn caller,
            Map map,
            IntVec3 spawnPos,
            bool free)
        {
            OrderedStuffDef stuff = DefDatabase<OrderedStuffDef>.GetNamed(def.defName + "Stuff");
            int randomIndex = random.Next(stuff.pawnToChoose.Count);

            for (var index = 0; index < def.royalAid.pawnCount; ++index)
            {
                Pawn pawn = PawnGenerator.GeneratePawn(stuff.pawnToChoose[randomIndex], Faction.OfPlayer);
                TradeUtility.SpawnDropPod(spawnPos + new IntVec3(index, 0, 0), map, pawn);
            }
            if (!free)
                caller.royalty.TryRemoveFavor(Faction.OfEmpire, def.royalAid.favorCost);
            caller.royalty.GetPermit(def, Faction.OfEmpire).Notify_Used();
            Messages.Message("MessagePermitTransportDrop".Translate(Faction.OfEmpire.Named("FACTION")),
                new LookTargets(spawnPos, map), MessageTypeDefOf.NeutralEvent);
        }
    }
}
