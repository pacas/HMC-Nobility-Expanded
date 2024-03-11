using System.Collections.Generic;
using System.Linq;
using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using Verse;

namespace NobilityExpanded
{
    public class Dialog_ChooseResource : Window
    {
        private float scrollHeight;
        private Vector2 scrollPosition;
        private static RoyalTitlePermitWorker_DropResourcesSpecific worker;
        private static ItemDataInfo chosenThing;
        private static Pawn curPawn;
        private static Map curMap;
        private static Faction curFaction;
        private static RoyalTitlePermitDef curDef;
        private static bool isFree;
        private static List<ItemDataInfo> resourceChoices;
        private static List<Thing> things = new List<Thing>();
        
        public Dialog_ChooseResource()
        {
            this.forcePause = true;
            this.absorbInputAroundWindow = true;
        }
        
        public override Vector2 InitialSize
        {
            get {
                return new Vector2(420f, 700f);
            }
        }

        public override void DoWindowContents(Rect inRect)
        {
            float num = 0f;
            Widgets.Label(0f, ref num, inRect.width, "PickResourceForDrop".Translate().Resolve());
            Rect outRect = new Rect(inRect.x, num + 15f, inRect.width + 20f, inRect.height - 210f);
            outRect.yMax -= 4f + CloseButSize.y;
            Text.Font = GameFont.Small;
            num = outRect.y;
            
            #region choices
            Rect choicesFullRect = new Rect(outRect.x, outRect.y, outRect.width - 16f, scrollHeight);
            Widgets.BeginScrollView(outRect, ref scrollPosition, choicesFullRect);
            DrawResourceChoices(choicesFullRect.width, ref num);
            if (Event.current.type == EventType.Layout) {
                scrollHeight = Mathf.Max(num - 24f - 15f, outRect.height);
            }

            Widgets.EndScrollView();
            #endregion

            #region description
            Rect countRect = new Rect(inRect.x, outRect.yMax + 5f, inRect.width, 30f);
            Rect descRect = new Rect(inRect.x, countRect.yMax + 15f, inRect.width, 120f);
            TaggedString taggedStringCount = "ChosenResourceCount".Translate().Resolve() + chosenThing.count;
            TaggedString taggedStringDesc = "ChosenResourceForDropDesc".Translate().Resolve() + "\n" + chosenThing.thing.description;
            Widgets.Label(countRect, taggedStringCount);
            Widgets.Label(descRect, taggedStringDesc);
            #endregion
            
            Rect acceptRect = new Rect(0f, descRect.yMax, inRect.width, CloseButSize.y);
            AcceptanceReport acceptanceReport = CanClose();
            if (!Widgets.ButtonText(new Rect(acceptRect.xMax - CloseButSize.x, acceptRect.y, CloseButSize.x, CloseButSize.y), "OK".Translate())) {
                return;
            }

            if (acceptanceReport.Accepted) {
                things = NE_Utility.GenerateItemByType(chosenThing);
                Find.Targeter.BeginTargeting(worker);
                Close();
                return;
            }
                
            Messages.Message(acceptanceReport.Reason, null, MessageTypeDefOf.RejectInput, false);
        }
        
        private static void DrawResourceChoices(float width, ref float curY)
        {
            Listing_Standard listingStandard = new Listing_Standard();
            Rect choicesRect = new Rect(0f, curY, width - 16f, 99999f);
            listingStandard.Begin(choicesRect);
            foreach (ItemDataInfo data in resourceChoices) {
                if (listingStandard.RadioButton(data.thing.LabelCap, chosenThing == data, 30f, chosenThing.thing.description)) {
                    chosenThing = data;
                }
            }
            
            listingStandard.End();
            curY += listingStandard.CurHeight + 10f + 4f;
        }
        
        private static bool SelectionsMade() {
            return resourceChoices.NullOrEmpty() || chosenThing != null;
        }
        
        private static AcceptanceReport CanClose() {
            return !SelectionsMade() ? "ChooseThisDrop".Translate() : AcceptanceReport.WasAccepted;
        }

        public static void SetData(RoyalTitlePermitWorker_DropResourcesSpecific createdWorker, Map map, Pawn caller, Faction faction, RoyalTitlePermitDef def, bool free) {
            resourceChoices = def.GetModExtension<PermitExtensionList>().data;
            chosenThing = resourceChoices?.First();
            worker = createdWorker;
            curPawn = caller;
            curMap = map;
            curFaction = faction;
            curDef = def;
            isFree = free;
        }

        public static void CallResources(IntVec3 cell) {
            if (!things.Any())
                return;
            var info = new ActiveDropPodInfo();
            info.innerContainer.TryAddRangeOrTransfer(things);
            DropPodUtility.MakeDropPodAt(cell, curMap, info);
            Messages.Message("MessagePermitTransportDrop".Translate(curFaction.Named("FACTION")),
                new LookTargets(cell, curMap), MessageTypeDefOf.NeutralEvent);
            curPawn.royalty.GetPermit(curDef, curFaction).Notify_Used();
            if (isFree)
                return;
            curPawn.royalty.TryRemoveFavor(curFaction, curDef.royalAid.favorCost);
        }

        public static void CallResourcesToCaravan() {
            var caravan = curPawn.GetCaravan();
            foreach (var t in things) {
                CaravanInventoryUtility.GiveThing(caravan, t);
            }
            
            Messages.Message(
                "MessagePermitTransportDropCaravan".Translate(curFaction.Named("FACTION"), curPawn.Named("PAWN")),
                (WorldObject)caravan, MessageTypeDefOf.NeutralEvent);
            curPawn.royalty.GetPermit(curDef, curFaction).Notify_Used();
            if (isFree)
                return;
            curPawn.royalty.TryRemoveFavor(curFaction, curDef.royalAid.favorCost);
        }
    }
}