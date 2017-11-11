using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;

/* This is a brute force solver, but it should be fast enough for most common Nonograms.
 * 
 * Let's assume a simple 4x5 Nonogram as the starting point:
 *       1 1
 *     2 1 2 2 0
 *   2 ∙ ■ ■ ∙ ∙
 * 1 1 ■ ∙ ∙ ■ ∙
 *   4 ■ ■ ■ ■ ∙
 *   1 ∙ ∙ ■ ∙ ∙
 * 
 * First off, we generate two PermutationCollections, with all possible Permutations for each row and column.
 * 
 * Each of these collection has exactly as many entries as there are spaces for that axis.
 * So we end up with a 4 element permutation row collection, and a 5 element column permutation collection.
 * Each element in those collections contains all possible permutations for that row/column.
 * rowPermutations[0] contains all permutations for the top row, the one with "2" as hint.
 * 
 * rowPermutations (If you're trying to visualize this, rows go from left to right)
 * [0]: 4 Permutations (Row Hint: 2)
 *      [true, true, false, false, false]
 *      [false, true, true, false, false]
 *      [false, false, true, true, false]
 *      [false, false, false, true, true]
 * [1]: 6 Permutations (Row Hint: 1 1)
 *      [true, false, true, false, false]
 *      [true, false, false, true, false]
 *      [true, false, false, false, true]
 *      [false, true, false, true, false]
 *      [false, true, false, false, true]
 *      [false, false, true, false, true]     
 * [2]: 2 Permutations (Row Hint: 4)
 *      [true, true, true, true, false]
 *      [false, true, true, true, true]
 * [3]: 5 Permutations (Row Hint: 1)
 *      [true, false, false, false, false]
 *      [false, true, false, false, false]
 *      [false, false, true, false, false]
 *      [false, false, false, true, false]
 *      [false, false, false, false, true]
 * 
 * colPermutations (If you're trying to visualize this, columns go from top to bottom)
 * [0]: 3 Permutations (Column Hint: 2)
 *      [true, true, false, false]
 *      [false, true, true, false]
 *      [false, false, true, true]
 * [1]: 3 Permutations (Column Hint: 1 1)
 *      [true, false, true, false]
 *      [true, false, false, true]
 *      [false, true, false, true]
 * [2]: 1 Permutation  (Column Hint: 1 2)
 *      [true, false, true, true]
 * [3]: 3 Permutations (Column Hint: 2)
 *      [true, true, false, false]
 *      [false, true, true, false]
 *      [false, false, true, true]
 * [4]: 1 Permutation (Column Hint: 0)
 *      [false, false, false, false]
 * 
 * After creating this initial set, we're removing combinations that won't work step by step.
 * Ideally, at the end of this, we end up with only 1 remaining permutation in all rowPermutations and colPermutations.
 * If we ever end up removing ALL permutations for a row or column, we know that the Nonogram is unsolvable.
 * 
 * During each step, we look at both collections, first trying to remove rowPermutations, then colPermutations.
 * 
 * Step #1:
 *  1. We have "colPermutations" as input and "rowPermutations" as target
 *  2. We look at each column (loop through input)
 *  3. For every column, we try to find common bits that are either set (on) across all permutations (and therefore MUST be checked) or not set (off) across any permutations
 *  4. For the first column (colPermutations[0]), on is false and off is true across all 4 bits.
 *  5. We're now going through every row (this is an inner loop, we're still in the column loop)
 *  6. We start with rowPermutations[0], which contains 4 permutations. Can we remove any?
 *  7. In order to remove a permutation, it must be the opposite of the on/off values. In other words: If on[0] is true and permutation[0] is false, we can remove that permutation. And if off[0] is false and permutation[0] is true, we can also remove that.
 *  8. We're not finding any permutations to remove from this row. Next up, rowPermutations[1] which contains 6 permutations, but also doesn't have anything to remove.
 *  9. rowPermutations[2] and [3] - no removals, we're done with this column
 * 10. Repeat for colPermutations[1]: on is false, off is true. no removals across all rowPermutations
 * 11. colPermutations[2]: off [t,f,t,t], on [t,f,t,t]. 
 * 12. We're removing permutations from rowPermutations[0]: [t,t,f,f,f] and [f,f,f,t,t] are gone, leaving [f,f,t,t,f] and [f,t,t,f,f] for now.
 * 13. rowPermutations[1] also removes two permutations, [t,f,t,f,f] and [f,f,t,f,t], leaving [t,f,f,t,f] [t,f,f,f,t] [f,t,f,t,f] [f,t,f,f,t]
 * 14. No removals in rowPermutations[2]
 * 15. We remove 4 out of 5 rowPermutations[3], leaving only [f,f,t,f,f]
 * 16. colPermutations[3]: No on/off, no rowPermutations removals
 * 17. colPermutations[4]: No on, but all off (this is the 0 column, after all). All rowPermutations that have true as the last element can go.
 * 18. No such removals in rowPermutations[0]
 * 19. 2 removals in rowPermutations[1], leaving [t,f,f,t,f] [f,t,f,t,f]
 * 20. 1 removal in rowPermutations[2], leaving only [t,t,t,t,f]
 * 21. No removals in rowPermutations[3]
 * 22. We're at the end of this call to RemovePermutations. Since there were removals, we're sending KeepGoing
 * 
 * After this first run, colPermutations is unchanged (as it's the input), while rowPermutations had quite a few removals, leaving us with this:
 * [0]: 2 Permutations left, 2 removed (Row Hint: 2)
 *      [false, true, true, false, false]
 *      [false, false, true, true, false]
 * [1]: 2 Permutations left, 4 removed (Row Hint: 1 1)
 *      [true, false, false, true, false]
 *      [false, true, false, true, false]    
 * [2]: 1 Permutation left, 1 removed (Row Hint: 4)
 *      [true, true, true, true, false]
 * [3]: 1 Permutation left, 4 removed (Row Hint: 1)
 *      [false, false, true, false, false]
 *      
 * Now, we are repeating this process with the collections swapped - we try to remove as many colPermutations as possible.
 * There were some to remove, we're now left with these colPermutations: * 
 * [0]: 1 left, 2 gone (Column Hint: 2)
 *      [false, true, true, false]
 * [1]: 1 left, 2 gone (Column Hint: 1 1)
 *      [true, false, true, false]
 * [2]: 1 left, 0 gone(Column Hint: 1 2)
 *      [true, false, true, true]
 * [3]: 1 left, 2 gone (Column Hint: 2)
 *      [false, true, true, false]
 * [4]: 1 left, 0 gone (Column Hint: 0)
 *      [false, false, false, false]
 *      
 * This concludes the first Step().
 * If we had removals in either run, we keep going, and so we Step() again.
 * 
 * Step #2:
 * We removed some more rowPermutations, and actually now we're only left with 1 permutation per row, which is the desired outcome.
 * We're still going to have at least another Step() though to make sure that we're not going to remove any further permutations,
 * leaving us with 0 permutations and an unsolvable Nonogram.
 * 
 * rowPermutations:
 * [0]: 1 Permutation left, 1 removed (Row Hint: 2)
 *      [false, true, true, false, false]
 * [1]: 1 Permutation left, 1 removed (Row Hint: 1 1)
 *      [true, false, false, true, false]
 * [2]: 1 Permutation left, 0 removed (Row Hint: 4)
 *      [true, true, true, true, false]
 * [3]: 1 Permutation left, 0 removed (Row Hint: 1)
 *      [false, false, true, false, false]
 *      
 * We did not remove any colPermutations, which is good as we're down to only 1 permutation per column.
 * But as said, because there were removals during this Step, we Step once more.
 * 
 * Step #3:
 * No row or col permutation removals - we're finished!
 * 
 * Our final permutations:
 * rowPermutations - left to right:
 *  [0]: [false, true, true, false, false]
 *  [1]: [true, false, false, true, false]
 *  [2]: [true, true, true, true, false]
 *  [3]: [false, false, true, false, false]
 *  
 * colPermutations - top to bottom:
 *  [0]: [false, true, true, false]
 *  [1]: [true, false, true, false]
 *  [2]: [true, false, true, true]
 *  [3]: [false, true, true, false]
 *  [4]: [false, false, false, false]
 *  
 *  Which results in the desired Nonogram:
 *       1 1
 *     2 1 2 2 0
 *   2 ∙ ■ ■ ∙ ∙
 * 1 1 ■ ∙ ∙ ■ ∙
 *   4 ■ ■ ■ ■ ∙
 *   1 ∙ ∙ ■ ∙ ∙
 *   
 * Even a larger Nonogram requires relatively few Steps, for example the WikipediaW Example takes 6 steps.
 * But of course, the number of loops grow exponentially, so each Step does a lot more work for larger Nonograms.
 */
namespace PicSol
{
    public static class Solver
    {
        /// <summary>
        /// Try to solve the given <see cref="Nonogram"/>.
        /// </summary>
        /// <param name="nonogram">The <see cref="Nonogram"/> to solve.</param>
        /// <param name="timeout">If given, we'll abort our search for a solution if it takes too long.</param>
        /// <param name="cancellationTokenSource">
        /// If you want to control cancellation yourself, pass in a <see cref="CancellationTokenSource"/>.
        /// Note that if you also set a <paramref name="timeout"/>, the Solver will call <see cref="CancellationTokenSource.Cancel"/> if the timeout expires.
        /// </param>
        /// <param name="useMultipleCores">If set to true, we try to parallelize solving the <see cref="Nonogram"/>, otherwise we only use 1 CPU core.</param>
        /// <returns>A <see cref="Solution"/> to the <see cref="Nonogram"/>, although it may not be a successful one.</returns>
        public static Solution Solve(Nonogram nonogram, TimeSpan? timeout = null, CancellationTokenSource cancellationTokenSource = null, bool useMultipleCores = true)
        {
            bool disposeCts = cancellationTokenSource == null;
            try
            {
                cancellationTokenSource = cancellationTokenSource ?? new CancellationTokenSource();
                var state = new SolverState(nonogram, timeout, useMultipleCores, cancellationTokenSource);

                Solution ReturnWithResult(SolveResult result) => new Solution(state, result);

                if (!state.PopulatePermutations())
                {
                    return ReturnWithResult(SolveResult.Cancelled);
                }

                var stepResult = ProcessStepResult.KeepGoing;
                while (stepResult == ProcessStepResult.KeepGoing)
                {
                    state.Stopwatch.Restart();

                    stepResult = Step(state.RowPermutations, state.ColumnPermutations, state.CancellationToken);
                    state.StepCount++;
                    if (stepResult == ProcessStepResult.Unsolvable)
                    {
                        state.UpdateTiles();
                        return ReturnWithResult(SolveResult.Unsolvable);
                    }

                    state.Stopwatch.Stop();
                    state.UpdateElapsed();
                    if (state.HasHitTimeout || state.CancellationToken.IsCancellationRequested)
                    {
                        state.CancelToken();
                        state.UpdateTiles();
                        return ReturnWithResult(SolveResult.Cancelled);
                    }
                }

                var success = state.UpdateTiles();
                return ReturnWithResult(success ? SolveResult.Success : SolveResult.Unsolvable);
            }
            finally
            {
                if (disposeCts && cancellationTokenSource != null)
                {
                    cancellationTokenSource.Dispose();
                }
            }
        }

        private static ProcessStepResult Step(PermutationsCollection rows, PermutationsCollection cols, CancellationToken cancellationToken)
        {
            try
            {
                var firstPass = RemovePermutations(cols, rows, cancellationToken);
                if (firstPass == ProcessStepResult.Unsolvable || firstPass == ProcessStepResult.Cancelled)
                {
                    return firstPass;
                }

                if (cancellationToken.IsCancellationRequested)
                {
                    return ProcessStepResult.Cancelled;
                }

                var secondPass = RemovePermutations(rows, cols, cancellationToken);
                if (secondPass == ProcessStepResult.Unsolvable || secondPass == ProcessStepResult.Cancelled)
                {
                    return secondPass;
                }

                bool keepGoing = firstPass == ProcessStepResult.KeepGoing || secondPass == ProcessStepResult.KeepGoing;
                return keepGoing ? ProcessStepResult.KeepGoing : ProcessStepResult.Finished;
            }
            catch (OperationCanceledException)
            {
                return ProcessStepResult.Cancelled;
            }
        }

        /// <summary>
        /// Go through all permutations within <paramref name="input"/> and see which fields are commonly on or commonly off between all of them.
        /// Then, remove any mismatches from <paramref name="target"/>, bringing the total permutation count down, hopefully to 1.
        /// 
        /// If we end up with no permutations in the target, the Nonogram is unsolvable.
        /// </summary>
        /// <param name="input">The collection that we're using to try to determine which permutations to remove from <paramref name="target"/>.</param>
        /// <param name="target">The collection that we're trying to remove permutations from.</param>
        /// <param name="cancellationToken">A cancellationToken to support canceling this method</param>
        /// <returns>
        /// Whether we successuly removed permutations from <paramref name="target"/>.
        /// * <see cref="ProcessStepResult.KeepGoing"/> means that permutations were removed, and that we should loop once more.
        /// * <see cref="ProcessStepResult.Finished"/> means that no further permutations were removed. We're possibly done with <paramref name="target"/> for good.
        /// * <see cref="ProcessStepResult.Unsolvable"/> means that there are no further permutations left in at least one element of <paramref name="target"/> - the Nonogram is unsolvable.
        /// * <see cref="ProcessStepResult.Cancelled"/> means that the <paramref name="cancellationToken"/> was cancelled, likely due to a timeout or user cancellation
        /// </returns>
        private static ProcessStepResult RemovePermutations(PermutationsCollection input, PermutationsCollection target, CancellationToken cancellationToken)
        {
            bool keepGoing = false;
            for (int inputIx = 0; inputIx < input.Count; inputIx++)
            {
                DetermineMustBeOnOff(input[inputIx], target.Count, cancellationToken, out var mustBeOn, out var mustBeOff);

                for (int targetIx = 0; targetIx < target.Count; targetIx++)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    if (RemoveFromCollection(target, mustBeOn, mustBeOff, inputIx, targetIx, cancellationToken))
                    {
                        keepGoing = true;
                    }

                    if (target[targetIx].Count == 0)
                    {
                        return ProcessStepResult.Unsolvable;
                    }
                }
            }
            return keepGoing ? ProcessStepResult.KeepGoing : ProcessStepResult.Finished;
        }

        private static bool RemoveFromCollection(PermutationsCollection target, BitArray mustBeOn, BitArray mustBeOff, int inputIx, int targetIx, CancellationToken cancellationToken)
        {
            var coll = target[targetIx];
            var count = coll.RemoveAll(permutation => CanPermutationBeRemoved(permutation, mustBeOn, mustBeOff, inputIx, targetIx, cancellationToken));
            return count > 0;
        }

        private static bool CanPermutationBeRemoved(BitArray permutation, BitArray mustBeOn, BitArray mustBeOff, int inputIx, int targetIx, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return (mustBeOn[targetIx] && !permutation[inputIx]) || (!mustBeOff[targetIx] && permutation[inputIx]);
        }

        private static void DetermineMustBeOnOff(IEnumerable<BitArray> permutations, int length, CancellationToken cancellationToken, out BitArray mustBeOn, out BitArray mustBeOff)
        {
            mustBeOn = new BitArray(length, true);
            mustBeOff = new BitArray(length, false);

            foreach (var permutation in permutations)
            {
                cancellationToken.ThrowIfCancellationRequested();
                mustBeOn.And(permutation);
                mustBeOff.Or(permutation);
            }
        }

        private enum ProcessStepResult
        {
            Invalid = 0,
            KeepGoing,
            Finished,
            Unsolvable,
            Cancelled
        }
    }
}
