using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace PicSol
{
    /// <summary>
    /// The hints that are part of the gutter of the <see cref="Nonogram"/>.
    /// In other words, the numbers on the side of each row/column.
    /// </summary>
    public class HintCollection : IEnumerable<int[]>
    {
        private int[][] _hints;

        public int Count { get; }

        private HintCollection(int count)
        {
            Count = count;
            _hints = new int[count][];
        }

        public int[] this[int index]
        {
            get
            {

                var h = _hints[index] ?? new int[0];
                var result = new int[h.Length];
                h.CopyTo(result, 0);
                return result;
            }
            private set
            {
                _hints[index] = value;
            }
        }

        private void SetHintValue(int index, int innerIndex, int value)
        {
            _hints[index][innerIndex] = value;
        }

        /// <summary>
        /// Expects a string where each row/column is separated by space and each hint is separated by a comma.
        /// For example, "1 1,2 3" will result in 3 Rows or Columns, with "1", "1,2" and "3" being the hints for those.
        /// </summary>
        /// <param name="hintString"></param>
        /// <returns></returns>
        public static HintCollection FromString(string hintString)
        {
            if (hintString == null) { throw new ArgumentNullException(nameof(hintString)); }
            
            var elems = hintString.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            var coll = new HintCollection(elems.Length);
            for (int i = 0; i < elems.Length; i++)
            {
                var data = elems[i].Split(',').Select(s => int.Parse(s)).ToArray();
                coll[i] = data;
            }
            return coll;
        }

        /// <summary>
        /// Expects a nested array with the first dimension representing the rows/columns, and the second dimension representing the hints for each of those rows/columns.
        /// For example, [[1], [1,2], [3]] represents a HintCollection with 3 Rows or Columns, with "1" being the hint for the first, "1,2" for the second and "3" for the third.
        /// </summary>
        /// <param name="hints"></param>
        /// <returns></returns>
        public static HintCollection From2DArray(int[][] hints)
        {
            if (hints == null) { throw new ArgumentNullException(nameof(hints)); }

            var coll = new HintCollection(hints.Length);
            for (int i = 0; i < hints.Length; i++)
            {
                var data = hints[i];
                if (data.Length == 0)
                {
                    data = new int[] { 0 };
                }

                coll[i] = new int[data.Length];
                for (int j = 0; j < data.Length; j++)
                {
                    coll.SetHintValue(i, j, data[j]);
                }
            }
            return coll;
        }

        public IEnumerator<int[]> GetEnumerator()
        {
            return new HintCollectionEnumerator(this);
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        private struct HintCollectionEnumerator : IEnumerator<int[]>
        {
            private HintCollection _hints;
            private int _index;

            public HintCollectionEnumerator(HintCollection hints)
            {
                _hints = hints;
                _index = -1;
            }
            
            public int[] Current => _hints[_index];

            object IEnumerator.Current => Current;

            public void Dispose()
            {
            }

            public bool MoveNext()
            {
                _index++;
                return _index < _hints.Count;
            }

            public void Reset()
            {
                _index = 0;
            }
        }
    }
}
