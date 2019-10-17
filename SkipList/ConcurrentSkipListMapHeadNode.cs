using System;

namespace SkipList
{
    internal class ConcurrentSkipListMapHeadNode<TKey, TValue> : ConcurrentSkipListMapNode<TKey, TValue>
    {
        public new TKey Key => throw new NotImplementedException();
        public new TValue Value => throw new NotImplementedException();

        public ConcurrentSkipListMapHeadNode(Int32 forwardLength)
            : base(forwardLength)
        {
        }
    }
}
