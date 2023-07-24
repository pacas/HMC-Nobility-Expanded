using System;
using System.Collections.Generic;
using RimWorld;

namespace NobilityExpanded
{
    public class NobilitySupportUtility
    {
        public const string PermitCategory = "PermitCategory_";
        public const string CoordsTable = "CoordsTable";
        public const string StuffPostfix = "Stuff";
        public const string CoordsTableColumn = "CoordsTableColumn_";

        private static readonly Random Random = new Random();
        public string curTab = "Resources";
        
        public static QualityCategory GenerateFromString(string quality)
        {
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
        
        public static QualityCategory GenerateFromStringRange(string quality)
        {
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
