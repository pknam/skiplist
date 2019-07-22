using System;

namespace SkipList
{
    // todo: implement KeyValuePair<TKey, TValue>
    internal class ConcurrentSkipListMapNode
    {
        public Int32 Key { get; set; }
        public Int32 Value { get; set; }
        public ConcurrentSkipListMapNode[] Forwards { get; set; }

        public ConcurrentSkipListMapNode(Int32 forwardLength)
        {
            Forwards = new ConcurrentSkipListMapNode[forwardLength];
        }
    }
}
