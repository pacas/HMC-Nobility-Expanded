using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;

namespace NobilityExpanded
{
    public class Dialog_ChooseResource : Window
    {
        private float scrollHeight;
        private Vector2 scrollPosition;
        public ThingDef chosenThing;
        public List<ThingDef> resourceChoices;
        
        public Dialog_ChooseResource(List<ThingDef> resourceChoices)
        {
            this.resourceChoices = resourceChoices;
            chosenThing = resourceChoices.First();
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
            Widgets.Label(0f, ref num, inRect.width, "PickResource".Translate().Resolve() + ":");
            Rect rect = new Rect(inRect.x, num + 15f, inRect.width, inRect.height - 120f);
            rect.yMax -= 4f + CloseButSize.y;
            Text.Font = GameFont.Small;
            num = rect.y;
            Rect rect2 = new Rect(rect.x, rect.y, rect.width - 16f, scrollHeight);
            Widgets.BeginScrollView(rect, ref scrollPosition, rect2);
            DrawResourceChoices(rect2.width, ref num);
            if (Event.current.type == EventType.Layout)
            {
                scrollHeight = Mathf.Max(num - 24f - 15f, rect.height);
            }
            Widgets.EndScrollView();
            Rect rect3 = new Rect(inRect.x, rect.yMax + 15f, inRect.width, 72f);
            TaggedString taggedString = chosenThing.label;
            Widgets.Label(rect3, taggedString);
            Rect rect4 = new Rect(0f, rect3.yMax, inRect.width, CloseButSize.y);
            AcceptanceReport acceptanceReport = CanClose();
            if (Widgets.ButtonText(new Rect(rect4.xMax - CloseButSize.x, rect4.y, CloseButSize.x, CloseButSize.y), "OK".Translate()))
            {
                if (acceptanceReport.Accepted)
                {
                    RoyalTitlePermitWorker_DropResourcesSpecific.chosenThing = chosenThing;
                    Close();
                    return;
                }
                
                Messages.Message(acceptanceReport.Reason, null, MessageTypeDefOf.RejectInput, false);
            }
        }
        
        private void DrawResourceChoices(float width, ref float curY)
        {
            Listing_Standard listingStandard = new Listing_Standard();
            Rect rect = new Rect(0f, curY, 230f, 99999f);
            listingStandard.Begin(rect);
            foreach (ThingDef thing in resourceChoices)
            {
                if (listingStandard.RadioButton(thing.LabelCap, this.chosenThing == thing, 30f, chosenThing.description))
                {
                    chosenThing = thing;
                }
            }
            
            listingStandard.End();
            curY += listingStandard.CurHeight + 10f + 4f;
        }
        
        private bool SelectionsMade()
        {
            return resourceChoices.NullOrEmpty() || chosenThing != null;
        }
        
        private AcceptanceReport CanClose()
        {
            if (!SelectionsMade())
            {
                return "ChooseThisDrop".Translate();
            }
            
            return AcceptanceReport.WasAccepted;
        }
    }
}