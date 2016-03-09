using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Cryptography;

namespace InlineTest.Model.FileSystemInterop
{
    [DebuggerDisplay("HashCode = {_hashCode}")]
    internal class FileDescriptor
    {
        private static readonly SHA256Managed Crypt = new SHA256Managed();
        private readonly int _hashCode;
        private readonly byte[] _sha256Bytes;
        public DateTime LastRead { get; }

        public FileDescriptor(string path)
        {
            LastRead = File.GetLastWriteTime(path);
            _sha256Bytes = GetHash(path);
            _hashCode = _sha256Bytes.Aggregate(0, (acc, b) => acc * 397 + b); // Можно было бы использовать отложенное вычисление, но т.к. словарь все равно будет считать, то лучше уж сразу 1 раз посчитать
        }

        protected bool Equals(FileDescriptor other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            if (_hashCode != other._hashCode)
                return false;
            for (int i = 0; i < _sha256Bytes.Length; i++)
            {
                if (_sha256Bytes[i] != other._sha256Bytes[i])
                    return false;
            }
            return true;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((FileDescriptor) obj);
        }

        public override int GetHashCode()
        {
            return _hashCode;
        }

        private static byte[] GetHash(string path)
        {
            using (var reader = File.OpenRead(path))
            {
                return Crypt.ComputeHash(reader);
            }
        }
    }
}
