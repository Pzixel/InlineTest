using System;
using System.Linq;
using InlineTest.Model;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace InlineTest.UnitTests
{
    [TestClass]
    public class LimitedSizeSortedListTest
    {
        [TestMethod]
        public void Insert()
        {
            int[] items = {4, 1, 7, 2, 3};
            var result = new LimitedSizeSortedList<int>(items.Length);
            foreach (int item in items)
            {
                result.Add(item);
            }

            bool sequenceEqual = result.SequenceEqual(items.OrderByDescending(x => x));
            Assert.IsTrue(sequenceEqual);
        }

        [TestMethod]
        public void Merge1()
        {
            int[] items1 = {1, 3, 5, 7, 8};
            int[] items2 = {2, 4, 6, 9, 10};
            var list1 = FromArray(items1);
            var list2 = FromArray(items2);

            var result = list1.MergeWith(list2);
            var expected = items1.Concat(items2).OrderByDescending(x => x).Take(items1.Length).ToList();
            bool sequenceEqual = result.SequenceEqual(expected);

            Assert.AreEqual(expected.Count, result.Count);
            Assert.IsTrue(sequenceEqual);
        }

        private static LimitedSizeSortedList<int> FromArray(int[] items1)
        {
            var list1 = new LimitedSizeSortedList<int>(items1.Length);
            foreach (int item in items1)
            {
                list1.Add(item);
            }
            return list1;
        }
    }
}
