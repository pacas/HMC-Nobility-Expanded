﻿using System.Collections.Generic;
using CombatExtended;
using RimWorld;
using Verse;
using Random = System.Random;

namespace NobilityExpanded.Utilities
{
    public static class ItemGenerator
    {
        private static readonly Random Random = new Random();
        
        public static List<Thing> GenerateItemRandom(List<ItemDataInfo> items) {
            var randomIndex = Random.Next(items.Count);
            var item = items[randomIndex];
            List<Thing> things = GenerateItemByType(item);
            if (item.additionalItems == null)
                return things;

            foreach (var itemAdditional in item.additionalItems) {
                var thing = ThingMaker.MakeThing(itemAdditional.thing);
                thing.stackCount = itemAdditional.count;
                things.Add(thing);
            }

            return things;
        }

        public static List<Thing> GenerateItemByType(ItemDataInfo item)
        {
            List<Thing> things = new List<Thing>();
            Thing thing;
            if (item.stuff != null){
                thing = item.quality != null ? GenerateThingStuffQuality(item) : ThingMaker.MakeThing(item.thing, item.stuff);
            } else {
                thing = item.quality != null ? GenerateThingQuality(item) : ThingMaker.MakeThing(item.thing);
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
            
            AmmoSetDef ammoUser = thing.TryGetComp<CompAmmoUser>().Props.ammoSet;
            if (ammoUser.ammoTypes.NullOrEmpty())
                return things;
            
            AmmoDef ammo = ammoUser.ammoTypes[0].ammo;
            Thing ammoThing = ThingMaker.MakeThing(ammo);
            ammoThing.stackCount = item.ammoCount;
            things.Add(ammoThing);

            return things;
        }
        
        public static Thing GenerateThingStuffQuality(ItemDataInfo item)
        {
            var stuff = item.stuff;
            var quality = item.quality;
            switch (item.quality)
            {
                case "Specific":
                    return new ThingStuffPairWithQuality(item.thing, stuff, ItemGenerator.GenerateQualityFromString(quality)).MakeThing();
                case "Range":
                    return new ThingStuffPairWithQuality(item.thing, stuff, ItemGenerator.GenerateQualityFromStringRange(quality)).MakeThing();
                default:
                    return new ThingStuffPairWithQuality(item.thing, stuff, ItemGenerator.GenerateQualityFromString(quality)).MakeThing();
            }
        }
        
        public static Thing GenerateThingQuality(ItemDataInfo item)
        {
            Thing thing = ThingMaker.MakeThing(item.thing);
            CompQuality comp = thing.TryGetComp<CompQuality>();
            var quality = item.quality;
            switch (quality)
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
    }
}