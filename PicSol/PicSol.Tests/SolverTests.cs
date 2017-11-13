using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace PicSol.Tests
{
    public class SolverTests
    {
        private void AssertRow(Solution solution, int rowIx, params bool[] expectedValues)
        {
            for (int col = 0; col < solution.ColumnCount; col++)
            {
                var val = solution.Tiles[rowIx, col];
                var expected = expectedValues[col];
                Assert.Equal(expected, val);
            }
        }

        [Fact]
        public void Chair()
        {
            var solution = Solver.Solve(ExampleNonograms.Chair);
            Assert.Equal(SolveResult.Success, solution.Result);
            AssertRow(solution, 0, true, true, true, false, false);
            AssertRow(solution, 1, true, true, true, false, false);
            AssertRow(solution, 2, true, true, true, true, true);
            AssertRow(solution, 3, true, false, true, false, true);
            AssertRow(solution, 4, true, false, true, false, true);
        }

        [Fact]
        public void Timeout()
        {
            var solution = Solver.Solve(ExampleNonograms.WikipediaW, TimeSpan.Zero);
            Assert.Equal(SolveResult.Cancelled, solution.Result);
        }

        [Fact]
        public void Impossible()
        {
            var solution = Solver.Solve(ExampleNonograms.Impossible);
            Assert.Equal(SolveResult.Unsolvable, solution.Result);
        }

        [Fact]
        public void WikipediaW_FinishesFastEnough()
        {
            // Note that Debug Builds are about half fast as Release builds
            var targetTime = TimeSpan.FromMilliseconds(250);

            var solution = Solver.Solve(ExampleNonograms.WikipediaW, targetTime);
            Assert.Equal(SolveResult.Success, solution.Result);
        }

        [Fact]
        public void ManyGaps_FinishesFastEnough()
        {
            // Note that Debug Builds are about half fast as Release builds
            var targetTime = TimeSpan.FromMilliseconds(250);

            var solution = Solver.Solve(ExampleNonograms.ManyGaps, targetTime);
            Assert.Equal(SolveResult.Success, solution.Result);
        }

        [Fact]
        public void PermutationsHitTimeout()
        {
            var solution = Solver.Solve(ExampleNonograms.SlowUnsolvable, TimeSpan.FromSeconds(1));
            Assert.Equal(SolveResult.Cancelled, solution.Result);
        }

        [Fact]
        public void ExternalCancellationToken()
        {
            var delay = TimeSpan.FromMilliseconds(250);

            using (var cts = new CancellationTokenSource())
            {
                var solTask = Task.Run<Solution>(() => Solver.Solve(ExampleNonograms.SlowUnsolvable, timeout: null, cancellationTokenSource: cts, useMultipleCores: true));
                cts.CancelAfter(delay);
                var solution = solTask.Result;
                Assert.Equal(SolveResult.Cancelled, solution.Result);
            }

            using (var cts = new CancellationTokenSource())
            {
                var solTask = Task.Run<Solution>(() => Solver.Solve(ExampleNonograms.SlowUnsolvable, timeout: null, cancellationTokenSource: cts, useMultipleCores: false));
                cts.CancelAfter(delay);
                var solution = solTask.Result;
                Assert.Equal(SolveResult.Cancelled, solution.Result);
            }
        }
    }
}
