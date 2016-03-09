using System.Collections.Generic;

namespace InlineTest.Model.Collections
{
    public static class XReadOnlyCustomCollection
    {
        public static ReadOnlyCustomCollection<T> AsReadOnly<T>(this ICollection<T> source)
        {
            return new ReadOnlyCustomCollection<T>(source);
        }
    }
}
