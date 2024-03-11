using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;
using Random = System.Random;

namespace NobilityExpanded
{
    public static class NE_Utility
    {
        private static readonly Random Random = new Random();

        // temp def or ext functions
        public static List<ThingDef> GetPermitStuffList(RoyalTitlePermitDef permit) {
            var stuffDefOrdered = DefDatabase<OrderedStuffDef>.GetNamedSilentFail(permit.defName + "Stuff");
            var extension = permit.GetModExtension<PermitExtensionList>();
            return stuffDefOrdered != null ? stuffDefOrdered.stuffList : extension?.stuffList;
        }
        
        public static List<PawnKindDef> GetPermitPawnList(RoyalTitlePermitDef permit) {
            var stuffDefOrdered = DefDatabase<OrderedStuffDef>.GetNamedSilentFail(permit.defName + "Stuff");
            var extension = permit.GetModExtension<PermitExtensionList>();
            return stuffDefOrdered != null ? stuffDefOrdered.pawnToChoose : extension?.pawnToChoose;
        }
        
        // menu labels functions

        public static TaggedString FormBasicPermitInfo(RoyalTitlePermitDef selectedPermit, Faction selectedFaction, RoyalTitleDef currentTitle, bool isUnlocked) {
            TaggedString taggedString;
            string tagged;
            
            var label = "Cooldown".Translate() + ": " + "PeriodDays".Translate(selectedPermit.cooldownDays);
            label += "\n" + "PrivilegesPointsRequired".Translate(selectedPermit.permitPointCost);
            if (selectedPermit.royalAid != null && selectedPermit.royalAid.favorCost > 0 && !selectedFaction.def.royalFavorLabel.NullOrEmpty()) {
                label += "\n" + "CooldownUseFavorCost".Translate(selectedFaction.def.royalFavorLabel.Named("HONOR")).CapitalizeFirst();
                label +=  ": " + selectedPermit.royalAid.favorCost.ToString();
            }
            
            if (selectedPermit.minTitle != null) {
                taggedString = "RequiresTitle".Translate((NamedArgument)selectedPermit.minTitle.GetLabelForBothGenders());
                tagged = taggedString.Resolve().Colorize(currentTitle == null || currentTitle.seniority < selectedPermit.minTitle.seniority ? ColorLibrary.RedReadable : Color.white);
                label += "\n" + tagged;
            }

            if (selectedPermit.prerequisite != null) {
                taggedString = "UpgradeFrom".Translate(selectedPermit.prerequisite.LabelCap);
                tagged = taggedString.Resolve().Colorize(isUnlocked ? Color.white : ColorLibrary.RedReadable);
                label += "\n" + tagged;
            }

            label += "\n\n";
            
            return label;
        }

        public static TaggedString FormAdditionalPermitInfoStuff(RoyalTitlePermitDef selectedPermit)
        {
            var label = "";
            var stuffDefOrdered = DefDatabase<OrderedStuffDef>.GetNamedSilentFail(selectedPermit.defName + "Stuff");
            var stuffList = GetPermitStuffList(selectedPermit);
            var pawnToChoose = GetPermitPawnList(selectedPermit);
            var isPawnsExists = pawnToChoose != null;
            var isStuffExists = stuffList != null;
            var isRoyalAidExists = selectedPermit.royalAid != null;

            bool isThingsExists = stuffDefOrdered != null && stuffDefOrdered.thingsToChoose != null && stuffDefOrdered.thingsToChoose.Count > 1;

            if (isThingsExists) {
                if (!isStuffExists || stuffList.Count != stuffDefOrdered.thingsToChoose.Count) {
                    label += "ItemIncludedInPermit".Translate() + "\n";
                    foreach (var t in stuffDefOrdered.thingsToChoose) {
                        label += "  - " + t.label.CapitalizeFirst() + "\n";
                    }
                } else {
                    for (var index = 0; index < stuffDefOrdered.thingsToChoose.Count; ++index) {
                        label += "  - " + "StuffDescription".Translate(
                            stuffList[index].stuffProps.stuffAdjective,
                            stuffDefOrdered.thingsToChoose[index].label) + "\n";
                    }
                }
            } else if (isPawnsExists && pawnToChoose.Count > 1) {
                label += "ItemIncludedInPermit".Translate() + "\n";
                foreach (var t in pawnToChoose) {
                    label += "  - " + t.LabelCap + "\n";
                }
            } else if (isRoyalAidExists &&
                     selectedPermit.royalAid.itemsToDrop != null &&
                     selectedPermit.royalAid.itemsToDrop[0].thingDef.defName != "Steel" &&
                     isStuffExists)
            {
                label += "ItemIncludedInPermit".Translate() + "\n";
                for (var index = 0; index < selectedPermit.royalAid.itemsToDrop.Count; ++index)
                {
                    label += "  - " + "StuffDescription".Translate(
                        stuffList[index].stuffProps.stuffAdjective,
                        selectedPermit.royalAid.itemsToDrop[index].Label) + "\n";
                }
            }

            if (isThingsExists && isStuffExists && isRoyalAidExists &&
                stuffList.Count == 1 &&
                selectedPermit.royalAid.itemsToDrop != null &&
                selectedPermit.royalAid.itemsToDrop[0].thingDef.defName == "Steel")
            {
                if (stuffDefOrdered.thingsToChoose.Count != 1) {
                    label += "StuffDescriptionSingle".Translate(
                        stuffList[0].LabelCap) + "\n";
                } else {
                    label += "  - " + "StuffDescription".Translate(
                    stuffList[0].stuffProps.stuffAdjective,
                    stuffDefOrdered.thingsToChoose[0].label) + "\n";
                }
            }

            if (stuffDefOrdered != null && stuffDefOrdered.typeOfQuality != null) {
                string qualityLabel = stuffDefOrdered.typeOfQuality + stuffDefOrdered.quality;
                label += qualityLabel.Translate() + "\n";
            }

            return label;
        }
        
        public static TaggedString FormAdditionalPermitInfoExtension(RoyalTitlePermitDef selectedPermit)
        {
            var label = "";
            var extension = selectedPermit.GetModExtension<PermitExtensionList>();
            bool isThingsExists = extension?.data != null && extension.data.Count > 1;
            
            if (isThingsExists) {
                label += "ItemIncludedInPermit".Translate() + "\n";
                foreach (var item in extension.data) {
                    if (item.stuff != null) {
                        label += "  - " + "StuffDescription".Translate(
                            item.stuff.stuffProps.stuffAdjective,
                            item.thing.LabelCap);
                    } else {
                        label += "  - " + item.thing.label.CapitalizeFirst();
                    }

                    label += " x" + item.count + "\n";
                }
            }

            return label;
        }
        
        public static QualityCategory GenerateQualityFromString(string quality) {
            switch (quality)
            {
                case "Awful":
                    return QualityCategory.Awful;
                case "Poor":
                    return QualityCategory.Poor;
                case "Normal":
                    return QualityCategory.Normal;
                case "Good":
                    return QualityCategory.Good;
                case "Excellent":
                    return QualityCategory.Excellent;
                case "Masterwork":
                    return QualityCategory.Masterwork;
                case "Legendary":
                    return QualityCategory.Legendary;
                default:
                    return QualityCategory.Normal;
            }
        }
        
        public static QualityCategory GenerateQualityFromStringRange(string quality) {
            var list = new List<QualityCategory>();
            switch (quality)
            {
                case "Poor":
                    list.AddRange(new List<QualityCategory>
                    {
                        QualityCategory.Awful,
                        QualityCategory.Poor,
                        QualityCategory.Normal,
                    });
                    return list[Random.Next(list.Count)];
                case "Normal":
                    list.AddRange(new List<QualityCategory>
                    {
                        QualityCategory.Good,
                        QualityCategory.Poor,
                        QualityCategory.Normal,
                    });
                    return list[Random.Next(list.Count)];
                case "Good":
                    list.AddRange(new List<QualityCategory>
                    {
                        QualityCategory.Good,
                        QualityCategory.Excellent,
                        QualityCategory.Normal,
                    });
                    return list[Random.Next(list.Count)];
                case "Excellent":
                    list.AddRange(new List<QualityCategory>
                    {
                        QualityCategory.Good,
                        QualityCategory.Excellent,
                        QualityCategory.Masterwork,
                    });
                    return list[Random.Next(list.Count)];
                case "Masterwork":
                    list.AddRange(new List<QualityCategory>
                    {
                        QualityCategory.Legendary,
                        QualityCategory.Excellent,
                        QualityCategory.Masterwork,
                    });
                    return QualityCategory.Masterwork;
                default:
                    list.AddRange(new List<QualityCategory>
                    {
                        QualityCategory.Good,
                        QualityCategory.Poor,
                        QualityCategory.Normal,
                    });
                    return list[Random.Next(list.Count)];
            }
        }
    }
}
