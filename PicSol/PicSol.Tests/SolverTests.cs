using System;
using System.Diagnostics;
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
            var targetTime = TimeSpan.FromMilliseconds(250);

            var solution = Solver.Solve(ExampleNonograms.WikipediaW, targetTime);
            Assert.Equal(SolveResult.Success, solution.Result);
        }

        [Fact]
        public void ManyGaps_FinishesFastEnough()
        {
            var targetTime = TimeSpan.FromMilliseconds(250);

            var solution = Solver.Solve(ExampleNonograms.ManyGaps, targetTime);
            Assert.Equal(SolveResult.Success, solution.Result);
        }

        [Fact]
        public void PermutationsHitTimeout()
        {
            var sw = new Stopwatch();
            sw.Restart();
            var solution = Solver.Solve(ExampleNonograms.SlowUnsolvable, TimeSpan.FromSeconds(1));
            sw.Stop();
            Assert.True(sw.Elapsed < TimeSpan.FromSeconds(3)); // The timeout may not be exactly hit, as the current loop still finishes
            Assert.Equal(SolveResult.Cancelled, solution.Result);
            Assert.True(solution.TimeTaken < TimeSpan.FromSeconds(3));
        }
    }
}
