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
        public static RoyalTitlePermitWorker_DropResourcesSpecific worker;
        public static ThingDefCountClass chosenThing;
        public static Pawn curPawn;
        public static Map curMap;
        public static Faction curFaction;
        public static RoyalTitlePermitDef curDef;
        public static bool isFree;
        public static List<ThingDefCountClass> resourceChoices;
        public static List<Thing> things = new List<Thing>();
        
        public Dialog_ChooseResource()
        {
            this.forcePause = true;
            this.absorbInputAroundWindow = true;
        }
        
        public override Vector2 InitialSize
        {
            get
            {
                return new Vector2(300f, 400f);
            }
        }

        public override void DoWindowContents(Rect inRect)
        {
            float num = 0f;
            Widgets.Label(0f, ref num, inRect.width, "PickResourceForDrop".Translate().Resolve());
            Rect rect = new Rect(inRect.x, num + 15f, inRect.width, inRect.height - 120f);
            rect.yMax -= 4f + CloseButSize.y;
            Text.Font = GameFont.Small;
            num = rect.y;
            Rect rect2 = new Rect(rect.x, rect.y, rect.width - 16f, scrollHeight);
            Widgets.BeginScrollView(rect, ref scrollPosition, rect2);
            DrawResourceChoices(rect2.width, ref num);
            if (Event.current.type == EventType.Layout) {
                scrollHeight = Mathf.Max(num - 24f - 15f, rect.height);
            }

            Widgets.EndScrollView();
            Rect rect3 = new Rect(inRect.x, rect.yMax + 15f, inRect.width, 72f);
            TaggedString taggedString = "ChosenResourceForDropDesc".Translate().Resolve() + chosenThing.thingDef.description;
            Widgets.Label(rect3, taggedString);
            Rect rect4 = new Rect(0f, rect3.yMax, inRect.width, CloseButSize.y);
            AcceptanceReport acceptanceReport = CanClose();
            if (!Widgets.ButtonText(new Rect(rect4.xMax - CloseButSize.x, rect4.y, CloseButSize.x, CloseButSize.y), "OK".Translate())) {
                return;
            }

            if (acceptanceReport.Accepted) {
                GenerateItems();
                Find.Targeter.BeginTargeting(worker);
                Close();
                return;
            }
                
            Messages.Message(acceptanceReport.Reason, null, MessageTypeDefOf.RejectInput, false);
        }
        
        private void DrawResourceChoices(float width, ref float curY)
        {
            Listing_Standard listingStandard = new Listing_Standard();
            Rect rect = new Rect(0f, curY, width - 16f, 99999f);
            listingStandard.Begin(rect);
            foreach (ThingDefCountClass thing in resourceChoices) {
                if (listingStandard.RadioButton(thing.thingDef.LabelCap, chosenThing == thing, 30f, chosenThing.thingDef.description)) {
                    chosenThing = thing;
                }
            }
            
            listingStandard.End();
            curY += listingStandard.CurHeight + 10f + 4f;
        }
        
        private bool SelectionsMade() {
            return resourceChoices.NullOrEmpty() || chosenThing != null;
        }
        
        private AcceptanceReport CanClose() {
            if (!SelectionsMade()) {
                return "ChooseThisDrop".Translate();
            }
            
            return AcceptanceReport.WasAccepted;
        }

        public void SetData(RoyalTitlePermitWorker_DropResourcesSpecific createdWorker, Map map, Pawn caller, Faction faction, RoyalTitlePermitDef def, bool free) {
            //resourceChoices = DefDatabase<OrderedStuffDef>.GetNamed(def.defName + "Stuff").thingsToChoose;
            resourceChoices = def.GetModExtension<PermitExtensionSpecificList>().thingsToChoose;
            chosenThing = resourceChoices.First();
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

        private void GenerateItems() {
            Thing thing = ThingMaker.MakeThing(chosenThing.thingDef);
            thing.stackCount = chosenThing.count;
            things.Add(thing);
        }
    }
}