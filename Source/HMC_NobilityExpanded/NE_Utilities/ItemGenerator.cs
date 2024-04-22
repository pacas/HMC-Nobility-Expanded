using System.Collections.Generic;
using System.Linq;
using CombatExtended;
using JetBrains.Annotations;
using RimWorld;
using Verse;
using Random = System.Random;

namespace NobilityExpanded.Utilities
{
    public static class ItemGenerator
    {
        private static readonly Random Random = new Random();
        
        public static List<Thing> GenerateItems(PermitExtensionList extension) {
            var things = new List<Thing>();
            if (extension?.itemData == null) {
                Log.Error("Cannot find mod extension");
                return things;
            }
            
            if (extension.randomItem) {
                var randomIndex = Random.Next(extension.itemData.Count);
                var item = extension.itemData[randomIndex];
                things.AddRange(GenerateItemByType(item));
            } else {
                foreach (var item in extension.itemData) {
                    things.AddRange(GenerateItemByType(item));
                }
            }

            return things;
        }

        public static List<Thing> GenerateItemByType(ItemDataInfo item) {
            List<Thing> things = new List<Thing>();
            Thing thing;
            if (item.stuff != null) {
                thing = item.quality != null ? GenerateThingStuffQuality(item) : ThingMaker.MakeThing(item.thing, item.stuff);
            } else {
                thing = item.quality != null ? GenerateThingQuality(item) : ThingMaker.MakeThing(item.thing);
            }

            if (item.isTurret) {
                thing = thing.MakeMinified();
            }
            
            thing.stackCount = item.count;
            things.Add(thing);
            if (item.additionalItems != null) {
                foreach (var additionalItem in item.additionalItems) {
                    things.AddRange(GenerateItemByType(additionalItem));
                }
            }
            
            if (item.ammoCount == 0)
                return things;

            Thing ammoThing = item.isTurret ? GenerateAmmoForTurrets(item) : GenerateAmmoForGuns(item, thing);
            if (ammoThing == null)
                return things;
            
            things.Add(ammoThing);
            return things;
        }
        
        public static Thing GenerateThingStuffQuality(ItemDataInfo item) {
            var stuff = item.stuff;
            var quality = item.quality;
            switch (item.qualityType) {
                case "Specific":
                    return new ThingStuffPairWithQuality(item.thing, stuff, ItemGenerator.GenerateQualityFromString(quality)).MakeThing();
                case "Range":
                    return new ThingStuffPairWithQuality(item.thing, stuff, ItemGenerator.GenerateQualityFromStringRange(quality)).MakeThing();
                default:
                    return new ThingStuffPairWithQuality(item.thing, stuff, ItemGenerator.GenerateQualityFromString(quality)).MakeThing();
            }
        }
        
        public static Thing GenerateThingQuality(ItemDataInfo item) {
            Thing thing = ThingMaker.MakeThing(item.thing);
            CompQuality comp = thing.TryGetComp<CompQuality>();
            var quality = item.quality;
            switch (item.qualityType)
            {
                case "Specific":
                    comp.SetQuality(ItemGenerator.GenerateQualityFromString(quality), ArtGenerationContext.Outsider);
                    break;
                case "Range":
                    comp.SetQuality(ItemGenerator.GenerateQualityFromStringRange(quality), ArtGenerationContext.Outsider);
                    break;
                default:
                    comp.SetQuality(ItemGenerator.GenerateQualityFromString(quality), ArtGenerationContext.Outsider);
                    break;
            }
            return thing;
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
                    return list[Random.Next(list.Count)];
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
        
        [CanBeNull]
        public static Thing GenerateAmmoForGuns(ItemDataInfo item, Thing thing) {
            AmmoSetDef ammoUser = thing.TryGetComp<CompAmmoUser>().Props.ammoSet;
            if (ammoUser.ammoTypes.NullOrEmpty())
                return null;
            
            AmmoDef ammo = ammoUser.ammoTypes.First().ammo;
            Thing ammoThing = ThingMaker.MakeThing(ammo);
            ammoThing.stackCount = item.ammoCount;

            return ammoThing;
        }

        [CanBeNull]
        public static Thing GenerateAmmoForTurrets(ItemDataInfo data) {
            var turretGun = data.thing?.building?.turretGunDef;
            if (turretGun == null)
                return null;
            
            AmmoSetDef ammoUser = ThingMaker.MakeThing(turretGun).TryGetComp<CompAmmoUser>().Props.ammoSet;
            if (ammoUser.ammoTypes.NullOrEmpty())
                return null;
            
            AmmoDef ammo = ammoUser.ammoTypes.First().ammo;
            Thing ammoThing = ThingMaker.MakeThing(ammo);
            ammoThing.stackCount = data.ammoCount;

            return ammoThing;
        }
    }
}
