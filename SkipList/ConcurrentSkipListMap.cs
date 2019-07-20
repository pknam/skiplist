using System;

namespace SkipList
{
    // todo: support concurrency
    // todo: implement IDictionary<TKey, TValue>, ICollection<KeyValuePair<TKey, TValue>>, IEnumerable<KeyValuePair<TKey, TValue>, IEnumerable
    public class ConcurrentSkipListMap
    {
        public static Int32 MAX_FORWARD_LENGTH = 20;

        private readonly Double p;
        private readonly Random random;
        private readonly ConcurrentSkipListMapHeadNode head;

        public ConcurrentSkipListMap(Double p = 0.5)
        {
            this.p = p;
            random = new Random(0x0d0ffFED);
            head = new ConcurrentSkipListMapHeadNode(MAX_FORWARD_LENGTH);
        }

        public Boolean TryGetValue(Int32 key, out Int32 value)
        {
            var initialNextIndex = TraverseNextStep(head.Forwards, key);
            if (initialNextIndex == null)
            {
                value = 0;
                return false;
            }

            var node = head.Forwards[initialNextIndex.Value];

            while (true)
            {
                if (node.Key == key)
                {
                    value = node.Value;
                    return true;
                }

                var nextIndex = TraverseNextStep(node.Forwards, key);
                if (nextIndex == null)
                {
                    value = 0;
                    return false;
                }

                node = node.Forwards[nextIndex.Value];
            }
        }

        public void Add(Int32 key, Int32 value)
        {
            var forwardLength = NewForwardLength();
            var newNode = new ConcurrentSkipListMapNode(forwardLength) { Key = key, Value = value };
            var backlook = GenerateInitialBacklook();

            var nextIndex = TraverseNextStep(head.Forwards, key);
            IConcurrentSkipListMapNode traverseNode = head;

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
            var r = random.NextDouble();

            for (var length = 1; length <= MAX_FORWARD_LENGTH; length++)
            {
                if (Math.Pow(p, length) < r)
                {
                    Console.WriteLine(length);
                    return length;
                }
            }

            return MAX_FORWARD_LENGTH;
        }

        private IConcurrentSkipListMapNode[] GenerateInitialBacklook()
        {
            var backlook = new IConcurrentSkipListMapNode[head.Forwards.Length];
            for (var i = 0; i < backlook.Length; i++)
            {
                backlook[i] = head;
            }

            return backlook;
        }
    }
}
