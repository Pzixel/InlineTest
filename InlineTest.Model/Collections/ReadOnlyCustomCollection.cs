using System;
using System.Collections;
using System.Collections.Generic;

namespace InlineTest.Model.Collections
{
    public class ReadOnlyCustomCollection<T> : ICollection<T>
    {
        private static InvalidOperationException ReadOnlyException => new InvalidOperationException("Collection is read-only");

        private readonly ICollection<T> _source;

        public ReadOnlyCustomCollection(ICollection<T> source)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));
            _source = source;
        }


        public IEnumerator<T> GetEnumerator()
        {
            return _source.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable) _source).GetEnumerator();
        }

        public void Add(T item)
        {
            throw ReadOnlyException;
        }

        public void Clear()
        {
            throw ReadOnlyException;
        }

        public bool Contains(T item)
        {
            return _source.Contains(item);
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            _source.CopyTo(array, arrayIndex);
        }

        public bool Remove(T item)
        {
            throw ReadOnlyException;
        }


        public int Count => _source.Count;

        public bool IsReadOnly => true;
    }
}
