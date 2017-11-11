using System.Collections;
using System.Collections.Generic;

namespace PicSol
{
    /// <summary>
    /// A container for all possible permutations of a row or column
    /// 
    /// For example, a hint of "1,1" in a row or column that is 4 spaces wide can have several possible permutations:
    /// ■ ∙ ■ ∙
    /// ■ ∙ ∙ ■
    /// ∙ ■ ∙ ■
    /// </summary>
    public class PermutationsCollection
    {
        private List<BitArray>[] _arr;

        public PermutationsCollection(int capacity)
        {
            _arr = new List<BitArray>[capacity];
        }

        internal int Count => _arr.Length;

        internal int GetTotalPermutationCount()
        {
            var result = 0;
            foreach(var list in _arr)
            {
                if (list != null)
                {
                    result += list.Count;
                }
            }
            return result;
        }

        internal void Add(List<BitArray> list, int atIndex)
        {
            _arr[atIndex] = list;
        }

        internal List<BitArray> this[int index]
        {
            get
            {
                if (_arr[index] == null)
                {
                    _arr[index] = new List<BitArray>();
                }
                return _arr[index];
            }
        }

        public IEnumerable<List<BitArray>> Enumerate()
        {
            foreach (var list in _arr)
            {
                yield return list;
            }
        }
    }
}