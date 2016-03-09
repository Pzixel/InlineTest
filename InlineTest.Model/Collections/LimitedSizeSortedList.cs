using System;
using System.Collections;
using System.Collections.Generic;

namespace InlineTest.Model.Collections
{
    public class LimitedSizeSortedList<T> : IEnumerable<T>
    {
        private readonly IComparer<T> _comparer;
        private readonly T[] _items;
        public int Size => _items.Length;
        public int Count { get; private set; }
        public T Minimal => _items[Count - 1];

        public LimitedSizeSortedList(IComparer<T> comparer, int size)
        {
            if (size < 1)
                throw new ArgumentException("Размер должен быть положительным числом не меньше 1!");
            _comparer = comparer;
            _items = new T[size];
        }

        public void Add(T item)
        {
            if (Count < Size)
                Count++;
            else if (IsBigger(Minimal, item)) //Если элемент меньше минимального, нет смысла искать ему место для вставки
                return;

            int index = FindInsertIndex(item);
            for (int i = _items.Length - 1; i > index; i--)
            {
                MoveToNewIndex(_items[i - 1], i);
            }
            MoveToNewIndex(item, index);
        }

        private void MoveToNewIndex(T item, int index)
        {
            _items[index] = item;
        }

        private int FindInsertIndex(T item)
        {
            int lo = 0, hi = Count - 1;
            while (lo < hi)
            {
                int m = lo + (hi - lo) / 2; // (hi + lo)/2 без переполнения
                if (IsBigger(_items[m], item))
                    lo = m + 1;
                else
                    hi = m - 1;
            }
            if (IsBigger(_items[lo], item) && lo != Count - 1)
                lo++;
            return lo;
        }

        private bool IsBigger(T x, T y)
        {
            return _comparer.Compare(x, y) > 0;
        }

        public IEnumerator<T> GetEnumerator()
        {
            for (int i = 0; i < Count; i++)
            {
                yield return _items[i];
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _items.GetEnumerator();
        }

        public void Clear()
        {
            Count = 0;
        }

        public void Update(IEnumerable<T> source)
        {
            Clear();
            foreach (T value in source)
            {
                Add(value);
            }
        }
    }
}
