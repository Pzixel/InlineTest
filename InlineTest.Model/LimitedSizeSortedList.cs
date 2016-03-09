using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Contracts;

namespace InlineTest.Model
{
    public class LimitedSizeSortedList<T> : IEnumerable<T> where T : IComparable<T>
    {
        private readonly T[] _items;
        public int Size => _items.Length;
        public int Count { get; private set; }

        public LimitedSizeSortedList(int size) : this(new T[size])
        {
        }

        private LimitedSizeSortedList(T[] items)
        {
            _items = items;
            Count = items.Length;
        }

        /// <summary>
        /// Сливает два списка фиксированной длины в один список с максимальными элементами
        /// </summary>
        /// <param name="other">Другой список для слияния</param>
        /// <returns>Итоговый список</returns>
        [Pure]
        public LimitedSizeSortedList<T> MergeWith(LimitedSizeSortedList<T> other)
        {
            T[] result = new T[Math.Max(Size, other.Size)];
            int i = 0, j = 0, k = 0;
            while(i < Count && j < other.Count && k < result.Length)
            {
                bool leftIsBigger = _items[i].CompareTo(other._items[j]) > 0;
                result[k++] = leftIsBigger ? _items[i++] : other._items[j++];
            }
            while (k < result.Length && i < Count)
            {
                result[k++] = _items[i++];
            }
            while (k < result.Length && j < other.Count)
            {
                result[k++] = other._items[j++];
            }
            return new LimitedSizeSortedList<T>(result);
        }

        public void Add(T item)
        {
            if (Count < Size)
                Count++;
            int index = FindInsertIndex(item);
            for (int i = _items.Length - 1; i > index; i--)
            {
                _items[i] = _items[i - 1];
            }
            _items[index] = item;
        }

        private int FindInsertIndex(T item)
        {
            int lo = 0, hi = Count - 1;
            while (lo < hi)
            {
                int m = lo + (hi - lo)/2; // (hi + lo)/2 без переполнения
                if (_items[m].CompareTo(item) > 0) lo = m + 1;
                else hi = m - 1;
            }
            if (_items[lo].CompareTo(item) > 0) lo++;
            return lo;
        }

        public IEnumerator<T> GetEnumerator()
        {
            return ((IEnumerable<T>) _items).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _items.GetEnumerator();
        }
    }
}
