using System;
using System.Collections.Generic;
using System.Linq;
using InlineTest.Model.Collections;
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
            var result = FromArray(items);

            bool sequenceEqual = result.SequenceEqual(items.OrderByDescending(x => x));
            Assert.IsTrue(sequenceEqual);
        }

        [TestMethod]
        public void Clear()
        {
            int[] items = { 4, 1, 7, 2, 3 };
            var result = FromArray(items);
            result.Clear();

            bool sequenceEqual = result.SequenceEqual(Enumerable.Empty<int>());
            Assert.IsTrue(sequenceEqual);
        }

        [TestMethod]
        public void TestMultiple()
        {
            KeyValuePair<char, int>[] items =
            {
                new KeyValuePair<char, int>('C', 1032508),
                new KeyValuePair<char, int>('E', 1609137),
                new KeyValuePair<char, int>('D', 1236174),
                new KeyValuePair<char, int>('_', 568439),
                new KeyValuePair<char, int>('\\', 287371),
                new KeyValuePair<char, int>('[', 1006805),
                new KeyValuePair<char, int>('A', 680143),
                new KeyValuePair<char, int>('L', 155975),
                new KeyValuePair<char, int>('I', 974892),
                new KeyValuePair<char, int>('F', 1197310),
                new KeyValuePair<char, int>('M', 1201940),
                new KeyValuePair<char, int>('B', 1820738),
                new KeyValuePair<char, int>('N', 640575),
                new KeyValuePair<char, int>('S', 1221010),
                new KeyValuePair<char, int>('R', 926485),
                new KeyValuePair<char, int>('U', 1742070),
                new KeyValuePair<char, int>('P', 602809),
                new KeyValuePair<char, int>('X', 886691),
                new KeyValuePair<char, int>('Y', 3020863),
                new KeyValuePair<char, int>('W', 1091417),
                new KeyValuePair<char, int>('Z', 834877),
                new KeyValuePair<char, int>('V', 82777),
                new KeyValuePair<char, int>('H', 920902),
                new KeyValuePair<char, int>('O', 288008),
                new KeyValuePair<char, int>('G', 616626)
            };

            var result = new LimitedSizeSortedList<KeyValuePair<char, int>>(FrequencyComparer.Instance, 5);
            foreach (var pair in items)
            {
                result.Add(pair);
            }
            var expected = items.OrderByDescending(x => x.Value).Take(5).ToArray();

            bool sequenceEqual = result.SequenceEqual(expected);
            Assert.IsTrue(sequenceEqual);
        }


        private static LimitedSizeSortedList<T> FromArray<T>(T[] items) where T: IComparable<T>
        {
            var list1 = XLimitedSizeSortedList.FromComparable<T>(items.Length);
            foreach (T item in items)
            {
                list1.Add(item);
            }
            return list1;
        }
    }
}
