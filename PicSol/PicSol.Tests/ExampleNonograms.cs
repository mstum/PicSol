using System.Collections.Generic;

namespace PicSol.Tests
{
    public static class ExampleNonograms
    {
        public static IEnumerable<Nonogram> AllNonograms => new Nonogram[] { Chair, CodeDocExample, WikipediaW, PermutationTest, OneByOne, OneByTwo, OneByTwoFlipped, Impossible, ManyGaps, SlowSolvable, SlowUnsolvable };

        public static Nonogram CodeDocExample
        {
            get
            {
                var rowHints = new int[4][] { new int[] { 2 }, new int[] { 1, 1 }, new int[] { 4 }, new int[] { 1 } };
                var colHints = new int[5][] { new int[] { 2 }, new int[] { 1, 1 }, new int[] { 1, 2 }, new int[] { 2 }, new int[] { 0 } };
                return new Nonogram("Solver Doc Example", 4, 5, HintCollection.From2DArray(rowHints), HintCollection.From2DArray(colHints));
            }
        }

        public static Nonogram PermutationTest => new Nonogram("Permutation Test", 1, 4, HintCollection.FromString("1,1"), HintCollection.FromString("1 0 0 1"));

        public static Nonogram Chair => new Nonogram("Chair", 5, 5, "3 3 5 1,1,1 1,1,1", "5 3 5 1 3");

        public static Nonogram OneByOne => new Nonogram("One by One", 1, 1, "1", "1");

        public static Nonogram OneByTwo => new Nonogram("One by Two", 1, 2, "1", "0 1");

        public static Nonogram OneByTwoFlipped => new Nonogram("One by Two Flipped", 2, 1, "1 0", "1");

        public static Nonogram Impossible => new Nonogram("Impossible", 2, 2, "2 1", "1 1");

        // https://en.wikipedia.org/w/index.php?title=File:Nonogram.svg&oldid=791896154
        public static Nonogram WikipediaW => new Nonogram("Wikipedia W", 20, 30,
            "8,7,5,7 5,4,3,3 3,3,2,3 4,3,2,2 3,3,2,2 3,4,2,2 4,5,2 3,5,1 4,3,2 3,4,2 4,4,2 3,6,2 3,2,3,1 4,3,4,2 3,2,3,2 6,5 4,5 3,3 3,3 1,1",
            "1 1 2 4 7 9 2,8 1,8 8 1,9 2,7 3,4 6,4 8,5 1,11 1,7 8 1,4,8 6,8 4,7 2,4 1,4 5 1,4 1,5 7 5 3 1 1");

        public static Nonogram ManyGaps => new Nonogram("Many Gaps", 15, 15,
            "7 2,4 1,4 2,1,1,1 1,1,2 6,1,1 5,3 2,2,2 2,2,2 1,5 2,2,2 2,2,2 8 7 5",
            "2 1,1,1 3,1,1 2,1,1,2 1,2,5 1,1,4,4 1,4,2 1,1,3 2,2,4 2,4,2 1,1,2,1 2,2,1,2 2,6 1,7 3");

        // This will result in ~4.6 Million Permutations and take several seconds to run through
        public static Nonogram SlowUnsolvable => new Nonogram("Slow and Unsolvable", 34, 50,
            "3,2 10 11 2,3,3 2,1,2 1,1,2 1,1 1 1 1 1,9 2,13 4,16 6,19 6,21 5,23 7,25 7,26 10,11,14 7,3,12,15 6,2,28 3,28 4,12,15 21,1,1,1,1,1,1,1 20,1,1,1,1,1,1,1 21,1,1,1,1,1,1 34 6,21 2,1,7 16,9,2 2,5,3,6,14 1,4,2,1,1,4 10,17,7 4,7,3,1,7,6",
            "1,1 1,1,1 2,1,1 3,1,1 3,1,1 4,1,1 4,1,1 4,2,1 4,1,1 3,1,1 6,1,1 10,2,2 4,6,2,3 5,4,1,3 4,5,4,1 4,4,3,1 4,5,2,2 5,1,1 5,1,1 6,1,1 7,1,1 8,1,2 8,4 9,1,1 10,1,1 11,2,2 12,2,2 14,2,2 15,1,1 15,1,1 16,1,1 2,10,5,1,2 2,17,1,2 2,12,3,2,1 2,8,8,1,2 3,8,4,2,1,1 3,18,2,1 4,13,2,2,1 27,1,2 3,13,3,1,2 3,19,1,2 3,13,3,1,1 3,19,3 4,12,3,1,1 3,18,1,2 2,11,3,1,1 15,2,1 8,2,1,1 6,1,1 4,1,1");

        public static Nonogram SlowSolvable => new Nonogram("Slow but Solvable", 34, 50,
            "3,2 10 11 2,3,3 2,1,2 1,1,2 1,1 1 1 1 1,9 2,13 4,16 6,19 6,21 5,23 7,25 7,26 10,11,14 7,3,12,15 6,2,28 3,28 4,12,15 21,1,1,1,1,1,1,1 20,1,1,1,1,1,1,1 21,1,1,1,1,1,1 34 6,21 2,1,7 16,9,2 2,5,3,6,14 1,4,2,1,1,4 10,17,7 4,7,3,1,7,6",
            "1,1 1,1,1 2,1,1 3,1,1 3,1,1 4,1,1 4,1,1 4,2,1 4,1,1 3,1,1 6,1,1 10,2,2 4,6,2,3 5,4,1,3 4,5,4,1 4,4,3,1 4,5,2,1 5,1,1 5,1,1 6,1,1 7,1,1 8,1,2 8,4 9,1,1 10,1,1 11,2,2 12,2,2 14,2,2 15,1,1 15,1,1 16,1,1 2,10,5,1,2 2,17,1,2 2,12,3,2,1 2,8,8,1,2 3,8,4,2,1,1 3,18,2,1 4,13,2,2,1 27,1,2 3,13,3,1,2 3,19,1,2 3,13,3,1,1 3,19,3 4,12,3,1,1 3,18,1,2 2,11,3,1,1 15,2,1 8,2,1,1 6,1,1 4,1,1");
    }
}
