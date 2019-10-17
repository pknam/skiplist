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
            var skipList = new ConcurrentSkipListMap<Int32, Int32>();
            AddItems_0_to_1000(skipList);
            Assert.AreEqual(1000, skipList.Count);
        }

        [TestMethod]
        public void GetAllItems_Success()
        {
            var skipList = new ConcurrentSkipListMap<Int32, Int32>();
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
            var skipList = new ConcurrentSkipListMap<Int32, Int32>();
            AddItems_0_to_1000(skipList);

            Assert.IsFalse(skipList.TryGetValue(1234, out var value));
        }

        [TestMethod]
        public void DuplicateKey_Exception()
        {
            var skipList = new ConcurrentSkipListMap<Int32, Int32>();
            skipList.Add(1, 1);

            Assert.ThrowsException<ArgumentException>(() =>
            {
                skipList.Add(1, 1);
            });
        }

        [TestMethod]
        public void ContainsKey_Success()
        {
            var skipList = new ConcurrentSkipListMap<Int32, Int32>();
            AddItems_0_to_1000(skipList);

            for (var i = 0; i < 1000; i++)
            {
                Assert.IsTrue(skipList.ContainsKey(i));
            }
        }

        [TestMethod]
        public void ContainsKey_Fail()
        {
            var skipList = new ConcurrentSkipListMap<Int32, Int32>();
            AddItems_0_to_1000(skipList);

            Assert.IsFalse(skipList.ContainsKey(-2));
            Assert.IsFalse(skipList.ContainsKey(-1));
            Assert.IsFalse(skipList.ContainsKey(1000));
            Assert.IsFalse(skipList.ContainsKey(1001));
            Assert.IsFalse(skipList.ContainsKey(1123123));
        }

        [TestMethod]
        public void RemoveBoundary_Fail()
        {
            var skipList = new ConcurrentSkipListMap<Int32, Int32>();
            AddItems_0_to_1000(skipList);

            Assert.IsFalse(skipList.Remove(-3));
            Assert.IsFalse(skipList.Remove(-2));
            Assert.IsFalse(skipList.Remove(-1));
            Assert.IsFalse(skipList.Remove(1000));
            Assert.IsFalse(skipList.Remove(1001));
            Assert.IsFalse(skipList.Remove(1002));
        }

        [TestMethod]
        public void Remove_Success()
        {
            var skipList = new ConcurrentSkipListMap<Int32, Int32>();
            AddItems_0_to_1000(skipList);

            var oldLength = skipList.Count;
            var toRemoveItems = new[] { 2, 934, 54, 19, 245, 512, 777, 13 };

            foreach (var item in toRemoveItems)
            {
                Assert.IsTrue(skipList.Remove(item));
            }

            Assert.AreEqual(oldLength - toRemoveItems.Length, skipList.Count);

            foreach (var item in toRemoveItems)
            {
                Assert.IsFalse(skipList.ContainsKey(item));
            }
        }

        [TestMethod]
        public void AddRemove_Success()
        {
            var skipList = new ConcurrentSkipListMap<Int32, Int32>();
            skipList.Add(3, 3);
            skipList.Add(1, 1);
            skipList.Add(10, 10);

            Assert.IsFalse(skipList.Remove(0));
            Assert.IsFalse(skipList.Remove(2));
            Assert.IsFalse(skipList.Remove(6));
            Assert.IsFalse(skipList.Remove(20));

            Assert.IsTrue(skipList.Remove(10));
            Assert.IsTrue(skipList.ContainsKey(1));
            Assert.IsTrue(skipList.ContainsKey(3));
            Assert.IsFalse(skipList.ContainsKey(10));
            Assert.AreEqual(2, skipList.Count);

            Assert.IsTrue(skipList.Remove(1));
            Assert.IsFalse(skipList.ContainsKey(1));
            Assert.IsTrue(skipList.ContainsKey(3));
            Assert.IsFalse(skipList.ContainsKey(10));
            Assert.AreEqual(1, skipList.Count);

            Assert.IsTrue(skipList.Remove(3));
            Assert.IsFalse(skipList.ContainsKey(1));
            Assert.IsFalse(skipList.ContainsKey(3));
            Assert.IsFalse(skipList.ContainsKey(10));
            Assert.AreEqual(0, skipList.Count);

            Assert.IsFalse(skipList.Remove(1));
        }

        private void AddItems_0_to_1000(ConcurrentSkipListMap<Int32, Int32> skipList)
        {
            for (var i = 0; i < 1000; i += 3)
            {
                skipList.Add(i, i + 1);
            }

            for (var i = 1; i < 1000; i += 3)
            {
                skipList.Add(i, i + 1);
            }

            for (var i = 2; i < 1000; i += 3)
            {
                skipList.Add(i, i + 1);
            }
        }
    }
}
