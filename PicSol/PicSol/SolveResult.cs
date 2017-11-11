using System.Threading;

namespace PicSol
{
    /// <summary>
    /// The result of an attempt to solve a <see cref="Nonogram"/>
    /// </summary>
    public enum SolveResult
    {
        /// <summary>
        /// We haven't tried to solve the <see cref="Nonogram"/>.
        /// </summary>
        None = 0,

        /// <summary>
        /// We successfully solved the <see cref="Nonogram"/>
        /// </summary>
        Success,

        /// <summary>
        /// The <see cref="Nonogram"/> is unsolvable.
        /// </summary>
        Unsolvable,

        /// <summary>
        /// We took too long to solve the <see cref="Nonogram"/> and stopped.
        /// Or the user called <see cref="CancellationTokenSource.Cancel"/> on
        /// a <see cref="CancellationTokenSource"/> that was passed into
        /// <see cref="Solver.Solve(Nonogram, System.TimeSpan?, CancellationTokenSource, bool)"/>
        /// </summary>
        Cancelled
    }
}
