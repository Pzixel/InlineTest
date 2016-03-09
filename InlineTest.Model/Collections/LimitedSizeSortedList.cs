using System;
using System.Collections;
using System.Collections.Generic;

namespace InlineTest.Model.Collections
{
    public class LimitedSizeSortedList<T> : IEnumerable<T> 
    {
        private readonly IComparer<T> _comparer;
        private readonly T[] _items;
        private readonly Dictionary<T, int> _itemsIndices;  
        public int Size => _items.Length;
        public int Count { get; private set; }
        public T Minimal => _items[_items.Length - 1];

        public LimitedSizeSortedList(IComparer<T> comparer, IEqualityComparer<T> eqComparer, int size)
        {
            if (size < 1)
                throw new ArgumentException("Размер должен быть положительным числом не меньше 1!");
            _comparer = comparer;
            _items = new T[size];
           _itemsIndices = new Dictionary<T, int>(eqComparer);
        }

        public void Add(T item)
        {
            if (IsBigger(Minimal, item)) //Если элемент меньше минимального, нет смысла искать ему место для вставки
                return;
            if (Count < Size)
                Count++;
            int alreadyExistingIndex;
            if (_itemsIndices.TryGetValue(item, out alreadyExistingIndex)) //Если элемент уже был добавлен, мы заменяем его значение
            {
                _items[alreadyExistingIndex] = item;
                return;
            }

            int index = FindInsertIndex(item);
            _itemsIndices.Remove(Minimal);
            for (int i = _items.Length - 1; i > index; i--)
            {
                MoveToNewIndex(_items[i - 1], i);
            }
            MoveToNewIndex(item, index);
        }

        private void MoveToNewIndex(T item, int index)
        {
            _items[index] = item;
            _itemsIndices[item] = index;
        }

        private int FindInsertIndex(T item)
        {
            int lo = 0, hi = Count - 1;
            while (lo < hi)
            {
                int m = lo + (hi - lo)/2; // (hi + lo)/2 без переполнения
                if (IsBigger(_items[m], item))
                    lo = m + 1;
                else
                    hi = m - 1;
            }
            if (IsBigger(_items[lo], item))
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
            _itemsIndices.Clear();
        }
    }
}
