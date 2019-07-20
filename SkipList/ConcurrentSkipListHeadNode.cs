using System;

namespace SkipList
{
    class ConcurrentSkipListHeadNode : IConcurrentSkipListNode
    {
        public ConcurrentSkipListNode[] Forwards { get; set; }

        public ConcurrentSkipListHeadNode(Int32 forwardSize)
        {
            Forwards = new ConcurrentSkipListNode[forwardSize];
        }
    }
}
