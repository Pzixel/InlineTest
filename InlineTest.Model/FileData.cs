using System.Collections.Generic;

namespace InlineTest.Model
{
    internal class FileData
    {
        public Statistics<char> Statistics { get; }
        public List<string> Paths { get; }

        public FileData(Statistics<char> statistics, string path)
        {
            Statistics = statistics;
            Paths = new List<string> {path};
        }
    }
}
