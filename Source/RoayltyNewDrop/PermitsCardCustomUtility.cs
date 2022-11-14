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
        private static readonly Vector2 PermitOptionSpacing = new Vector2(0.25f, 0.35f);
        private static readonly Texture2D SwitchFactionIcon = ContentFinder<Texture2D>.Get("UI/Icons/SwitchFaction");

        private static bool ShowSwitchFactionButton
        {
            get
            {
                var num = 0;
                foreach (var faction in Find.FactionManager.AllFactionsVisible)
                    if (!faction.IsPlayer && !faction.def.permanentEnemy && !faction.temporary)
                        foreach (var allDef in DefDatabase<RoyalTitlePermitDef>.AllDefs)
                            if (allDef.faction == faction.def)
                            {
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
                string label = "ReturnAllPermits".Translate();
                var rect2 = new Rect(rect.xMax - 180f, rect.y - 4f, 180f, 51f);
                var num = TotalReturnPermitsCost(pawn);
                if (Widgets.ButtonText(rect2, label))
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
            var currentTitle = pawn.royalty.GetCurrentTitle(selectedFaction);
            var rect3 = new Rect((float)(rect.xMax - 360.0 - 4.0), rect.y - 4f, 360f, 55f);
            var label1 = "CurrentTitle".Translate() + ": " +
                (currentTitle != null
                    ? currentTitle.GetLabelFor(pawn).CapitalizeFirst()
                    : (string)"None".Translate()) + "\n" + "UnusedPermits".Translate() + ": " +
                pawn.royalty.GetPermitPoints(selectedFaction).ToString();
            if (!selectedFaction.def.royalFavorLabel.NullOrEmpty())
                label1 = label1 + "\n" + selectedFaction.def.royalFavorLabel.CapitalizeFirst() + ": " +
                         pawn.royalty.GetFavor(selectedFaction).ToString();
            Widgets.Label(rect3, label1);
            rect.yMin += 55f;
            var rect4 = new Rect(rect);
            rect4.width *= 0.33f;
            DoLeftRect(rect4, pawn);
            DoRightRect(new Rect(rect) {xMin = rect4.xMax + 10f}, pawn);
        }

        private static void DoLeftRect(Rect rect, Pawn pawn)
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
                if (!selectedPermit.description.NullOrEmpty())
                {
                    var rect3 = new Rect(0.0f, y2, rect1.width, 0.0f);
                    Widgets.LabelCacheHeight(ref rect3, selectedPermit.description);
                    y2 += rect3.height + 16f;
                }
                TaggedString taggedString;
                string tagged;
                var rect4 = new Rect(0.0f, y2, rect1.width, 0.0f);
                string label = "Cooldown".Translate() + ": " +
                               "PeriodDays".Translate(selectedPermit.cooldownDays);

                if (selectedPermit.permitPointCost < 90)
                {
                    label += "\n" + (string) ("PrivilegesPointsRequired"
                        .Translate(selectedPermit.permitPointCost));
                }
                
                if (selectedPermit.royalAid != null && selectedPermit.royalAid.favorCost > 0 &&
                    !selectedFaction.def.royalFavorLabel.NullOrEmpty())
                    label += "\n" + (string) ("CooldownUseFavorCost"
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

        private static void DoRightRect(Rect rect, Pawn pawn)
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
            PermitsCardCustomUtility.DrawLines();
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
                    var rect2 = new Rect(vector2.x, vector2.y, 200f, 50f);
                    if (permit.permitPointCost == 99)
                    {
                        textColor = new Color(1f, 0.5f, 0.0f, 1.0f);
                        bgColor = new Color(0.06f, 0.06f, 0.06f, 1);
                        rect2 = new Rect(vector2.x, vector2.y, 300f, 50f);
                    }
                    else if (permit.permitPointCost == 98)
                    {
                        textColor = new Color(1f, 0.8f, 0.2f, 1.0f);
                        bgColor = new Color(0.06f, 0.06f, 0.06f, 1);
                        rect2 = new Rect(vector2.x, vector2.y, 150f, 50f);
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
                    if (Widgets.CustomButtonText(ref rect2, string.Empty, bgColor, textColor, borderResearchColor))
                    {
                        SoundDefOf.Click.PlayOneShotOnCamera();
                        selectedPermit = permit;
                    }

                    var anchor = (int)Text.Anchor;
                    var color = GUI.color;
                    GUI.color = textColor;
                    Text.Anchor = TextAnchor.MiddleCenter;
                    Widgets.Label(rect2, permit.LabelCap);
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
        {
            var stuffDefOrdered = DefDatabase<OrderedStuffDef>.GetNamedSilentFail(permit.defName + "Stuff");
            int index;
            Vector2 newCoords;
            Log.Message(permit.label);
            if (stuffDefOrdered != null)
            {
                var autopatcher =
                    DefDatabase<RoyaltyCoordsTableDef>.GetNamed("CoordsTableColumn_" + stuffDefOrdered.column);
                index = autopatcher.loadOrder.IndexOf(permit);
                newCoords = new Vector2(autopatcher.coordX * 200f, index * 50f);
            }
            else if (permit.permitPointCost == 99)
            {
                var autopatcher = DefDatabase<RoyaltyCoordsTableDef>.GetNamed("CoordsTableColumn_0");
                index = autopatcher.loadOrder.IndexOf(permit);
                newCoords = new Vector2(60f, index * 50f);
            }
            else if (permit.permitPointCost == 98)
            {
                var autopatcher = DefDatabase<RoyaltyCoordsTableDef>.GetNamed("CoordsTableColumn_0");
                index = autopatcher.loadOrder.IndexOf(permit);
                newCoords = new Vector2(120f, index * 40f);
            }
            else
            {
                newCoords = new Vector2(permit.uiPosition.x * 200f, permit.uiPosition.y * 50f);
            }
            return newCoords + newCoords * PermitOptionSpacing;
        }

        private static bool CanDrawPermit(RoyalTitlePermitDef permit)
        {
            if (permit.permitPointCost <= 0)
                return false;
            return permit.faction == null || permit.faction == selectedFaction.def;
        }
    }
}