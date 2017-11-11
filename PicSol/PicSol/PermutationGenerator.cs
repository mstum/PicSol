using System;
using System.Collections;
using System.Linq;

namespace PicSol
{
    internal static class PermutationGenerator
    {
        internal static void GeneratePermutationsForHintData(int[] hintData, int lengthPerPermutation, Action<BitArray> actionWithPermutation)
        {
            if (hintData == null || hintData.Length == 0)
            {
                throw new InvalidOperationException("GeneratePermutationsForHintData called with empty hintData. Even empty rows/columns need at least a hintData with a single element of 0.");
            }

            var hintSum = hintData.Sum();
            if (hintSum > lengthPerPermutation)
            {
                var msg = $"A hint requires more space than there is. Length: {lengthPerPermutation}, Hint Sum: {hintSum}, Hint: {string.Join(",", hintData)}";
                throw new InvalidOperationException(msg);
            }

            var hintTotalSpace = hintSum + hintData.Length - 1;
            if (hintTotalSpace > lengthPerPermutation)
            {
                var msg = $"A hint requires more space than there is. Length: {lengthPerPermutation}, Hint Sum: {hintSum}, Hint: {string.Join(",", hintData)}";
                throw new InvalidOperationException(msg);
            }

            // Try all the cheap options first.
            if (hintSum == 0)
            {
                // Permutation is completely empty
                actionWithPermutation(new BitArray(lengthPerPermutation, false));
            }
            else if (hintSum == lengthPerPermutation)
            {
                // Permutation fills the entire row/column by itself, with a single section.
                actionWithPermutation(new BitArray(lengthPerPermutation, true));
            }
            else if (hintTotalSpace == lengthPerPermutation)
            {
                // Permutation fills the entire row/column by itself, but has multiple sections with gaps between them.
                actionWithPermutation(CreateFilledPermutationWithGaps(hintData, lengthPerPermutation));
            }
            else if (hintData.Length == 1)
            {
                // Permutations for just a single element can use a simple shift from left to right
                CreateSimplePermutations(hintData[0], lengthPerPermutation, actionWithPermutation);
            }
            else
            {
                // All the cheap options are exhausted, run the expensive part of the algorithm :(
                CreateExpensivePermutations(hintData, lengthPerPermutation, hintSum, actionWithPermutation);
            }
        }

        /// <summary>
        /// Creates a single Permutation for a row/column that is completely filled, but has gaps.
        /// For example, a hint of [1,2] on a length of 4 only has one permutation: [true, false, true, true]
        /// 
        /// This method does NOT check that this is actually the case, it's up to the caller to verify this.
        /// </summary>
        /// <param name="hintData"></param>
        /// <param name="lengthPerPermutation"></param>
        /// <returns></returns>
        internal static BitArray CreateFilledPermutationWithGaps(int[] hintData, int lengthPerPermutation)
        {
            var ba = new BitArray(lengthPerPermutation, false);
            var ix = 0;

            for (int hix = 0; hix < hintData.Length; hix++)
            {
                var count = hintData[hix];
                ba.SetBits(ix, count, true);
                ix += count + 1;
            }

            return ba;
        }

        /// <summary>
        /// Create all permutations for the simple case where we just shift one element from left to right.
        /// For example, a hintData of [2] with a length of 4 just has these permutations:
        /// ■ ■ ∙ ∙
        /// ∙ ■ ■ ∙
        /// ∙ ∙ ■ ■
        /// </summary>
        /// <param name="hintData"></param>
        /// <param name="lengthPerPermutation"></param>
        /// <returns></returns>
        internal static void CreateSimplePermutations(int hint, int lengthPerPermutation, Action<BitArray> actionWithPermutation)
        {
            var falseCount = lengthPerPermutation - hint;
            if (falseCount == 0)
            {
                actionWithPermutation(new BitArray(lengthPerPermutation, true));
            }
            else
            {
                var left = 0;
                while (falseCount >= 0)
                {
                    var ba = new BitArray(lengthPerPermutation, false);
                    ba.SetBits(left, hint, true);

                    left++;
                    falseCount--;

                    actionWithPermutation(ba);
                }
            }
        }

        /// <summary>
        /// Creates all possible permutations.
        /// This is the most expensive way to generate permutations, and can result in thousands of permutations.
        /// 
        /// Example, hintData is [1,2], and lengthPerPermutation is 7 => There are 10 possible permutations:
        /// ■ ∙ ■ ■ ∙ ∙ ∙
        /// ■ ∙ ∙ ■ ■ ∙ ∙
        /// ■ ∙ ∙ ∙ ■ ■ ∙
        /// ■ ∙ ∙ ∙ ∙ ■ ■
        /// ∙ ■ ∙ ■ ■ ∙ ∙
        /// ∙ ■ ∙ ∙ ■ ■ ∙
        /// ∙ ■ ∙ ∙ ∙ ■ ■
        /// ∙ ∙ ■ ∙ ■ ■ ∙
        /// ∙ ∙ ■ ∙ ∙ ■ ■
        /// ∙ ∙ ∙ ■ ∙ ■ ■
        /// </summary>
        /// <param name="hintData"></param>
        /// <param name="lengthPerPermutation"></param>
        /// <param name="hintSum"></param>
        /// <param name="actionWithPermutation"></param>
        internal static void CreateExpensivePermutations(int[] hintData, int lengthPerPermutation, int hintSum, Action<BitArray> actionWithPermutation)
        {
            var falseCount = lengthPerPermutation - hintSum + 1; // The + 1 is intentional, because of the way the Enumerator works.
            foreach (var bas in ExpensivePermutationEnumerator.CreateEnumerable(hintData, 0, lengthPerPermutation, falseCount))
            {
                actionWithPermutation(bas.Permutation);
            }
        }
    }
}
