using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;

namespace InlineTest.Model.Collections
{
    public class XLimitedSizeSortedList
    {
        [Pure]
        public static LimitedSizeSortedList<T> FromComparable<T>(int size) where T : IComparable<T>
        {
            return new LimitedSizeSortedList<T>(Comparer<T>.Default, size);
        } 
    }
}
