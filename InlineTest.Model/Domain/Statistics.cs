using System.Collections.Generic;

namespace InlineTest.Model.Domain
{
    internal class Statistics<T> : Dictionary<T, int>
    {
        public Statistics()
        {
            
        }

        public Statistics(IEnumerable<T> source)
        {
            foreach (T value in source)
            {
                if (!ContainsKey(value))
                    Add(value, 1);
                else
                    this[value]++;
            }
        }

        public void Add(Statistics<T> statistics)
        {
            foreach (var pair in statistics)
            {
                if (!ContainsKey(pair.Key))
                    Add(pair.Key, pair.Value);
                else
                    this[pair.Key] += pair.Value;
            }
        }

        public void Remove(Statistics<T> statistics)
        {
            foreach (var pair in statistics)
            {
                this[pair.Key] -= pair.Value;
            }
        }
    }
}
