using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace SkipList.Test
{
    [TestClass]
    public class MethodTest
    {
        [TestMethod]
        public void Add_Success()
        {
            var skipList = new ConcurrentSkipListMap();
            AddItems_0_to_1000(skipList);
        }

        [TestMethod]
        public void GetAllItems_Success()
        {
            var skipList = new ConcurrentSkipListMap();
            AddItems_0_to_1000(skipList);

            Int32 value;
            for (var i = 0; i < 1000; i++)
            {
                Assert.IsTrue(skipList.TryGetValue(i, out value));
                Assert.AreEqual(i + 1, value);
            }
        }

        [TestMethod]
        public void GetNotExists_Fail()
        {
            var skipList = new ConcurrentSkipListMap();
            AddItems_0_to_1000(skipList);

            Assert.IsFalse(skipList.TryGetValue(1234, out var value));
        }

        [TestMethod]
        public void DuplicateKey_Exception()
        {
            var skipList = new ConcurrentSkipListMap();
            skipList.Add(1, 1);

            Assert.ThrowsException<ArgumentException>(() =>
            {
                skipList.Add(1, 1);
            });
        }

        [TestMethod]
        public void ContainsKey_Success()
        {
            var skipList = new ConcurrentSkipListMap();
            AddItems_0_to_1000(skipList);

            Assert.IsTrue(skipList.ContainsKey(10));
            Assert.IsFalse(skipList.ContainsKey(1123123));
        }

        private void AddItems_0_to_1000(ConcurrentSkipListMap skipList)
        {
            for (var i = 0; i < 1000; i += 2)
            {
                skipList.Add(i, i + 1);
            }

            for (var i = 1; i < 1000; i += 2)
            {
                skipList.Add(i, i + 1);
            }
        }
    }
}
