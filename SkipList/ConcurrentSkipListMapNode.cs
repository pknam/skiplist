using System;

namespace SkipList
{
    internal class ConcurrentSkipListMapNode<TKey, TValue>
    {
        public TKey Key { get; set; }
        public TValue Value { get; set; }
        public ConcurrentSkipListMapNode<TKey, TValue>[] Forwards { get; set; }

        public ConcurrentSkipListMapNode(Int32 forwardLength)
        {
            Forwards = new ConcurrentSkipListMapNode<TKey, TValue>[forwardLength];
        }
    }
}
