using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SkipList.Test
{
    [TestClass]
    public class CollectionTest
    {
        [TestMethod]
        public void IsReadOnly_Success()
        {
            var skipList = new ConcurrentSkipListMap<Int32, Int32>();
            Assert.IsFalse(skipList.IsReadOnly);
        }

        [TestMethod]
        public void Clear_Success()
        {
            var skipList = new ConcurrentSkipListMap<Int32, Int32>
            {
                { 3, 5 },
                { 6, 7 }
            };
            Assert.AreEqual(2, skipList.Count);

            skipList.Clear();
            Assert.AreEqual(0, skipList.Count);
            Assert.AreEqual(0, skipList.AsEnumerable().Count());
        }

        [TestMethod]
        public void Contains_Success()
        {
            var skipList = new ConcurrentSkipListMap<Int32, Int32>
            {
                { 2, 10 },
                { 234, 3 }
            };

            Assert.AreEqual(2, skipList.Count);
            Assert.IsTrue(skipList.ContainsKey(234));
            Assert.IsTrue(skipList.Contains(new KeyValuePair<int, int>(234, 3)));
            Assert.IsFalse(skipList.Contains(new KeyValuePair<int, int>(234, 1)));
            Assert.IsFalse(skipList.Contains(new KeyValuePair<int, int>(50, 50)));
        }
    }
}
