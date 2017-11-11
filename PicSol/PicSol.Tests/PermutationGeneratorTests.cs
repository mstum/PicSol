using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace PicSol.Tests
{
    public class PermutationGeneratorTests
    {
        [Fact]
        public void GeneratePermutationsForHintDataTest_Empty()
        {
            int[] hintData = new int[] { 0 };
            var permutations = new List<BitArray>();
            PermutationGenerator.GeneratePermutationsForHintData(hintData, 4, permutations.Add);

            Assert.Equal(1, permutations.Count);
            Assert.True(permutations[0].Cast<bool>().All(b => b == false));
        }

        [Fact]
        public void GeneratePermutationsForHintDataTest_Full()
        {
            int[] hintData = new int[] { 4 };
            var permutations = new List<BitArray>();
            PermutationGenerator.GeneratePermutationsForHintData(hintData, 4, permutations.Add);

            Assert.Equal(1, permutations.Count);
            Assert.True(permutations[0].Cast<bool>().All(b => b == true));
        }

        [Fact]
        public void GeneratePermutationsForHintDataTest_Full_Gaps()
        {
            // [true, false, true, true]
            int[] hintData = new int[] { 1, 2 };
            var length = 4;
            var permutations = new List<BitArray>();
            PermutationGenerator.GeneratePermutationsForHintData(hintData, length, permutations.Add);

            Assert.Equal(1, permutations.Count);
            var result = permutations[0];
            Assert.Equal(true, result[0]);
            Assert.Equal(false, result[1]);
            Assert.Equal(true, result[2]);
            Assert.Equal(true, result[3]);
        }

        [Fact]
        public void GeneratePermutationsForHintDataTest_SimpleShift()
        {
            int[] hintData = new int[] { 2 };
            var length = 4;

            /// [true, true, false, false]
            var expected1 = new BitArray(4, false);
            expected1.Set(0, true);
            expected1.Set(1, true);

            /// [false, true, true, false]
            var expected2 = new BitArray(4, false);
            expected2.Set(1, true);
            expected2.Set(2, true);

            /// [false, false, true, true]
            var expected3 = new BitArray(4, false);
            expected3.Set(2, true);
            expected3.Set(3, true);

            var permutations = new List<BitArray>();
            PermutationGenerator.GeneratePermutationsForHintData(hintData, length, permutations.Add);

            Assert.Equal(3, permutations.Count);
            Assert.Contains(expected1, permutations);
            Assert.Contains(expected2, permutations);
            Assert.Contains(expected3, permutations);
        }

        [Fact]
        public void CreateFilledPermutationWithGapsTest_TwoElement()
        {
            // [true, false, true, true]
            int[] hintData = new int[] { 1, 2 };
            var length = 4;
            var result = PermutationGenerator.CreateFilledPermutationWithGaps(hintData, length);

            Assert.Equal(length, result.Count);
            Assert.Equal(true, result[0]);
            Assert.Equal(false, result[1]);
            Assert.Equal(true, result[2]);
            Assert.Equal(true, result[3]);
        }

        [Fact]
        public void CreateFilledPermutationWithGapsTest_FourElement()
        {
            // [true, false, true, true]
            int[] hintData = new int[] { 4, 5, 3, 2 };
            var length = 17;
            var result = PermutationGenerator.CreateFilledPermutationWithGaps(hintData, length);

            Assert.Equal(length, result.Count);
            Assert.Equal(true, result[0]);
            Assert.Equal(true, result[1]);
            Assert.Equal(true, result[2]);
            Assert.Equal(true, result[3]);

            Assert.Equal(false, result[4]);

            Assert.Equal(true, result[5]);
            Assert.Equal(true, result[6]);
            Assert.Equal(true, result[7]);
            Assert.Equal(true, result[8]);
            Assert.Equal(true, result[9]);

            Assert.Equal(false, result[10]);

            Assert.Equal(true, result[11]);
            Assert.Equal(true, result[12]);
            Assert.Equal(true, result[13]);

            Assert.Equal(false, result[14]);

            Assert.Equal(true, result[15]);
            Assert.Equal(true, result[16]);
        }

        [Fact]
        public void CreateSimplePermutationsTest()
        {
            int[] hintData = new int[] { 2 };
            var length = 4;

            /// [true, true, false, false]
            var expected1 = new BitArray(4, false);
            expected1.Set(0, true);
            expected1.Set(1, true);

            /// [false, true, true, false]
            var expected2 = new BitArray(4, false);
            expected2.Set(1, true);
            expected2.Set(2, true);

            /// [false, false, true, true]
            var expected3 = new BitArray(4, false);
            expected3.Set(2, true);
            expected3.Set(3, true);

            var permutations = new List<BitArray>();
            PermutationGenerator.CreateSimplePermutations(hintData, length, permutations.Add);

            Assert.Equal(3, permutations.Count);
            Assert.Contains(expected1, permutations);
            Assert.Contains(expected2, permutations);
            Assert.Contains(expected3, permutations);
        }
    }
}
