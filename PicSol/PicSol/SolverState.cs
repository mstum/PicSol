using System;
using System.Collections;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace PicSol
{
    public class SolverState
    {
        private readonly CancellationTokenSource _cancellationTokenSource;

        public Stopwatch Stopwatch { get; }
        public Nonogram Nonogram { get; }
        public TimeSpan? Timeout { get; }
        public CancellationToken CancellationToken { get; }

        public bool UseMultipleCores { get; }
        public PermutationsCollection RowPermutations { get; set; }
        public PermutationsCollection ColumnPermutations { get; set; }
        public TimeSpan Elapsed { get; set; }
        public TilesCollection Tiles { get; }

        // PerformanceMetrics
        public int StepCount { get; set; }
        public int InitialRowPermutationsCount { get; set; }
        public int InitialColumnPermutationsCount { get; set; }

        public SolverState(Nonogram nonogram, TimeSpan? timeout, bool useMultipleCores, CancellationTokenSource cancellationTokenSource)
        {
            Nonogram = nonogram ?? throw new ArgumentNullException(nameof(nonogram));
            _cancellationTokenSource = cancellationTokenSource ?? throw new ArgumentNullException(nameof(cancellationTokenSource));
            CancellationToken = _cancellationTokenSource.Token;
            Timeout = timeout;
            UseMultipleCores = useMultipleCores;

            Stopwatch = new Stopwatch();
            Tiles = new TilesCollection(nonogram.RowCount, nonogram.ColumnCount);
        }

        public bool HasHitTimeout => Timeout.HasValue && Elapsed > Timeout.Value;
        public bool WillHitTimeout => Timeout.HasValue && Elapsed + Stopwatch.Elapsed > Timeout.Value;
        public void UpdateElapsed() => Elapsed += Stopwatch.Elapsed;
        public void CancelToken() => _cancellationTokenSource.Cancel();

        public Solution CreateSolution(SolveResult solverResult)
        {
            var isSolvable = UpdateTiles();
            if (solverResult == SolveResult.Success && !isSolvable)
            {
                solverResult = SolveResult.Unsolvable;
            }

            return new Solution(this, solverResult);
        }

        private bool UpdateTiles()
        {
            for (int rowIndex = 0; rowIndex < RowPermutations.Count; rowIndex++)
            {
                var row = RowPermutations[rowIndex];
                if (row.Count < 1)
                {
                    return false;
                }

                for (int col = 0; col < ColumnPermutations.Count; col++)
                {
                    Tiles.SetTile(rowIndex, col, row[0][col]);
                }
            }

            return true;
        }

        /// <summary>
        /// Create Row and Column Permutations.
        /// </summary>
        /// <returns>FALSE if we hit the timeout, TRUE if everything went well.</returns>
        internal bool PopulatePermutations()
        {
            var rowPermutations = new PermutationsCollection(Nonogram.RowHints.Count);
            var colPermutations = new PermutationsCollection(Nonogram.ColumnHints.Count);
            var colLength = Nonogram.ColumnHints.Count;
            var rowLength = Nonogram.RowHints.Count;
            bool result = true;

            Stopwatch.Restart();
            if (UseMultipleCores)
            {
                var po = new ParallelOptions { CancellationToken = CancellationToken, MaxDegreeOfParallelism = Environment.ProcessorCount };
                void AddAndCheckTimeout(BitArray ba, PermutationsCollection collection, int index)
                {
                    collection[index].Add(ba);
                    if (WillHitTimeout || CancellationToken.IsCancellationRequested)
                    {
                        CancelToken();
                    }
                }

                try
                {
                    Parallel.For(0, rowLength, po, ix =>
                    {
                        PermutationGenerator.GeneratePermutationsForHintData(Nonogram.RowHints[ix], colLength, ba => AddAndCheckTimeout(ba, rowPermutations, ix));
                    });

                    Parallel.For(0, colLength, po, ix =>
                    {
                        PermutationGenerator.GeneratePermutationsForHintData(Nonogram.ColumnHints[ix], rowLength, ba => AddAndCheckTimeout(ba, colPermutations, ix));
                    });

                    result = true;
                }
                catch (OperationCanceledException)
                {
                    result = false;
                }
            }
            else
            {
                for (int ix = 0; ix < rowLength; ix++)
                {
                    PermutationGenerator.GeneratePermutationsForHintData(Nonogram.RowHints[ix], colLength, rowPermutations[ix].Add);
                    if (WillHitTimeout || CancellationToken.IsCancellationRequested)
                    {
                        CancelToken();
                        result = false;
                        break;
                    }
                }

                if (result)
                {
                    for (int ix = 0; ix < colLength; ix++)
                    {
                        PermutationGenerator.GeneratePermutationsForHintData(Nonogram.ColumnHints[ix], rowLength, colPermutations[ix].Add);
                        if (WillHitTimeout || CancellationToken.IsCancellationRequested)
                        {
                            CancelToken();
                            result = false;
                            break;
                        }
                    }
                }
            }
            Stopwatch.Stop();
            UpdateElapsed();

            RowPermutations = rowPermutations;
            ColumnPermutations = colPermutations;
            InitialRowPermutationsCount = rowPermutations.GetTotalPermutationCount();
            InitialColumnPermutationsCount = colPermutations.GetTotalPermutationCount();
            return result;
        }
    }
}
