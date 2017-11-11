using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;

namespace PicSol
{
    internal struct ExpensivePermutationEnumerator : IEnumerable<BitArray>, IEnumerator<BitArray>
    {
        internal static IEnumerable<BitArray> CreateEnumerable(int[] hintData, int currentIx, int lengthPerPermutation, int falseCount)
        {
            var result = new ExpensivePermutationEnumerator(hintData, currentIx, lengthPerPermutation, falseCount);
            return result;
        }

        IEnumerator<BitArray> IEnumerable<BitArray>.GetEnumerator() => this;
        IEnumerator IEnumerable.GetEnumerator() => this;
        bool IEnumerator.MoveNext() => _innerEnumerator.MoveNext();
        void IEnumerator.Reset() => throw new NotSupportedException();
        void IDisposable.Dispose() => _innerEnumerator.Dispose();
        BitArray IEnumerator<BitArray>.Current => _innerEnumerator.Current.Permutation;
        object IEnumerator.Current => _innerEnumerator.Current.Permutation;

        private IEnumerator<ExpensivePermutationState> _innerEnumerator;

        private ExpensivePermutationEnumerator(int[] hintData, int currentIx, int lengthPerPermutation, int falseCount)
        {
            _innerEnumerator = ExpensivePermutationEnumeratorInner.CreateEnumerable(hintData, currentIx, lengthPerPermutation, falseCount).GetEnumerator();
        }

        private struct ExpensivePermutationState
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
        private struct ExpensivePermutationEnumeratorInner : IEnumerable<ExpensivePermutationState>, IEnumerator<ExpensivePermutationState>
        {
            internal static IEnumerable<ExpensivePermutationState> CreateEnumerable(int[] hintData, int currentIx, int lengthPerPermutation, int falseCount)
            {
                var result = new ExpensivePermutationEnumeratorInner(State.NotInitialized, hintData, currentIx, lengthPerPermutation, falseCount);
                return result;
            }
            // TODO: Refactor this, as it's mostly a copy/pasted Compiler-generated Enumerator

            private State _state;
            private ExpensivePermutationState _current;
            private int _initialThreadId;

            private int[] _hintData;
            private int _currentIx;
            private int _lengthPerPermutation;
            private int _falseCount;

            private int _hdl;
            private int _rightFreeSpace;
            private IEnumerator<ExpensivePermutationState> _innerEnumerator;

            private ExpensivePermutationEnumeratorInner(State state, int[] hintData, int currentIx, int lengthPerPermutation, int falseCount)
            {
                _state = state;
                _initialThreadId = Thread.CurrentThread.ManagedThreadId;

                _current = default(ExpensivePermutationState);
                _hintData = hintData;
                _currentIx = currentIx;
                _lengthPerPermutation = lengthPerPermutation;
                _falseCount = falseCount;
                _hdl = default(int);
                _rightFreeSpace = default(int);
                _innerEnumerator = default(IEnumerator<ExpensivePermutationState>);
            }

            public bool MoveNext()
            {
                try
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
                        ReinitializeInnerEnumerator();
                        return InnerEnumeratorLoop();
                    }
                    else
                    {
                        return false;
                    }
                }
                catch
                {
                    ((IDisposable)this).Dispose();
                    return false;
                }
            }

            private bool InnerEnumeratorLoop()
            {
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
                    PermutationGenerator.SetBits(bac.Permutation, count, hd, true);
                    count = count - _rightFreeSpace;
                    _current = new ExpensivePermutationState(bac.Permutation, count);
                    _state = State.InnerEnumeratorAdvancedSuccessfully;
                    return true;
                }

                _innerEnumerator.Dispose();
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
                    return new ExpensivePermutationEnumeratorInner(State.StartNewEnumeration, _hintData, _currentIx, _lengthPerPermutation, _falseCount);
                }
            }

            void IDisposable.Dispose()
            {
                if (_state == State.InnerEnumeratorAdvancedSuccessfully)
                {
                    _innerEnumerator?.Dispose();
                }
            }

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
}
