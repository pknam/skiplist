using System;

namespace SkipList
{
    // todo: implement KeyValuePair<TKey, TValue>
    internal class ConcurrentSkipListNode : IConcurrentSkipListNode
    {
        public Int32 Key { get; set; }
        public Int32 Value { get; set; }
        public ConcurrentSkipListNode[] Forwards { get; set; }

        public ConcurrentSkipListNode(Int32 forwardLength)
        {
            Forwards = new ConcurrentSkipListNode[forwardLength];
        }
    }
}
