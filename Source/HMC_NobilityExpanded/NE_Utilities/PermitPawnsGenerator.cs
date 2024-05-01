using System;
using System.Collections.Generic;
using NobilityExpanded.NE_Data;
using RimWorld;
using Verse;
using Random = System.Random;

namespace NobilityExpanded.Utilities
{
    public static class PermitPawnsGenerator
    {
        private static readonly Random Random = new Random();
        
        public static Pawn GeneratePawnWithGender(PawnDataInfo data, int index) {
            GenderType genderInfo = data.genderInfo;
            Gender gender = GenerateGenderFromExt(genderInfo, index);
            PawnGenerationRequest request = new PawnGenerationRequest(data.pawn, Faction.OfPlayer, fixedGender: gender);
            return PawnGenerator.GeneratePawn(request);
        }
        
        private static Gender GenerateGenderFromExt(GenderType genderInfo, int index) {
            switch (genderInfo) {
                case GenderType.RandomBase:
                    return index % 2 == 0 ? Gender.Female : Gender.Male;
                case GenderType.RandomBinary:
                    var gendersBinary = new List<Gender>{Gender.Male, Gender.Female};
                    return gendersBinary[Random.Next(gendersBinary.Count)];
                case GenderType.RandomNonBinary:
                    var gendersNonBinary = Enum.GetValues(typeof(Gender));
                    return (Gender)gendersNonBinary.GetValue(Random.Next(gendersNonBinary.Length));
                case GenderType.Female:
                    return Gender.Female;
                case GenderType.Male:
                    return Gender.Male;
                case GenderType.None:
                    return Gender.None;
                default:
                    return Gender.Female;
            }
        }
    }
}
