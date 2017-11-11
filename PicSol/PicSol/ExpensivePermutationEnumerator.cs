using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;

namespace PicSol
{
    internal struct ExpensivePermutationState
    {
        public BitArray Permutation;
        public int Index;

        public ExpensivePermutationState(BitArray ba, int ix)
        {
            Permutation = ba ?? throw new ArgumentNullException(nameof(ba));
            Index = ix;
        }
    }

    /// <summary>
    /// This is a recursive Enumerator, that is, it contains an _innerEnumerator which can contain an _innerEnumerator which can contain an _innerEnumerator...
    /// </summary>
    internal struct ExpensivePermutationEnumerator : IEnumerable<ExpensivePermutationState>, IEnumerator<ExpensivePermutationState>
    {
        internal static IEnumerable<ExpensivePermutationState> CreateEnumerable(int[] hintData, int currentIx, int lengthPerPermutation, int falseCount) 
            => new ExpensivePermutationEnumerator(State.NotInitialized, hintData, currentIx, lengthPerPermutation, falseCount);

        private State _state;
        private ExpensivePermutationState _current;
        private int _initialThreadId;

        private int[] _hintData;
        private int _currentIx;
        private int _lengthPerPermutation;
        private int _falseCount;

        private int _hdl;
        private int _rightFreeSpace;
        // TODO: Can I do without the inner enumerator? (So make this non-recursive)
        private IEnumerator<ExpensivePermutationState> _innerEnumerator;

        private ExpensivePermutationEnumerator(State state, int[] hintData, int currentIx, int lengthPerPermutation, int falseCount)
        {
            _state = state;
            _initialThreadId = Thread.CurrentThread.ManagedThreadId;

            _current = default(ExpensivePermutationState);
            _hintData = hintData;
            _currentIx = currentIx;
            _lengthPerPermutation = lengthPerPermutation;
            _falseCount = falseCount;
            _hdl = 0;
            _rightFreeSpace = 0;
            _innerEnumerator = null;
        }

        public bool MoveNext()
        {
            if (_state == State.CreatedEmptyBitArray)
            {
                // This terminates an Inner Enumerator, it won't be reached on the outermost enumerator
                return false;
            }
            else if (_state == State.InnerEnumeratorAdvancedSuccessfully)
            {
                return InnerEnumeratorLoop();
            }
            else if (_state == State.StartNewEnumeration)
            {
                _hdl = _hintData.Length - _currentIx;
                if (_hdl == 0)
                {
                    _current = new ExpensivePermutationState(new BitArray(_lengthPerPermutation, false), _lengthPerPermutation - _falseCount);
                    _state = State.CreatedEmptyBitArray;
                    return true; // Must return true so that the caller actually fetches the _current BitArray
                }
                _rightFreeSpace = 1;
                return InnerEnumeratorLoop();
            }
            else
            {
                return false;
            }
        }

        private bool InnerEnumeratorLoop()
        {
            if (_innerEnumerator == null)
            {
                if (!ReinitializeInnerEnumerator())
                {
                    return false;
                }
            }

            while (true)
            {
                if (TryAdvancingInnerEnumerator())
                {
                    return true;
                }
                if (!ReinitializeInnerEnumerator())
                {
                    return false;
                }
            }
        }

        private bool ReinitializeInnerEnumerator()
        {
            if (_rightFreeSpace >= _falseCount - _hdl + 2)
            {
                return false;
            }
            _innerEnumerator = CreateEnumerable(_hintData, _currentIx + 1, _lengthPerPermutation, _falseCount - _rightFreeSpace).GetEnumerator();
            _state = State.InnerEnumeratorAdvancedSuccessfully;
            return true;
        }

        private bool TryAdvancingInnerEnumerator()
        {
            if (_innerEnumerator.MoveNext())
            {
                var bac = _innerEnumerator.Current;
                var hd = _hintData[_currentIx];
                var count = bac.Index - hd;
                bac.Permutation.SetBits(count, hd, true);
                count = count - _rightFreeSpace;
                _current = new ExpensivePermutationState(bac.Permutation, count);
                _state = State.InnerEnumeratorAdvancedSuccessfully;
                return true;
            }

            _innerEnumerator = null;
            _rightFreeSpace = _rightFreeSpace + 1;
            return false;
        }

        IEnumerator<ExpensivePermutationState> IEnumerable<ExpensivePermutationState>.GetEnumerator()
        {
            // TODO: Does the Thread ID really matter here?
            if (_state == State.NotInitialized && _initialThreadId == Thread.CurrentThread.ManagedThreadId)
            {
                _state = State.StartNewEnumeration;
                return this;
            }
            else
            {
                return new ExpensivePermutationEnumerator(State.StartNewEnumeration, _hintData, _currentIx, _lengthPerPermutation, _falseCount);
            }
        }

        // Nothing to Dispose.
        void IDisposable.Dispose() { }

        IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable<ExpensivePermutationState>)this).GetEnumerator();
        ExpensivePermutationState IEnumerator<ExpensivePermutationState>.Current => _current;
        object IEnumerator.Current => _current;
        void IEnumerator.Reset() => throw new NotSupportedException();

        public enum State
        {
            /// <summary>
            /// GetEnumerator() hasn't been called yet
            /// </summary>
            NotInitialized = 0,

            /// <summary>
            /// Ready to start a new round.
            /// </summary>
            StartNewEnumeration,

            /// <summary>
            /// The innermost _innerEnumerator was reached.
            /// This enumerator is the one that actually creates each BitArray, and then exits.
            /// </summary>
            CreatedEmptyBitArray,

            /// <summary>
            /// The last time we tried to call _innerEnumerator.MoveNext(), we succeeded.
            /// So let's try to MoveNext() again.
            /// </summary>
            InnerEnumeratorAdvancedSuccessfully,
        }
    }
}
