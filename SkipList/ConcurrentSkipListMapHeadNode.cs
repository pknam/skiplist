﻿using System;

namespace SkipList
{
    internal class ConcurrentSkipListMapHeadNode : ConcurrentSkipListMapNode
    {
        public new Int32 Key => throw new NotImplementedException();
        public new Int32 Value => throw new NotImplementedException();

        public ConcurrentSkipListMapHeadNode(Int32 forwardLength)
            :base(forwardLength)
        {
        }
    }
}
