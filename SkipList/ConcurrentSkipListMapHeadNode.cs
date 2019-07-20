using System;

namespace SkipList
{
    class ConcurrentSkipListMapHeadNode : IConcurrentSkipListMapNode
    {
        public ConcurrentSkipListMapNode[] Forwards { get; set; }

        public ConcurrentSkipListMapHeadNode(Int32 forwardSize)
        {
            Forwards = new ConcurrentSkipListMapNode[forwardSize];
        }
    }
}
