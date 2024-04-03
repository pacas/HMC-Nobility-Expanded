using System;
using RimWorld;
using Verse;
using Random = System.Random;

namespace NobilityExpanded.Utilities
{
    public static class PermitPawnsGenerator
    {
        private static readonly Random Random = new Random();
        
        public static Pawn GeneratePawnWithGender(PawnDataInfo data, int index) {
            string genderInfo = data.genderInfo;
            Gender gender = GenerateGenderFromString(genderInfo, index);
            PawnGenerationRequest request = new PawnGenerationRequest(data.pawn, Faction.OfPlayer, fixedGender: gender);
            return PawnGenerator.GeneratePawn(request);
        }
        
        private static Gender GenerateGenderFromString(string genderInfo, int index) {
            switch (genderInfo) {
                case "RandomBase":
                    return index > 0 ? Gender.Female : Gender.Male;
                case "RandomFull":
                    var genders = Enum.GetValues(typeof(Gender));
                    return (Gender)genders.GetValue(Random.Next(genders.Length));
                case "Female":
                    return Gender.Female;
                case "Male":
                    return Gender.Male;
                case "None":
                    return Gender.None;
                default:
                    return Gender.Female;
            }
        }
    }
}
