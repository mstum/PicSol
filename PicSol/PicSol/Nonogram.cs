using System;
using System.Diagnostics;

namespace PicSol
{
    /// <summary>
    /// The Nonogram to solve, given Board Size and Hints.
    /// </summary>
    [DebuggerDisplay("{Name} ({RowCount}x{ColumnCount})")]
    public class Nonogram
    {
        public string Name { get; }

        public int RowCount { get; }

        public int ColumnCount { get; }

        public HintCollection RowHints { get; }

        public HintCollection ColumnHints { get; }

        public Nonogram(string name, int rowCount, int colCount, string rowHintString, string colHintString) : this(name, rowCount, colCount, HintCollection.FromString(rowHintString), HintCollection.FromString(colHintString)) { }

        public Nonogram(string name, int rowCount, int colCount, HintCollection rowHints, HintCollection columnHints)
        {
            Name = name ?? string.Empty;
            RowCount = rowCount;
            ColumnCount = colCount;
            RowHints = rowHints ?? throw new ArgumentNullException(nameof(rowHints));
            ColumnHints = columnHints ?? throw new ArgumentNullException(nameof(columnHints));

            if (RowHints.Count != RowCount)
            {
                throw new ArgumentException($"Expected hints for {RowCount} rows, but got {RowHints.Count} instead.");
            }

            if (ColumnHints.Count != ColumnCount)
            {
                throw new ArgumentException($"Expected hints for {ColumnCount} columns, but got {ColumnHints.Count} instead.");
            }
        }
    }
}
