using System.Collections.Generic;

namespace InlineTest.Model.Collections
{
    public class FrequencyComparer : IComparer<KeyValuePair<char, int>>, IEqualityComparer<KeyValuePair<char, int>>
    {
        public int Compare(KeyValuePair<char, int> x, KeyValuePair<char, int> y)
        {
            return x.Value.CompareTo(y.Value);
        }

        public bool Equals(KeyValuePair<char, int> x, KeyValuePair<char, int> y)
        {
            return x.Key == y.Key;
        }

        public int GetHashCode(KeyValuePair<char, int> obj)
        {
            return obj.Key.GetHashCode();
        }

        public static FrequencyComparer Instance { get; } = new FrequencyComparer();
    }
}