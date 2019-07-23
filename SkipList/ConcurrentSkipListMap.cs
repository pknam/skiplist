using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

namespace SkipList
{
    // todo: support concurrency
    public class ConcurrentSkipListMap<TKey, TValue> : IEnumerable<KeyValuePair<TKey, TValue>>, IEnumerable, ICollection<KeyValuePair<TKey, TValue>>
    {
        public static readonly Int32 MAX_FORWARD_LENGTH = 20;
        private readonly Double _p;
        private readonly Random _random;
        private readonly ConcurrentSkipListMapHeadNode<TKey, TValue> _head;
        private readonly IComparer<TKey> _comparer;
        private Int32 _count = 0;

        public ConcurrentSkipListMap()
            : this((IComparer<TKey>)null)
        {

        }

        public ConcurrentSkipListMap(IComparer<TKey> comparer, Double p = 0.5)
        {
            _p = p;
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
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }

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
            return RemoveInternal(key, false, default);
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

        private bool RemoveInternal(TKey key, Boolean matchValue, TValue value)
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
                if (_comparer.Compare(traverseNode.Key, key) == 0)
                {
                    if (matchValue && EqualityComparer<TValue>.Default.Equals(traverseNode.Value, value) == false)
                    {
                        return false;
                    }

                    found = true;
                    break;
                }
                else if (_comparer.Compare(key, traverseNode.Key) < 0)
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

        #region Collection
        public Boolean IsReadOnly => false;

        public Int32 Count
        {
            get
            {
                Debug.Assert(0 <= _count);
                return _count;
            }
        }

        public void Add(KeyValuePair<TKey, TValue> item)
        {
            Add(item.Key, item.Value);
        }

        public void Clear()
        {
            for (var i = 0; i < _head.Forwards.Length; i++)
            {
                _head.Forwards[i] = null;
            }

            _count = 0;
        }

        public bool Contains(KeyValuePair<TKey, TValue> item)
        {
            return
                TryGetValue(item.Key, out var value) &&
                EqualityComparer<TValue>.Default.Equals(value, item.Value);
        }

        public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
        {
            if (array == null)
            {
                throw new ArgumentNullException(nameof(array));
            }

            if (arrayIndex < 0 || array.Length <= arrayIndex)
            {
                throw new ArgumentOutOfRangeException(nameof(arrayIndex));
            }

            if (array.Length - arrayIndex < _count)
            {
                throw new ArgumentException();
            }

            if (array.Rank != 1)
            {
                throw new ArgumentException(nameof(array));
            }

            foreach (var node in this)
            {
                array[arrayIndex++] = new KeyValuePair<TKey, TValue>(node.Key, node.Value);
            }
        }

        public bool Remove(KeyValuePair<TKey, TValue> item)
        {
            return RemoveInternal(item.Key, true, item.Value);
        }
        #endregion

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
