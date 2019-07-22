using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;

namespace SkipList.Test
{
    [TestClass]
    public class EnumeratorTest
    {
        [TestMethod]
        public void Foreach_Success()
        {
            var skipList = new ConcurrentSkipListMap<Int32, Int32>();
            var items = new[] { 1, 5, 10, 7, 100, 54, 23, 86 };
            foreach (var item in items)
            {
                skipList.Add(item, item + 1);
            }

            var sortedItems = items.ToList();
            sortedItems.Sort();

            var index = 0;
            foreach (var skipListItem in skipList)
            {
                Assert.AreEqual(sortedItems[index], skipListItem.Key);
                Assert.AreEqual(sortedItems[index] + 1, skipListItem.Value);
                index++;
            }

            index = 0;
            foreach (var skipListValue in skipList.Select(x => x.Value))
            {
                Assert.AreEqual(sortedItems[index] + 1, skipListValue);
                index++;
            }
        }
    }
}
