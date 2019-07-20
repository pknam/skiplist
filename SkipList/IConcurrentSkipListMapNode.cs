namespace SkipList
{
    interface IConcurrentSkipListMapNode
    {
        ConcurrentSkipListMapNode[] Forwards { get; set; }
    }
}
