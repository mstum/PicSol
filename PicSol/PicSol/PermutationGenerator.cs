using System;
using System.Collections;
using System.Linq;

namespace PicSol
{
    /* Algorithm:
     * Go through every Element of the HintsCollection, which gives us the hints.
     * There may be only one hint (e.g., [2]) or multiple ones (e.g., [1,1]).
     * Given a lengthPerPermutation of 4 and a hint of [2], the expected result is:
     *      [true, true, false, false]
     *      [false, true, true, false]
     *      [false, false, true, true]
     *      
     * For a hint of [1,1], the expected permutations are:
     *      [true, false, true, false]
     *      [true, false, false, true]
     *      [false, true, false, true]
     *      
     * For a one-element hint, the algorithm needs to walk that one element.
     * FalseCount = lengthPerPermutation - hint[0]
     * This tells us how many "False" there must be around the hint in total, so we need a 
     * left and right component.
     * Start out with Left = 0, Right = FalseCount and then shift over to left one by one.
     * 
     * For multi-element hints, elements need to be separated by at least one False, but can be more.
     * FalseCount = lengthPerPermutation - hintData.Sum()
     * This only tells us how many Falses there need to be, but not where they are.
     * One option is to first find all the permutations for the actual elements,
     * so with [1,1] on a length of 4, the options are "true, false, true" and "true, false, false, true".
     * With those permutations as the "center piece", we can do the same shift from left to right.
     * 
     * Of course, there are optimizations:
     * If hintData.Sum() == 0 then there is only 1 Permutation: All Empty
     * If hintData.Sum() == lengthPerPermutation then there is only 1 Permutation: All Filled
     * If hintData.Sum() + hintData.Count == lengthPerPermutation then there is only 1 Permutation: Each hintData element, separated by a single False.
     * 
     * For everything else, we need to get all the permutations.
     */
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
                CreateSimplePermutations(hintData, lengthPerPermutation, actionWithPermutation);
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
                for (int i = 0; i < hintData[hix]; i++)
                {
                    ba.Set(ix, true);
                    ix++;
                }

                if (ix < ba.Count)
                {
                    ba.Set(ix, false);
                    ix++;
                }
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
        internal static void CreateSimplePermutations(int[] hintData, int lengthPerPermutation, Action<BitArray> actionWithPermutation)
        {
            if (hintData.Length != 1)
            {
                throw new InvalidOperationException($"CreateSimplePermutations was called, but hintData doesn't have exactly 1 element. hintData: [{string.Join(",", hintData)}]");
            }

            var hd = hintData[0];

            var falseCount = lengthPerPermutation - hd;
            if (falseCount == 0)
            {
                actionWithPermutation(new BitArray(lengthPerPermutation, true));
            }
            else
            {
                var left = 0;
                var right = falseCount;

                while (right >= 0)
                {
                    var ix = 0;
                    var ba = new BitArray(lengthPerPermutation, false);
                    for (var l = 0; l < left; l++)
                    {
                        ba[ix++] = false;
                    }

                    for (var h = 0; h < hd; h++)
                    {
                        ba[ix++] = true;
                    }

                    for (var r = 0; r < right; r++)
                    {
                        ba[ix++] = false;
                    }

                    left++;
                    right--;

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
            var falseCount = lengthPerPermutation - hintSum + 1;
            foreach (var bas in ExpensivePermutationEnumerator.CreateEnumerable(hintData, 0, lengthPerPermutation, falseCount))
            {
                actionWithPermutation(bas);
            }
        }
    }
}
