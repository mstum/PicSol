using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
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

        private struct ExpensivePermutationEnumeratorInner : IEnumerable<ExpensivePermutationState>, IEnumerator<ExpensivePermutationState>
        {
            internal static IEnumerable<ExpensivePermutationState> CreateEnumerable(int[] hintData, int currentIx, int lengthPerPermutation, int falseCount)
            {
                var result = new ExpensivePermutationEnumeratorInner(State.Initial, hintData, currentIx, lengthPerPermutation, falseCount);
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
            private int _x;
            private IEnumerator<ExpensivePermutationState> _innerEnumerator;
            private int _count;

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
                _x = default(int);
                _innerEnumerator = default(IEnumerator<ExpensivePermutationState>);
                _count = default(int);
            }

            ExpensivePermutationState IEnumerator<ExpensivePermutationState>.Current => _current;
            object IEnumerator.Current => _current;

            public bool MoveNext()
            {
                try
                {
                    switch (_state)
                    {
                        case State.StartNewEnumeration:
                            _state = State.Finished;
                            _hdl = _hintData.Length - _currentIx;
                            if (_hdl == 0)
                            {
                                _current = new ExpensivePermutationState(new BitArray(_lengthPerPermutation, false), _lengthPerPermutation - _falseCount);
                                _state = State.CreatedEmptyBitArray;
                                return true;
                            }
                            _x = 1;
                            ReinitializeInnerEnumerator();
                            return InnerEnumeratorLoop();
                        case State.CreatedEmptyBitArray:
                            _state = State.Finished;
                            return false;
                        case State.InnerEnumeratorAdvancedSuccessfully:
                            _state = State.ReinitializedInnerEnumerator;
                            break;
                        default:
                            return false;
                    }

                    return InnerEnumeratorLoop();
                }
                catch
                {
                    ((IDisposable)this).Dispose();
                }
                return false;
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
                if (_x >= _falseCount - _hdl + 2)
                {
                    return false;
                }
                _innerEnumerator = CreateEnumerable(_hintData, _currentIx + 1, _lengthPerPermutation, _falseCount - _x).GetEnumerator();
                _state = State.ReinitializedInnerEnumerator;
                return true;
            }

            private bool TryAdvancingInnerEnumerator()
            {
                if (_innerEnumerator.MoveNext())
                {
                    var bac = _innerEnumerator.Current;
                    _count = bac.Index - _hintData[_currentIx];
                    PermutationGenerator.SetBits(bac.Permutation, _count, _hintData[_currentIx], true);
                    _count = _count - _x;
                    _current = new ExpensivePermutationState(bac.Permutation, _count);
                    _state = State.InnerEnumeratorAdvancedSuccessfully;
                    return true;
                }
                Finally();
                _innerEnumerator = null;
                _x = _x + 1;
                return false;
            }

            IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable<ExpensivePermutationState>)this).GetEnumerator();

            IEnumerator<ExpensivePermutationState> IEnumerable<ExpensivePermutationState>.GetEnumerator()
            {
                ExpensivePermutationEnumeratorInner result;
                // TODO: Does the Thread ID really matter here?
                if (_state == State.Initial && _initialThreadId == Thread.CurrentThread.ManagedThreadId)
                {
                    _state = State.StartNewEnumeration;
                    result = this;
                }
                else
                {
                    result = new ExpensivePermutationEnumeratorInner(State.StartNewEnumeration, _hintData, _currentIx, _lengthPerPermutation, _falseCount);
                }

                return result;
            }

            [DebuggerHidden]
            void IEnumerator.Reset()
            {
                throw new NotSupportedException();
            }

            private void Finally()
            {
                _state = State.Finished;
                _innerEnumerator?.Dispose();
            }

            void IDisposable.Dispose()
            {
                if (_state == State.ReinitializedInnerEnumerator || _state == State.InnerEnumeratorAdvancedSuccessfully)
                {
                    try { }
                    finally
                    {
                        Finally();
                    }
                }
            }

            public enum State
            {
                ReinitializedInnerEnumerator = -3,

                Initial = -2,

                /// <summary>
                /// May be set temporarily
                /// </summary>
                Finished = -1,

                StartNewEnumeration = 0,

                CreatedEmptyBitArray = 1,

                InnerEnumeratorAdvancedSuccessfully = 2,
            }
        }
    }
}
