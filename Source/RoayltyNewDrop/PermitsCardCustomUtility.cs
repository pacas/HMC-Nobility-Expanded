using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace NobilityExpanded
{
    [StaticConstructorOnStartup]
    public static class PermitsCardCustomUtility
    {
        private static Vector2 rightScrollPosition;
        public static RoyalTitlePermitDef selectedPermit;
        public static Faction selectedFaction;
        public static readonly NobilitySupportUtility UtilityClass = new NobilitySupportUtility();
        private static readonly Vector2 PermitOptionSpacing = new Vector2(0.25f, 0.35f);
        private static readonly Texture2D SwitchFactionIcon = ContentFinder<Texture2D>.Get("UI/Icons/SwitchFaction");
        
        private static bool ShowSwitchFactionButton
        {
            get {
                var num = 0;
                foreach (var faction in Find.FactionManager.AllFactionsVisible)
                    if (!faction.IsPlayer && !faction.def.permanentEnemy && !faction.temporary)
                        if (DefDatabase<RoyalTitlePermitDef>.AllDefs.Any(allDef => allDef.faction == faction.def)) {
                            ++num;
                        }
                return num > 1;
            }
        }

        private static int TotalReturnPermitsCost(Pawn pawn)
        {
            var returnCost = 8;
            var allFactionPermits = pawn.royalty.AllFactionPermits;
            foreach (var t in allFactionPermits)
                if (t.OnCooldown && t.Permit.royalAid != null)
                    returnCost += t.Permit.royalAid.favorCost;

            return returnCost;
        }

        public static void DrawRecordsCard(Rect rect, Pawn pawn)
        {
            if (!ModLister.CheckRoyalty("Permit"))
                return;
            rect.yMax -= 4f;
            if (ShowSwitchFactionButton) {
                var switchButton = new Rect(rect.x, rect.y, 32f, 32f);
                if (Widgets.ButtonImage(switchButton, SwitchFactionIcon))
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
                TooltipHandler.TipRegion(switchButton, "SwitchFaction_Desc".Translate());
            }
            if (selectedFaction.def.HasRoyalTitles)
            {
                var returnPointsButton = new Rect(rect.xMax - 180f, rect.y - 4f, 180f, 30f);
                var returnCost = TotalReturnPermitsCost(pawn);
                if (Widgets.ButtonText(returnPointsButton, "ReturnAllPermits".Translate()))
                {
                    if (!pawn.royalty.PermitsFromFaction(selectedFaction).Any())
                    {
                        Messages.Message("NoPermitsToReturn".Translate(pawn.Named("PAWN")),
                            new LookTargets(pawn), MessageTypeDefOf.RejectInput, false);
                    }
                    else if (pawn.royalty.GetFavor(selectedFaction) < returnCost)
                    {
                        Messages.Message(
                            "NotEnoughFavor".Translate(returnCost.ToString().Named("FAVORCOST"),
                                selectedFaction.def.royalFavorLabel.Named("FAVOR"), pawn.Named("PAWN"),
                                pawn.royalty.GetFavor(selectedFaction).ToString().Named("CURFAVOR")),
                            MessageTypeDefOf.RejectInput);
                    }
                    else
                    {
                        Find.WindowStack.Add(Dialog_MessageBox.CreateConfirmation(
                            (TaggedString)(string)"ReturnAllPermits_Confirm".Translate(
                                8.ToString().Named("BASEFAVORCOST"),
                                returnCost.ToString().Named("FAVORCOST"), selectedFaction.def.royalFavorLabel.Named("FAVOR"),
                                selectedFaction.Named("FACTION")),
                            () => pawn.royalty.RefundPermits(8, selectedFaction), true));
                    }
                }
                TooltipHandler.TipRegion(returnPointsButton,
                    "ReturnAllPermits_Desc".Translate(8.ToString().Named("BASEFAVORCOST"), returnCost.ToString().Named("FAVORCOST"),
                        selectedFaction.def.royalFavorLabel.Named("FAVOR")));
            }
            Text.Font = GameFont.Medium;
            var currentTitle = pawn.royalty.GetCurrentTitle(selectedFaction);
            var statusLabelRect = new Rect(rect.xMin + 200f, rect.y - 2f, rect.xMax - 320f, 30f);
            var statusLabelText = "CurrentTitle".Translate() + ": " +
                (currentTitle != null
                    ? currentTitle.GetLabelFor(pawn).CapitalizeFirst()
                    : (string)"None".Translate()) + "            " + "UnusedPermits".Translate() + ": " +
                pawn.royalty.GetPermitPoints(selectedFaction).ToString();
            if (!selectedFaction.def.royalFavorLabel.NullOrEmpty())
                statusLabelText +=  "            " + selectedFaction.def.royalFavorLabel.CapitalizeFirst() + ": " +
                                  pawn.royalty.GetFavor(selectedFaction).ToString();
            Widgets.Label(statusLabelRect, statusLabelText);
            
            var chooseCategoryRect = new Rect(rect.xMin, rect.y - 4f, 180f, 30f);
            Text.Font = GameFont.Small;
            rect.yMin += 45f;
            var leftRect = new Rect(rect) {
                xMin = 15f,
                xMax = 260f
            };
            var middleRect = new Rect(rect) {
                xMin = 270f,
                xMax = 790f
            };
            var rightRect = new Rect(rect) {
                xMin = 810f,
                xMax = 1030f
            };
            DoLeftRect(leftRect, pawn);
            DoMiddleRect(middleRect, pawn);
            DoRightRect(rightRect, pawn);
            if (Widgets.ButtonText(chooseCategoryRect, "ChoosePermitCategory".Translate())) {
                var tabOptions = SetCategoryButton();
                Find.WindowStack.Add(new FloatMenu(tabOptions));
            }
        }

        
        private static void DoLeftRect(Rect rect, Pawn pawn)
        {
            var leftPanel = new Rect(rect);
            Widgets.BeginGroup(leftPanel);
            if (selectedPermit != null)
            {
                Text.Font = GameFont.Medium;
                var titleRect = new Rect(0.0f, 0.0f, leftPanel.width, 0.0f);
                Widgets.LabelCacheHeight(ref titleRect, selectedPermit.LabelCap);
                Text.Font = GameFont.Small;
                var yPos = titleRect.height;
                if (!selectedPermit.description.NullOrEmpty()) {
                    var descRect = new Rect(0.0f, yPos + 10f, leftPanel.width, 0.0f);
                    Widgets.LabelCacheHeight(ref descRect, selectedPermit.description);
                    yPos += descRect.height + 26f;
                }
                if (selectedPermit.permitPointCost < 90) {
                    var commentRect = new Rect(0.0f, yPos, leftPanel.width, 0.0f);
                    var dropComment = "DropComment_" + selectedPermit.defName;
                    string label = "\n" + "DropComment".Translate() + "\n\n" + dropComment.Translate();
                    Widgets.LabelCacheHeight(ref commentRect, label);
                }
                var acceptRect = new Rect(0.0f, leftPanel.height - 50f, leftPanel.width, 50f);
                if (selectedPermit.AvailableForPawn(pawn, selectedFaction) && !PermitUnlocked(selectedPermit, pawn) &&
                    Widgets.ButtonText(acceptRect, "AcceptPermit".Translate()))
                {
                    SoundDefOf.Quest_Accepted.PlayOneShotOnCamera();
                    pawn.royalty.AddPermit(selectedPermit, selectedFaction);
                }
            }
            Widgets.EndGroup();
        }

        
        private static void DoMiddleRect(Rect rect, Pawn pawn)
        {
            Widgets.DrawMenuSection(rect);
            if (selectedFaction == null)
                return;
            var defsListForReading = DefDatabase<RoyalTitlePermitDef>.AllDefsListForReading;
            var outRect = rect.ContractedBy(10f);
            var middlePanel = new Rect();
            foreach (var permit in defsListForReading.Where(CanDrawPermit))
            {
                middlePanel.width = permit.permitPointCost == 98 ? 
                    Mathf.Max(middlePanel.width, (float)(DrawPosition(permit).x + 150.0 + 26.0)) : 
                    Mathf.Max(middlePanel.width, (float)(DrawPosition(permit).x + 200.0 + 26.0));
                middlePanel.height = Mathf.Max(middlePanel.height, (float)(DrawPosition(permit).y + 50.0 + 26.0));
            }

            Widgets.BeginScrollView(outRect, ref rightScrollPosition, middlePanel);
            Widgets.BeginGroup(middlePanel.ContractedBy(10f));
            DrawLines();
            foreach (var permit in defsListForReading)
            {
                if (CanDrawPermit(permit) && permit.permitPointCost != 90)
                {
                    var vector2 = DrawPosition(permit);
                    var textColor = Widgets.NormalOptionColor;
                    var bgColor = PermitUnlocked(permit, pawn)
                        ? TexUI.FinishedResearchColor
                        : TexUI.AvailResearchColor;
                    Color borderResearchColor;
                    var permitRect = new Rect(vector2.x, vector2.y, 200f, 50f);
                    if (permit.permitPointCost == 99) {
                        textColor = new Color(1f, 0.5f, 0.0f, 1.0f);
                        bgColor = new Color(0.06f, 0.06f, 0.06f, 1);
                        permitRect = new Rect(vector2.x, vector2.y, 300f, 50f);
                    }
                    else if (permit.permitPointCost == 98) {
                        textColor = new Color(1f, 0.8f, 0.2f, 1.0f);
                        bgColor = new Color(0.06f, 0.06f, 0.06f, 1);
                        permitRect = new Rect(vector2.x - 25f, vector2.y, 200f, 50f);
                    }
                    else if (!permit.AvailableForPawn(pawn, selectedFaction) && !PermitUnlocked(permit, pawn)) {
                        textColor = new Color(0.6f, 0.1f, 0.1f, 1.0f);
                    }
                    if (selectedPermit == permit) {
                        borderResearchColor = TexUI.HighlightBorderResearchColor;
                        bgColor += TexUI.HighlightBgResearchColor;
                    }
                    else {
                        borderResearchColor = TexUI.DefaultBorderResearchColor;
                    }
                    if (Widgets.CustomButtonText(ref permitRect, string.Empty, bgColor, textColor, borderResearchColor)) {
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
        
        private static void DoRightRect(Rect rect, Pawn pawn)
        {
            var currentTitle = pawn.royalty.GetCurrentTitle(selectedFaction);
            var rect1 = new Rect(rect);
            Widgets.BeginGroup(rect1);
            if (selectedPermit != null)
            {
                var rect4 = new Rect(0.0f, 0.0f, rect1.width, 0.0f);
                var label = "";

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

                    TaggedString taggedString;
                    string tagged;
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

                    var stuffDefOrdered =
                        DefDatabase<OrderedStuffDef>.GetNamedSilentFail(selectedPermit.defName + "Stuff");
                    label += "\n\n";
                    var isThingsExists = stuffDefOrdered.thingsToChoose != null;
                    var isPawnsExists = stuffDefOrdered.pawnToChoose != null;
                    var isStuffExists = stuffDefOrdered.stuffList != null;
                    var isRoyalAidExists = selectedPermit.royalAid != null;

                    if (isThingsExists && stuffDefOrdered.thingsToChoose.Count > 1)
                    {
                        if (!isStuffExists || stuffDefOrdered.stuffList.Count != stuffDefOrdered.thingsToChoose.Count)
                        {
                            label += "ItemIncludedInPermit".Translate() + "\n";
                            foreach (var t in stuffDefOrdered.thingsToChoose) {
                                label += "  - " + t.LabelCap + "\n";
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
                        foreach (var t in stuffDefOrdered.pawnToChoose)
                        {
                            label += "  - " + t.LabelCap + "\n";
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
                        if (stuffDefOrdered.thingsToChoose.Count != 1) {
                            label += "StuffDescriptionSingle".Translate(
                                stuffDefOrdered.stuffList[0].LabelCap) + "\n";
                        }
                        else
                        {
                            label += "  - " + "StuffDescription".Translate(
                            stuffDefOrdered.stuffList[0].stuffProps.stuffAdjective,
                            stuffDefOrdered.thingsToChoose[0].label) + "\n";
                        }
                    }

                    if (stuffDefOrdered.typeOfQuality != null)
                    {
                        string qualityLabel = stuffDefOrdered.typeOfQuality + stuffDefOrdered.quality;
                        label += qualityLabel.Translate() + "\n";
                    }
                }
                Widgets.LabelCacheHeight(ref rect4, label);
            }
            Widgets.EndGroup();
        }

        
        private static void DrawLines()
        {
            var start = new Vector2();
            var end = new Vector2();
            var defsListForReading = DefDatabase<RoyalTitlePermitDef>.AllDefsListForReading;
            for (var columnIndex = 0; columnIndex < 2; ++columnIndex)
                foreach (var permit in defsListForReading)
                {
                    if (!CanDrawPermit(permit)) continue;
                    var vector1 = DrawPosition(permit);
                    start.x = vector1.x;
                    start.y = vector1.y + 25f;
                    var prerequisite = permit.prerequisite;
                    if (prerequisite != null)
                    {
                        var vector2 = DrawPosition(prerequisite);
                        end.x = vector2.x + 200f;
                        end.y = vector2.y + 25f;
                        if ((columnIndex == 1 && selectedPermit == permit) || selectedPermit == prerequisite)
                            Widgets.DrawLine(start, end, TexUI.HighlightLineResearchColor, 4f);
                        else if (columnIndex == 0)
                            Widgets.DrawLine(start, end, TexUI.DefaultLineResearchColor, 2f);
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
        /* Fake static method, replaced in runtime with real. */
        /* Why? I dunno, this is working that way, I will try to fix it later. */

        
        private static bool CanDrawPermit(RoyalTitlePermitDef permit)
        {
            if (permit.permitPointCost <= 0)
                return false;
            return permit.faction == null || permit.faction == selectedFaction.def;
        }
        
        
        private static List<FloatMenuOption> SetCategoryButton()
        {
            var tabOptions = new List<FloatMenuOption>();
            var catTable = DefDatabase<RoyaltyPermitCategoryTableDef>.GetNamedSilentFail("PermitCategoryTable");
            var dict = new Dictionary<string, Action> {};
            foreach (var category in catTable.categories)
            {
                dict.Add(NobilitySupportUtility.permitCategory + category, () => UtilityClass.curTab = category);
            }     
            foreach (var table in dict)
            {
                tabOptions.Add(new FloatMenuOption(table.Key.Translate(), table.Value));
            }
            return tabOptions;
        }
    }
}
