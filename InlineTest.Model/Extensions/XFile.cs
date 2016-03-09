using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;

namespace InlineTest.Model.Extensions
{
    internal static class XFile
    {
        /// <summary>
        /// �������� ��������� ���� �� ��� ���, ���� �� ������� ������
        /// </summary>
        /// <param name="path">���� �� �����</param>
        /// <param name="timeoutMs">������� ����� ��������� �������� ������ � ����� � �������������</param>
        /// <returns></returns>
        public static IEnumerable<char> ReadText(string path, int timeoutMs = 10)
        {
            const int bufferSize = 4096; 
            char[] buffer = new char[bufferSize];
            using (var sr = new StreamReader(TryGetStream(path, timeoutMs)))
            {
                while (!sr.EndOfStream)
                {
                    int read = sr.Read(buffer, 0, buffer.Length);
                    for (int i = 0; i < read; i++)
                    {
                        char c = buffer[i];
                        yield return c;
                    }
                }
            }
        }

        private static Stream TryGetStream(string path, int timeoutMs)
        {
            while (true)
            {
                try
                {
                    return new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                }
                catch (Exception ex)
                {
                    Trace.WriteLine("File still be locked: " + ex.Message);
                    Thread.Sleep(timeoutMs);   
                    // �������� ��������� ���� ������ ��������� ������ �����, �� � ��������� .Net �� ��������� ������ � ���, ����� �� ��������� ����
                    // �� ������ ������� ����������, ���� ������. � ��������� ��� ������, ��� �� �������� ��������� ����, ������� ������� ��� �� �����������
                    // � ������� �� ����� ������ ���������. ����� ������� ��� ������������ ������ ���������, ����� ���������� �����������, 
                    // ����� ����� ������� ��������� ����.
                }
            }
        }
    }
}