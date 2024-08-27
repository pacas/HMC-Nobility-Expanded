using RimWorld;
using UnityEngine;
using Verse;

namespace NobilityExpanded.Utilities
{
    public static class TextFormatter
    {
        public static TaggedString FormBasicPermitInfo(RoyalTitlePermitDef selectedPermit, Faction selectedFaction, RoyalTitleDef currentTitle, bool isUnlocked) {
            TaggedString taggedString;
            string tagged;
            
            var label = "NextDropAvailable".Translate() + ": " + "PeriodDays".Translate(selectedPermit.cooldownDays);
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
        
        public static TaggedString FormAdditionalPermitInfoExtension(RoyalTitlePermitDef selectedPermit)
        {
            var label = "";
            var extension = selectedPermit.GetModExtension<PermitExtensionList>();
            bool isThingsExists = extension?.itemData != null && extension.itemData.Count > 0;
            bool isPawnsExists = extension?.pawnData != null && extension.pawnData.Count > 0;

            if (extension == null)
                return label;

            label += extension.randomItem ? "DropTypeRandom".Translate() : "DropTypeSpecific".Translate();
            label += "\n";
            
            if (isThingsExists) {
                if (extension.itemData[0].quality != null) {
                    string qualityLabel = extension.itemData[0].qualityType + extension.itemData[0].quality;
                    label += qualityLabel.Translate() + "\n\n";
                } else {
                    label += "\n";
                }

                label += "ItemIncludedInPermit".Translate() + "\n";
                int counter = 0;
                foreach (var item in extension.itemData) {
                    counter++;
                    try {
                        if (item.stuff != null) {
                            label += "  - " + "StuffDescription".Translate(
                                item.stuff.stuffProps.stuffAdjective,
                                item.thing.LabelCap);
                        } else {
                            label += "  - " + item.thing.label.CapitalizeFirst();
                        }

                        label += " x" + item.count + "\n";
                    } catch {
                        Log.Error("Error in permit " + selectedPermit.LabelCap + " - missing item at pos " + counter);
                        continue;
                    }
                }
            }
            
            if (isPawnsExists) {
                label += "ItemIncludedInPermit".Translate() + "\n";
                foreach (var pawnInfo in extension.pawnData) {
                    label += "  - " + pawnInfo.pawn.label.CapitalizeFirst();
                    label += " x" + pawnInfo.count + "\n";
                }
            }

            return label;
        }
    }
}
