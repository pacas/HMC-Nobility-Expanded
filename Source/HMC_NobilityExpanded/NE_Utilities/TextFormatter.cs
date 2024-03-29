﻿using RimWorld;
using UnityEngine;
using Verse;

namespace NobilityExpanded.Utilities
{
    public static class TextFormatter
    {
        public static TaggedString FormBasicPermitInfo(RoyalTitlePermitDef selectedPermit, Faction selectedFaction, RoyalTitleDef currentTitle, bool isUnlocked) {
            TaggedString taggedString;
            string tagged;
            
            var label = "Cooldown".Translate() + ": " + "PeriodDays".Translate(selectedPermit.cooldownDays);
            label += "\n\n" + "PrivilegesPointsRequired".Translate(selectedPermit.permitPointCost);
            if (selectedPermit.royalAid != null && selectedPermit.royalAid.favorCost > 0 && !selectedFaction.def.royalFavorLabel.NullOrEmpty()) {
                label += "\n\n" + "CooldownUseFavorCost".Translate(selectedFaction.def.royalFavorLabel.Named("HONOR")).CapitalizeFirst();
                label +=  ": " + selectedPermit.royalAid.favorCost.ToString();
            }
            
            if (selectedPermit.minTitle != null) {
                taggedString = "RequiresTitle".Translate((NamedArgument)selectedPermit.minTitle.GetLabelForBothGenders());
                tagged = taggedString.Resolve().Colorize(currentTitle == null || currentTitle.seniority < selectedPermit.minTitle.seniority ? ColorLibrary.RedReadable : Color.white);
                label += "\n\n" + tagged;
            }

            if (selectedPermit.prerequisite != null) {
                taggedString = "UpgradeFrom".Translate(selectedPermit.prerequisite.LabelCap);
                tagged = taggedString.Resolve().Colorize(isUnlocked ? Color.white : ColorLibrary.RedReadable);
                label += "\n\n" + tagged;
            }

            label += "\n\n\n\n";
            
            return label;
        }

        public static TaggedString FormAdditionalPermitInfoStuff(RoyalTitlePermitDef selectedPermit)
        {
            var label = "";
            var stuffDefOrdered = DefDatabase<OrderedStuffDef>.GetNamedSilentFail(selectedPermit.defName + "Stuff");
            var stuffList = VarsExposer.GetPermitStuffList(selectedPermit);
            var pawnToChoose = VarsExposer.GetPermitPawnList(selectedPermit);
            var isPawnsExists = pawnToChoose != null;
            var isStuffExists = stuffList != null;
            var isRoyalAidExists = selectedPermit.royalAid?.itemsToDrop != null;
            bool isThingsExists = stuffDefOrdered?.thingsToChoose != null && stuffDefOrdered.thingsToChoose.Count > 1;

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
    }
}
