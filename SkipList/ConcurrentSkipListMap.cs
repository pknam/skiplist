using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

namespace SkipList
{
    // todo: support concurrency
    // todo: implement IDictionary<TKey, TValue>, ICollection<KeyValuePair<TKey, TValue>>
    public class ConcurrentSkipListMap<TKey, TValue> : IEnumerable<KeyValuePair<TKey, TValue>>, IEnumerable
    {
        public static readonly Int32 MAX_FORWARD_LENGTH = 20;
        private readonly Double _p;
        private readonly Random _random;
        private readonly ConcurrentSkipListMapHeadNode<TKey, TValue> _head;
        private readonly IComparer<TKey> _comparer;

        public ConcurrentSkipListMap()
            : this((IComparer<TKey>)null)
        {

        }

        public ConcurrentSkipListMap(IComparer<TKey> comparer, Double p = 0.5)
        {
            this._p = p;
            _random = new Random(0x0d0ffFED);
            _head = new ConcurrentSkipListMapHeadNode<TKey, TValue>(MAX_FORWARD_LENGTH);

            if (comparer == null)
            {
                _comparer = Comparer<TKey>.Default;
            }
            else
            {
                _comparer = comparer;
            }
        }

        private Int32 _count = 0;
        public Int32 Count
        {
            get
            {
                Debug.Assert(0 <= _count);
                return _count;
            }
        }

        public Boolean IsReadOnly => false;

        public Boolean TryGetValue(TKey key, out TValue value)
        {
            ConcurrentSkipListMapNode<TKey, TValue> traverseNode = _head;
            var nextIndex = TraverseNextStep(traverseNode.Forwards, key);
            while (nextIndex != null)
            {
                traverseNode = traverseNode.Forwards[nextIndex.Value];

                if (_comparer.Compare(traverseNode.Key, key) == 0)
                {
                    value = traverseNode.Value;
                    return true;
                }

                nextIndex = TraverseNextStep(traverseNode.Forwards, key);
            }

            value = default;
            return false;
        }

        public void Add(TKey key, TValue value)
        {
            ConcurrentSkipListMapNode<TKey, TValue> traverseNode = _head;
            var backlook = GenerateInitialBacklook();
            var nextIndex = TraverseNextStep(_head.Forwards, key);

            while (nextIndex != null)
            {
                for (var i = nextIndex.Value; i < traverseNode.Forwards.Length; i++)
                {
                    backlook[i] = traverseNode;
                }

                traverseNode = traverseNode.Forwards[nextIndex.Value];

                if (_comparer.Compare(traverseNode.Key, key) == 0)
                {
                    throw new ArgumentException("the key already exists", nameof(key));
                }

                nextIndex = TraverseNextStep(traverseNode.Forwards, key);
            }

            for (var i = 0; i < traverseNode.Forwards.Length; i++)
            {
                backlook[i] = traverseNode;
            }

            var forwardLength = NewForwardLength();
            var newNode = new ConcurrentSkipListMapNode<TKey, TValue>(forwardLength) { Key = key, Value = value };
            for (var i = 0; i < forwardLength; i++)
            {
                var prevNode = backlook[i];
                var nextNode = prevNode?.Forwards[i];

                newNode.Forwards[i] = nextNode;
                prevNode.Forwards[i] = newNode;
            }

            _count++;
        }

        public bool Remove(TKey key)
        {
            if (_count == 0)
            {
                return false;
            }

            ConcurrentSkipListMapNode<TKey, TValue> traverseNode = _head;
            var backlook = GenerateInitialBacklook();
            var nextIndex = TraverseNextStep(traverseNode.Forwards, key);
            Boolean found = false;

            while (nextIndex != null)
            {
                for (var i = nextIndex.Value; i < traverseNode.Forwards.Length; i++)
                {
                    backlook[i] = traverseNode;
                }

                traverseNode = traverseNode.Forwards[nextIndex.Value];

                var traverseNodeKey = (traverseNode as ConcurrentSkipListMapNode<TKey, TValue>).Key;
                if (_comparer.Compare(traverseNodeKey, key) == 0)
                {
                    found = true;
                    break;
                }
                else if (_comparer.Compare(key, traverseNodeKey) < 0)
                {
                    return false;
                }

                nextIndex = TraverseNextStep(traverseNode.Forwards, key);
            }

            if (found == false)
            {
                return false;
            }

            var foundNode = traverseNode;
            var prevNode = backlook[nextIndex.Value];

            for (var i = 0; i < nextIndex.Value; i++)
            {
                backlook[i] = prevNode;
            }

            for (var i = 0; i < foundNode.Forwards.Length; i++)
            {
                backlook[i].Forwards[i] = foundNode.Forwards[i];
            }

            _count--;
            return true;
        }

        public bool ContainsKey(TKey key)
        {
            return TryGetValue(key, out var value);
        }

        private Int32? TraverseNextStep(ConcurrentSkipListMapNode<TKey, TValue>[] forwards, TKey targetKey)
        {
            if (forwards == null)
            {
                throw new ArgumentNullException();
            }

            for (var i = forwards.Length - 1; 0 <= i; i--)
            {
                if (forwards[i] == null)
                {
                    continue;
                }

                if (_comparer.Compare(forwards[i].Key, targetKey) <= 0)
                {
                    return i;
                }
            }

            return null;
        }

        private Int32 NewForwardLength()
        {
            var r = _random.NextDouble();

            for (var length = 1; length <= MAX_FORWARD_LENGTH; length++)
            {
                if (Math.Pow(_p, length) < r)
                {
                    return length;
                }
            }

            return MAX_FORWARD_LENGTH;
        }

        private ConcurrentSkipListMapNode<TKey, TValue>[] GenerateInitialBacklook()
        {
            var backlook = new ConcurrentSkipListMapNode<TKey, TValue>[_head.Forwards.Length];
            for (var i = 0; i < backlook.Length; i++)
            {
                backlook[i] = _head;
            }

            return backlook;
        }

        #region Enumerator
        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        {
            return new Enumerator(this);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return new Enumerator(this);
        }

        public struct Enumerator : IEnumerator<KeyValuePair<TKey, TValue>>, IEnumerator
        {
            private readonly ConcurrentSkipListMap<TKey, TValue> _skipListMap;
            private ConcurrentSkipListMapNode<TKey, TValue> _currentNode;
            private KeyValuePair<TKey, TValue> _current;

            internal Enumerator(ConcurrentSkipListMap<TKey, TValue> skipListMap)
            {
                _skipListMap = skipListMap;
                _currentNode = _skipListMap._head;
                _current = new KeyValuePair<TKey, TValue>();
            }

            public bool MoveNext()
            {
                if (_currentNode.Forwards[0] != null)
                {
                    _currentNode = _currentNode.Forwards[0];
                    _current = new KeyValuePair<TKey, TValue>(_currentNode.Key, _currentNode.Value);
                    return true;
                }

                _currentNode = default;
                _current = new KeyValuePair<TKey, TValue>();
                return false;
            }

            public void Reset()
            {
                _currentNode = _skipListMap._head;
                _current = new KeyValuePair<TKey, TValue>();
            }

            public KeyValuePair<TKey, TValue> Current => _current;

            object IEnumerator.Current => _current;

            public void Dispose() { }
        }
        #endregion
    }
}
