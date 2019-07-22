using System;
using System.Diagnostics;

namespace SkipList
{
    // todo: support concurrency
    // todo: implement IDictionary<TKey, TValue>, ICollection<KeyValuePair<TKey, TValue>>, IEnumerable<KeyValuePair<TKey, TValue>, IEnumerable
    public class ConcurrentSkipListMap
    {
        public static readonly Int32 MAX_FORWARD_LENGTH = 20;
        private readonly Double _p;
        private readonly Random _random;
        private readonly ConcurrentSkipListMapHeadNode _head;

        public ConcurrentSkipListMap(Double p = 0.5)
        {
            this._p = p;
            _random = new Random(0x0d0ffFED);
            _head = new ConcurrentSkipListMapHeadNode(MAX_FORWARD_LENGTH);
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

        public Boolean TryGetValue(Int32 key, out Int32 value)
        {
            IConcurrentSkipListMapNode traverseNode = _head;
            var nextIndex = TraverseNextStep(traverseNode.Forwards, key);
            while (nextIndex != null)
            {
                traverseNode = traverseNode.Forwards[nextIndex.Value];

                if (traverseNode is ConcurrentSkipListMapNode node && node.Key == key)
                {
                    value = node.Value;
                    return true;
                }

                nextIndex = TraverseNextStep(traverseNode.Forwards, key);
            }

            value = 0;
            return false;
        }

        public void Add(Int32 key, Int32 value)
        {
            var forwardLength = NewForwardLength();
            var newNode = new ConcurrentSkipListMapNode(forwardLength) { Key = key, Value = value };
            var backlook = GenerateInitialBacklook();

            var nextIndex = TraverseNextStep(_head.Forwards, key);
            IConcurrentSkipListMapNode traverseNode = _head;

            while (nextIndex != null)
            {
                for (var i = nextIndex.Value; i < traverseNode.Forwards.Length; i++)
                {
                    backlook[i] = traverseNode;
                }

                traverseNode = traverseNode.Forwards[nextIndex.Value];

                if ((traverseNode as ConcurrentSkipListMapNode).Key == key)
                {
                    throw new ArgumentException("the key already exists", nameof(key));
                }

                nextIndex = TraverseNextStep(traverseNode.Forwards, key);
            }

            for (var i = 0; i < traverseNode.Forwards.Length; i++)
            {
                backlook[i] = traverseNode;
            }

            for (var i = 0; i < forwardLength; i++)
            {
                var prevNode = backlook[i];
                var nextNode = prevNode?.Forwards[i];

                newNode.Forwards[i] = nextNode;
                prevNode.Forwards[i] = newNode;
            }

            _count++;
        }

        public bool Remove(Int32 key)
        {
            // todo
            return true;
        }

        public bool ContainsKey(Int32 key)
        {
            return TryGetValue(key, out var value);
        }

        private Int32? TraverseNextStep(ConcurrentSkipListMapNode[] forwards, Int32 targetKey)
        {
            if (forwards == null)
            {
                throw new ArgumentNullException();
            }

            for (var i = forwards.Length - 1; 0 <= i; i--)
            {
                if (forwards[i]?.Key <= targetKey)
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
                    Console.WriteLine(length);
                    return length;
                }
            }

            return MAX_FORWARD_LENGTH;
        }

        private IConcurrentSkipListMapNode[] GenerateInitialBacklook()
        {
            var backlook = new IConcurrentSkipListMapNode[_head.Forwards.Length];
            for (var i = 0; i < backlook.Length; i++)
            {
                backlook[i] = _head;
            }

            return backlook;
        }
    }
}
