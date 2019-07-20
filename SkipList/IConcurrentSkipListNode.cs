namespace SkipList
{
    interface IConcurrentSkipListNode
    {
        ConcurrentSkipListNode[] Forwards { get; set; }
    }
}
