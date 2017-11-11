using System;

namespace PicSol
{
    /// <summary>
    /// The solved <see cref="PicSol.Nonogram"/> - if it was solvable.
    /// </summary>
    public class Solution
    {
        public SolveResult Result { get; }
        public bool IsSolved => Result == SolveResult.Success;

        public SolverState SolverState { get; }

        /// <summary>
        /// The actual tiles of the solved Nonogram.
        /// Note that if <see cref="IsSolved"/> is false,
        /// this contains an incomplete state (that may be useful
        /// to find the error in the Nonogram).
        /// </summary>
        public ReadOnlyTilesCollection Tiles => SolverState.Tiles;
        public int RowCount => SolverState.Tiles.RowCount;
        public int ColumnCount => SolverState.Tiles.ColumnCount;       
        public TimeSpan TimeTaken => SolverState.Elapsed;
        public Nonogram Nonogram => SolverState.Nonogram;

        public Solution(SolverState finalSolverState, SolveResult result)
        {
            SolverState = finalSolverState ?? throw new ArgumentNullException(nameof(finalSolverState));
            Result = result;
        }
    }
}
