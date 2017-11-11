# PicSol
This is a [Nonogram](https://en.wikipedia.org/wiki/Nonogram) solver library for .net Standard 2.0/.net Framework 4.  
Nonograms are also known as "Picture Crosswords", "Picross", or "Griddlers".

# Usage
To try solving a Nonogram, you can call `Solver.Solve` with a `Nonogram`, and you get a `Solution` back.  

If you want to limit how long the solver tries to solve it (large Nonograms can take a lot of time), you can pass in a TimeSpan.

If you want to abort the solving process yourself, you can pass in a `CancellationTokenSource` and call `Cancel()` on it whenever you want.

Note that if you also set a Timeout, the Solver will call `Cancel` on the `CancellationTokenSource` - if you don't want that, pass `null` for the timeout.  

By default, the Solver tries to use all CPU Cores and pararellize work. If you don't want that, pass `false` to the useMultipleCores argument.  

## Defining a Nonogram
The `Nonogram` class has a constructor that takes the Name, the number of rows an columns, and the hints for rows and columns.
Hints are the numbers displayed on the side of the grid.   
You can either create a `HintCollection`, or pass in a string.   

If you pass a string, separate individual elements with a space, and hints for the same element with a comma.   
For columns, numbers are from top to bottom.   
For rows, numbers are from left to right.   

For example, `"3 1,1 3 4,1,2"` describes 4 elements (rows or columns).   
The first element has `3` contigous cells filled.   
The second element has two groups with `1` filled cell each, with at least one empty cell between them.   
The third element has `3` contigous cells filled.   
The fourth element has a group of `4` filled cells, then at least one empty cell, and then a second group of `1` filled cell, at least one empty cell, and a group of `2` filled cells.   

For example, some definitions and their (unsolved) Nonograms:
```cs
new Nonogram("Chair", 5, 5, "3 3 5 1,1,1 1,1,1", "5 3 5 1 3");

//        5  3  5  1  3
//       --------------      
//     3| ?  ?  ?  ?  ?
//     3| ?  ?  ?  ?  ?
//     5| ?  ?  ?  ?  ?
// 1 1 1| ?  ?  ?  ?  ?
// 1 1 1| ?  ?  ?  ?  ?


new Nonogram("Four Rows, Five Columns", 4, 5, "2 1,1 4 1", "2 1,1 1,2 2 0");

//      2  1  1  2  0
//         1  1
//     ---------------
//   2| ?  ?  ?  ?  ?
// 1 1| ?  ?  ?  ?  ?
//   4| ?  ?  ?  ?  ?
//   1| ?  ?  ?  ?  ?

// This Nonogram is invalid and unsolvable
new Nonogram("Unsolvable", 2, 2, "2 1", "1 1");

//    1 1
//   ----
// 2| ? ?
// 1| ? ?

 // This uses 2D arrays as hints, and is the same as the Four Rows, Five Columns example above.
var rowHints = new int[4][] { new int[] { 2 }, new int[] { 1, 1 }, new int[] { 4 }, new int[] { 1 } };
var colHints = new int[5][] { 
         new int[] { 2 }, new int[] { 1, 1 },  new int[] { 1, 2 }, new int[] { 2 }, new int[] { 0 }
};
new Nonogram("Four Rows, Five Columns", 4, 5, HintCollection.From2DArray(rowHints),
                                              HintCollection.From2DArray(colHints));

// You can also create a HintCollection from a string ahead of time
var rowHints = HintCollection.FromString("2 1,1 4 1");
var colHints = HintCollection.FromString("2 1,1 1,2 2 0");
new Nonogram("Four Rows, Five Columns", 4, 5, rowHints, colHints);
```

## The Solution
The `Solution` class is the result of an attempt to solve the Nonogram, whether it succeeded or not.

`Nonogram` is the Nonogram that was tried to be solved, and contains the original Row and Column Hints (the numbers on the top/side).

`SolverState` is the inner state that the Solver used. You can get some performance information here (e.g., how many Permutations were generated).

The `Result` contains the exact result, while `IsSolved` is a shortcut for `Result == SolveResult.Success`.
Possible Results:
* Success - The solver did solve the Nonogram. Hooray!
* Unsolvable - The solver ran into an unsolvable scenario - the Nonogram is invalid. (See above for an example)
* Cancelled - The solver either did take too long to solve the Nonogram (and hit the timeout), or you called Cancel on the CancellationTokenSource that you passed in.
Note that a status of Cancelled doesn't tell you whether the Nonogram was valid or not.

The `Tiles` contain the actual cells, a 2-Dimensional collection of bools where `true` means the cell is filled.   
Note that even if `IsSolved` is false, Tiles may contain data (in this case, the last internal state of the solver before it gave up).   
This can be useful to troubleshoot why a Nonogram couldn't be solved.

You can iterate over the rows and columns:
```cs
for (int row = 0; row < solution.RowCount; row++)
{
    for (int col = 0; col < solution.ColumnCount; col++)
    {
        var isFilled = solution.Tiles[row, col];
        // You now know if the cell at 0-based coordinate row/col is filled or not
    }
}
```

# Examples
For a more detailed example, check out the `PicSol.Console` and `PicSol.Tests` projects, which contain [ExampleNonograms](https://github.com/mstum/PicSol/blob/master/PicSol/PicSol.Tests/ExampleNonograms.cs) and a [Solution-to-string renderer](https://github.com/mstum/PicSol/blob/master/PicSol/PicSol.Console/SolutionRenderer.cs).

# Caveats / Known Issues
The solver is using a rather brute force method, creating a lot of data (e.g., the `SlowUnsolvable` example creates ~4.6 Million possibilities, and uses about 500 MB of memory).

However, even larger Nonograms should be solvable in a sane amount of time, however, it's recommended that you pass either a Timeout or your own CancellationTokenSource, to prevent the process from going for too long.

There are some optimizations to reduce the required memory and CPU, but Nonograms are NP-Complete, and as such there is no guaranteed way to limit how much time or memory is needed to solve it.

That said, more optimizations are always possible.

# Changelog
## 1.0.0 (2017-11-11)
* Initial Release

# License
http://mstum.mit-license.org/

The MIT License (MIT)
 
Copyright (c) 2017 Michael Stum, http://www.Stum.de &lt;opensource@stum.de&gt;  

Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.