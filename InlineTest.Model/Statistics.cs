using System.Collections.Generic;

namespace InlineTest.Model
{
    public class Statistics<T> : Dictionary<T, int>
    {
        internal Statistics()
        {
            
        }

        internal Statistics(IEnumerable<T> source)
        {
            foreach (T value in source)
            {
                if (!ContainsKey(value))
                    Add(value, 1);
                else
                    this[value]++;
            }
        }

        internal void Add(Statistics<T> statistics)
        {
            foreach (var pair in statistics)
            {
                if (!ContainsKey(pair.Key))
                    Add(pair.Key, pair.Value);
                else
                    this[pair.Key] += pair.Value;
            }
        }

        internal void Remove(Statistics<T> statistics)
        {
            foreach (var pair in statistics)
            {
                this[pair.Key] -= pair.Value;
            }
        }
    }
}
