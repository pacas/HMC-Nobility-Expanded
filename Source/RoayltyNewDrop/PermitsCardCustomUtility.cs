using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace RimWorld
{
    [StaticConstructorOnStartup]
    public static class PermitsCardCustomUtility
    {
        public static Vector2 rightScrollPosition;
        public static RoyalTitlePermitDef selectedPermit;
        public static Faction selectedFaction;
        public static NobilitySupportUtility utility = new NobilitySupportUtility();
        private static readonly Vector2 PermitOptionSpacing = new Vector2(0.25f, 0.35f);
        private static readonly Texture2D SwitchFactionIcon = ContentFinder<Texture2D>.Get("UI/Icons/SwitchFaction");
        
        private static bool ShowSwitchFactionButton
        {
            get {
                var num = 0;
                foreach (var faction in Find.FactionManager.AllFactionsVisible)
                    if (!faction.IsPlayer && !faction.def.permanentEnemy && !faction.temporary)
                        foreach (var allDef in DefDatabase<RoyalTitlePermitDef>.AllDefs)
                            if (allDef.faction == faction.def) {
                                ++num;
                                break;
                            }
                return num > 1;
            }
        }

        private static int TotalReturnPermitsCost(Pawn pawn)
        {
            var num = 8;
            var allFactionPermits = pawn.royalty.AllFactionPermits;
            for (var index = 0; index < allFactionPermits.Count; ++index)
                if (allFactionPermits[index].OnCooldown && allFactionPermits[index].Permit.royalAid != null)
                    num += allFactionPermits[index].Permit.royalAid.favorCost;

            return num;
        }

        public static void DrawRecordsCard(Rect rect, Pawn pawn)
        {
            if (!ModLister.CheckRoyalty("Permit"))
                return;
            rect.yMax -= 4f;
            if (ShowSwitchFactionButton) {
                var rect1 = new Rect(rect.x, rect.y, 32f, 32f);
                if (Widgets.ButtonImage(rect1, SwitchFactionIcon))
                {
                    var options = new List<FloatMenuOption>();
                    foreach (var faction in Find.FactionManager.AllFactionsVisibleInViewOrder)
                        if (!faction.IsPlayer && !faction.def.permanentEnemy)
                        {
                            var localFaction = faction;
                            options.Add(new FloatMenuOption(localFaction.Name, () =>
                            {
                                selectedFaction = localFaction;
                                selectedPermit = null;
                            }, localFaction.def.FactionIcon, localFaction.Color));
                        }
                    Find.WindowStack.Add(new FloatMenu(options));
                }
                TooltipHandler.TipRegion(rect1, "SwitchFaction_Desc".Translate());
            }
            if (selectedFaction.def.HasRoyalTitles)
            {
                var rect2 = new Rect(rect.xMax - 180f, rect.y - 4f, 180f, 30f);
                var num = TotalReturnPermitsCost(pawn);
                if (Widgets.ButtonText(rect2, "ReturnAllPermits".Translate()))
                {
                    if (!pawn.royalty.PermitsFromFaction(selectedFaction).Any())
                    {
                        Messages.Message("NoPermitsToReturn".Translate(pawn.Named("PAWN")),
                            new LookTargets(pawn), MessageTypeDefOf.RejectInput, false);
                    }
                    else if (pawn.royalty.GetFavor(selectedFaction) < num)
                    {
                        Messages.Message(
                            "NotEnoughFavor".Translate(num.ToString().Named("FAVORCOST"),
                                selectedFaction.def.royalFavorLabel.Named("FAVOR"), pawn.Named("PAWN"),
                                pawn.royalty.GetFavor(selectedFaction).ToString().Named("CURFAVOR")),
                            MessageTypeDefOf.RejectInput);
                    }
                    else
                    {
                        Find.WindowStack.Add(Dialog_MessageBox.CreateConfirmation(
                            (TaggedString)(string)"ReturnAllPermits_Confirm".Translate(
                                8.ToString().Named("BASEFAVORCOST"),
                                num.ToString().Named("FAVORCOST"), selectedFaction.def.royalFavorLabel.Named("FAVOR"),
                                selectedFaction.Named("FACTION")),
                            () => pawn.royalty.RefundPermits(8, selectedFaction), true));
                    }
                }
                TooltipHandler.TipRegion(rect2,
                    "ReturnAllPermits_Desc".Translate(8.ToString().Named("BASEFAVORCOST"), num.ToString().Named("FAVORCOST"),
                        selectedFaction.def.royalFavorLabel.Named("FAVOR")));
            }
            Text.Font = GameFont.Medium;
            var currentTitle = pawn.royalty.GetCurrentTitle(selectedFaction);
            var rect3 = new Rect((float)(rect.xMin + 200.0), rect.y - 4f, rect.xMax - 320f, 30f);
            var label1 = "CurrentTitle".Translate() + ": " +
                (currentTitle != null
                    ? currentTitle.GetLabelFor(pawn).CapitalizeFirst()
                    : (string)"None".Translate()) + "               " + "UnusedPermits".Translate() + ": " +
                pawn.royalty.GetPermitPoints(selectedFaction).ToString();
            if (!selectedFaction.def.royalFavorLabel.NullOrEmpty())
                label1 = label1 + "               " + selectedFaction.def.royalFavorLabel.CapitalizeFirst() + ": " +
                         pawn.royalty.GetFavor(selectedFaction).ToString();
            Widgets.Label(rect3, label1);
            
            var chooseCategoryRect = new Rect(rect.xMin, rect.y - 4f, 180f, 30f);
            Text.Font = GameFont.Small;
            rect.yMin += 35f;
            var leftRect = new Rect(rect);
            rect.xMin += (rect.xMax - rect.xMin - 50f) * 0.3f;
            var rightRect = new Rect(rect);
            leftRect.width *= 0.30f;
            DoLeftRect(leftRect, pawn);
            DoRightRect(rightRect, pawn);
            if (Widgets.ButtonText(chooseCategoryRect, "ChooseCategory".Translate()))
            {
                var tabOptions = SetCategoryButton();
                Find.WindowStack.Add(new FloatMenu(tabOptions));
            }
                
        }

        
        public static void DoLeftRect(Rect rect, Pawn pawn)
        {
            var y1 = 0.0f;
            var currentTitle = pawn.royalty.GetCurrentTitle(selectedFaction);
            var rect1 = new Rect(rect);
            Widgets.BeginGroup(rect1);
            if (selectedPermit != null)
            {
                Text.Font = GameFont.Medium;
                var rect2 = new Rect(0.0f, y1, rect1.width, 0.0f);
                Widgets.LabelCacheHeight(ref rect2, selectedPermit.LabelCap);
                Text.Font = GameFont.Small;
                var y2 = y1 + rect2.height;
                if (!selectedPermit.description.NullOrEmpty()) {
                    var rect3 = new Rect(0.0f, y2, rect1.width, 0.0f);
                    Widgets.LabelCacheHeight(ref rect3, selectedPermit.description);
                    y2 += rect3.height + 16f;
                }
                TaggedString taggedString;
                string tagged;
                var rect4 = new Rect(0.0f, y2, rect1.width, 0.0f);
                string label = "";

                if (selectedPermit.permitPointCost < 90)
                {
                    label += "Cooldown".Translate() + ": " +
                            "PeriodDays".Translate(selectedPermit.cooldownDays);
                    label += "\n" + (string)("PrivilegesPointsRequired"
                        .Translate(selectedPermit.permitPointCost));
                    if (selectedPermit.royalAid != null && selectedPermit.royalAid.favorCost > 0 &&
                        !selectedFaction.def.royalFavorLabel.NullOrEmpty())
                        label += "\n" + (string)("CooldownUseFavorCost"
                            .Translate(selectedFaction.def.royalFavorLabel.Named("HONOR"))
                            .CapitalizeFirst() + ": ") + selectedPermit.royalAid.favorCost.ToString();

                    if (selectedPermit.minTitle != null)
                    {
                        taggedString =
                            "RequiresTitle".Translate((NamedArgument)selectedPermit.minTitle.GetLabelForBothGenders());
                        tagged = taggedString.Resolve()
                            .Colorize(currentTitle == null || currentTitle.seniority < selectedPermit.minTitle.seniority
                                ? ColorLibrary.RedReadable
                                : Color.white);
                        label += "\n" + tagged;
                    }

                    if (selectedPermit.prerequisite != null)
                    {
                        taggedString = "UpgradeFrom".Translate(selectedPermit.prerequisite.LabelCap);
                        tagged = taggedString.Resolve().Colorize(PermitUnlocked(selectedPermit.prerequisite, pawn)
                            ? Color.white
                            : ColorLibrary.RedReadable);
                        label += "\n" + tagged;
                    }

                    OrderedStuffDef stuffDefOrdered =
                        DefDatabase<OrderedStuffDef>.GetNamedSilentFail(selectedPermit.defName + "Stuff");
                    label += "\n\n";
                    bool isThingsExists = stuffDefOrdered.thingsToChoose != null;
                    bool isPawnsExists = stuffDefOrdered.pawnToChoose != null;
                    bool isStuffExists = stuffDefOrdered.stuffList != null;
                    bool isRoyalAidExists = selectedPermit.royalAid != null;

                    if (isThingsExists && stuffDefOrdered.thingsToChoose.Count > 1)
                    {
                        if (!isStuffExists || stuffDefOrdered.stuffList.Count != stuffDefOrdered.thingsToChoose.Count)
                        {
                            label += "ItemIncludedInPermit".Translate() + "\n";
                            for (var index = 0; index < stuffDefOrdered.thingsToChoose.Count; ++index)
                            {
                                label += "  - " + stuffDefOrdered.thingsToChoose[index].LabelCap + "\n";
                            }
                        }
                        else
                        {
                            for (var index = 0; index < stuffDefOrdered.thingsToChoose.Count; ++index)
                            {
                                label += "  - " + "StuffDescription".Translate(
                                    stuffDefOrdered.stuffList[index].stuffProps.stuffAdjective,
                                    stuffDefOrdered.thingsToChoose[index].label) + "\n";
                            }
                        }
                    }
                    else if (isPawnsExists && stuffDefOrdered.pawnToChoose.Count > 1)
                    {
                        label += "ItemIncludedInPermit".Translate() + "\n";
                        for (var index = 0; index < stuffDefOrdered.pawnToChoose.Count; ++index)
                        {
                            label += "  - " + stuffDefOrdered.pawnToChoose[index].LabelCap + "\n";
                        }
                    }
                    else if (isRoyalAidExists &&
                             selectedPermit.royalAid.itemsToDrop != null &&
                             selectedPermit.royalAid.itemsToDrop[0].thingDef.defName != "Steel" &&
                             isStuffExists)
                    {
                        label += "ItemIncludedInPermit".Translate() + "\n";
                        for (var index = 0; index < selectedPermit.royalAid.itemsToDrop.Count; ++index)
                        {
                            label += "  - " + "StuffDescription".Translate(
                                stuffDefOrdered.stuffList[index].stuffProps.stuffAdjective,
                                selectedPermit.royalAid.itemsToDrop[index].Label) + "\n";
                        }
                    }

                    if (isThingsExists && isStuffExists && isRoyalAidExists &&
                        stuffDefOrdered.stuffList.Count == 1 &&
                        selectedPermit.royalAid.itemsToDrop != null &&
                        selectedPermit.royalAid.itemsToDrop[0].thingDef.defName == "Steel")
                    {
                        label += "  - " + "StuffDescription".Translate(
                            stuffDefOrdered.stuffList[0].stuffProps.stuffAdjective,
                            stuffDefOrdered.thingsToChoose[0].label) + "\n";
                    }

                    if (stuffDefOrdered.typeOfQuality != null)
                    {
                        string qualityLabel = stuffDefOrdered.typeOfQuality + stuffDefOrdered.quality;
                        label += qualityLabel.Translate() + "\n";
                    }
                }

                Widgets.LabelCacheHeight(ref rect4, label);
                var rect5 = new Rect(0.0f, rect1.height - 50f, rect1.width, 50f);
                if (selectedPermit.AvailableForPawn(pawn, selectedFaction) && !PermitUnlocked(selectedPermit, pawn) &&
                    Widgets.ButtonText(rect5, "AcceptPermit".Translate()))
                {
                    SoundDefOf.Quest_Accepted.PlayOneShotOnCamera();
                    pawn.royalty.AddPermit(selectedPermit, selectedFaction);
                }
            }
            Widgets.EndGroup();
        }

        
        public static void DoRightRect(Rect rect, Pawn pawn)
        {
            Widgets.DrawMenuSection(rect);
            if (selectedFaction == null)
                return;
            var defsListForReading = DefDatabase<RoyalTitlePermitDef>.AllDefsListForReading;
            var outRect = rect.ContractedBy(10f);
            var rect1 = new Rect();
            for (var index = 0; index < defsListForReading.Count; ++index)
            {
                var permit = defsListForReading[index];
                if (CanDrawPermit(permit))
                {
                    if (permit.permitPointCost == 98)
                        rect1.width = Mathf.Max(rect1.width, (float)(DrawPosition(permit).x + 150.0 + 26.0));
                    else
                        rect1.width = Mathf.Max(rect1.width, (float)(DrawPosition(permit).x + 200.0 + 26.0));
                    rect1.height = Mathf.Max(rect1.height, (float)(DrawPosition(permit).y + 50.0 + 26.0));
                }
            }

            Widgets.BeginScrollView(outRect, ref rightScrollPosition, rect1);
            Widgets.BeginGroup(rect1.ContractedBy(10f));
            DrawLines();
            for (var index = 0; index < defsListForReading.Count; ++index)
            {
                var permit = defsListForReading[index];
                if (CanDrawPermit(permit) && permit.permitPointCost != 90)
                {
                    var vector2 = DrawPosition(permit);
                    var textColor = Widgets.NormalOptionColor;
                    var bgColor = PermitUnlocked(permit, pawn)
                        ? TexUI.FinishedResearchColor
                        : TexUI.AvailResearchColor;
                    Color borderResearchColor;
                    var permitRect = new Rect(vector2.x, vector2.y, 200f, 50f);
                    if (permit.permitPointCost == 99)
                    {
                        textColor = new Color(1f, 0.5f, 0.0f, 1.0f);
                        bgColor = new Color(0.06f, 0.06f, 0.06f, 1);
                        permitRect = new Rect(vector2.x, vector2.y, 300f, 50f);
                    }
                    else if (permit.permitPointCost == 98)
                    {
                        textColor = new Color(1f, 0.8f, 0.2f, 1.0f);
                        bgColor = new Color(0.06f, 0.06f, 0.06f, 1);
                        permitRect = new Rect(vector2.x, vector2.y, 150f, 50f);
                    }
                    else if (!permit.AvailableForPawn(pawn, selectedFaction) && !PermitUnlocked(permit, pawn)) {
                        textColor = Color.red;
                    }
                    if (selectedPermit == permit)
                    {
                        borderResearchColor = TexUI.HighlightBorderResearchColor;
                        bgColor += TexUI.HighlightBgResearchColor;
                    }
                    else
                    {
                        borderResearchColor = TexUI.DefaultBorderResearchColor;
                    }
                    if (Widgets.CustomButtonText(ref permitRect, string.Empty, bgColor, textColor, borderResearchColor))
                    {
                        SoundDefOf.Click.PlayOneShotOnCamera();
                        selectedPermit = permit;
                    }

                    var anchor = (int)Text.Anchor;
                    var color = GUI.color;
                    GUI.color = textColor;
                    Text.Anchor = TextAnchor.MiddleCenter;
                    Widgets.Label(permitRect, permit.LabelCap);
                    GUI.color = color;
                    Text.Anchor = (TextAnchor)anchor;
                }
            }
            Widgets.EndGroup();
            Widgets.EndScrollView();
        }

        
        private static void DrawLines()
        {
            var start = new Vector2();
            var end = new Vector2();
            var defsListForReading = DefDatabase<RoyalTitlePermitDef>.AllDefsListForReading;
            for (var index1 = 0; index1 < 2; ++index1)
            for (var index2 = 0; index2 < defsListForReading.Count; ++index2)
            {
                var permit = defsListForReading[index2];
                if (CanDrawPermit(permit))
                {
                    var vector1 = DrawPosition(permit);
                    start.x = vector1.x;
                    start.y = vector1.y + 25f;
                    var prerequisite = permit.prerequisite;
                    if (prerequisite != null)
                    {
                        var vector2 = DrawPosition(prerequisite);
                        end.x = vector2.x + 200f;
                        end.y = vector2.y + 25f;
                        if ((index1 == 1 && selectedPermit == permit) || selectedPermit == prerequisite)
                            Widgets.DrawLine(start, end, TexUI.HighlightLineResearchColor, 4f);
                        else if (index1 == 0)
                            Widgets.DrawLine(start, end, TexUI.DefaultLineResearchColor, 2f);
                    }
                }
            }
        }

        
        private static bool PermitUnlocked(RoyalTitlePermitDef permit, Pawn pawn)
        {
            if (pawn.royalty.HasPermit(permit, selectedFaction)) return true;

            var allFactionPermits = pawn.royalty.AllFactionPermits;
            for (var index = 0; index < allFactionPermits.Count; ++index)
            {
                if (allFactionPermits[index].Permit.prerequisite == permit &&
                    allFactionPermits[index].Faction == selectedFaction)
                    return true;
                try
                {
                    var secondPrerequisite = allFactionPermits[index].Permit.prerequisite.prerequisite;
                    if (secondPrerequisite == permit &&
                        allFactionPermits[index].Faction == selectedFaction)
                        return true;
                }
                catch (NullReferenceException)
                {
                }
            }

            return false;
        }

        
        private static Vector2 DrawPosition(RoyalTitlePermitDef permit)
        {return PermitOptionSpacing;}
        /* Fake static method, replaced in runtime with real */

        
        private static bool CanDrawPermit(RoyalTitlePermitDef permit)
        {
            if (permit.permitPointCost <= 0)
                return false;
            return permit.faction == null || permit.faction == selectedFaction.def;
        }
        
        
        public static List<FloatMenuOption> SetCategoryButton()
        {
            var tabOptions = new List<FloatMenuOption>();
            var seeds = DefDatabase<RoyalTitlePermitDef>.GetNamedSilentFail("SeedsPermitTitle");
            var dict = new Dictionary<string, Action>
            {
                { "Resources", () => utility.curTab = "Resources" },
                { "Pawns", () => utility.curTab = "Pawns" },
                { "Airstrike", () => utility.curTab = "Airstrike" },
                { "Tools", () => utility.curTab = "Tools" },
                { "Armor", () => utility.curTab = "Armor" },
                { "Apparel", () => utility.curTab = "Apparel" },
                { "Melee", () => utility.curTab = "Melee" },
                { "Ranged", () => utility.curTab = "Ranged" },
                { "Turrets", () => utility.curTab = "Turrets" },
                { "Animals", () => utility.curTab = "Animals" }
            };
            if (seeds != null)
                dict.Add("Seeds", () => utility.curTab = "Seeds");
            foreach (var table in dict)
            {
                tabOptions.Add(new FloatMenuOption(table.Key, table.Value));
            }
            return tabOptions;
        }
    }
}
