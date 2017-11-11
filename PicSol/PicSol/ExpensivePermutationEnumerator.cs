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
                var result = new ExpensivePermutationEnumeratorInner(State.MinusTwo, hintData, currentIx, lengthPerPermutation, falseCount);
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
            private ExpensivePermutationState _bac;
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
                _bac = default(ExpensivePermutationState);
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
                        case State.Zero:
                            _state = State.MinusOne;
                            _hdl = _hintData.Length - _currentIx;
                            if (_hdl == 0)
                            {
                                _current = new ExpensivePermutationState(new BitArray(_lengthPerPermutation, false), _lengthPerPermutation - _falseCount);
                                _state = State.One;
                                return true;
                            }
                            _x = 1;
                            goto label_11; // This is copy/pasted from a compiler-generated Enumerator and hasn't been refactored yet.
                        case State.One:
                            _state = State.MinusOne;
                            return false;
                        case State.Two:
                            _state = State.MinusThree;
                            _bac = new ExpensivePermutationState();
                            break;
                        default:
                            return false;
                    }

                    label_9:
                    if (_innerEnumerator.MoveNext())
                    {
                        _bac = _innerEnumerator.Current;
                        _count = _bac.Index - _hintData[_currentIx];
                        SetBits(_bac.Permutation, _count, _hintData[_currentIx], true);
                        _count = _count - _x;
                        _current = new ExpensivePermutationState(_bac.Permutation, _count);
                        _state = State.Two;
                        return true;
                    }
                    Finally();
                    _innerEnumerator = null;
                    _x = _x + 1;

                    label_11:
                    if (_x >= _falseCount - _hdl + 2)
                    {
                        return false;
                    }
                    _innerEnumerator = CreateEnumerable(_hintData, _currentIx + 1, _lengthPerPermutation, _falseCount - _x).GetEnumerator();
                    _state = State.MinusThree;
                    goto label_9;
                }
                catch
                {
                    ((IDisposable)this).Dispose();
                }
                return false;
            }

            IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable<ExpensivePermutationState>)this).GetEnumerator();

            IEnumerator<ExpensivePermutationState> IEnumerable<ExpensivePermutationState>.GetEnumerator()
            {
                ExpensivePermutationEnumeratorInner result;
                if (_state == State.MinusTwo && _initialThreadId == Thread.CurrentThread.ManagedThreadId)
                {
                    _state = State.Zero;
                    result = this;
                }
                else
                {
                    result = new ExpensivePermutationEnumeratorInner(State.Zero, _hintData, _currentIx, _lengthPerPermutation, _falseCount);
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
                _state = State.MinusOne;
                _innerEnumerator?.Dispose();
            }

            void IDisposable.Dispose()
            {
                switch (_state)
                {
                    case State.MinusThree:
                    case State.Two:
                        try { }
                        finally
                        {
                            Finally();
                        }
                        break;
                }
            }

            public enum State
            {
                // TODO: Name these States properly.
                Zero = 0,
                One = 1,
                Two = 2,
                MinusThree = -3,
                MinusTwo = -2,
                MinusOne = -1
            }

            private static void SetBits(BitArray ba, int startIx, int length, bool value)
            {
                for (int i = startIx; i < (startIx + length); i++)
                {
                    ba.Set(i, value);
                }
            }
        }
    }
}
