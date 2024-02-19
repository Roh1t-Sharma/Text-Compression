using System.Text;
using System.Collections.Generic;
using System.Linq;

namespace Coursework
{
    public delegate string CompressionDelegate(string text, Dictionary<string, int> dictionary);
    public delegate string DecompressionDelegate(string compressedText, Dictionary<string, int> dictionary);

    public interface ICompressor
    {
        string Compress(string text, Dictionary<string, int> dictionary);
    }

    public interface IDecompressor
    {
        string Decompress(string compressedText, Dictionary<string, int> dictionary);
    }

    public class Compressor : ICompressor
    {
        public string Compress(string text, Dictionary<string, int> dictionary)
        {
            var compressed = new StringBuilder();
            var currentString = string.Empty;

            foreach (var c in text)
            {
                var currentChar = c.ToString();

                if (dictionary.ContainsKey(currentString + currentChar))
                {
                    currentString += currentChar;
                }
                else
                {
                    compressed.Append(dictionary[currentString] + " ");
                    dictionary.Add(currentString + currentChar, dictionary.Count);
                    currentString = currentChar;
                }
            }

            if (!string.IsNullOrEmpty(currentString))
            {
                compressed.Append(dictionary[currentString]);
            }

            return compressed.ToString();
        }
    }

    public class Decompressor : IDecompressor
    {
        public string Decompress(string compressedText, Dictionary<string, int> dictionary)
        {
            var decompressed = new StringBuilder();
            var reverseLookup = new List<string>(dictionary.Keys);

            var entries = compressedText.Split(' ');

            var currentEntry = reverseLookup[int.Parse(entries[0])];
            decompressed.Append(currentEntry);

            foreach (var entry in entries.Skip(1))
            {
                var currentCode = int.Parse(entry);
                string newEntry;

                if (dictionary.ContainsKey(currentEntry + currentCode))
                {
                    newEntry = reverseLookup[dictionary[currentEntry + currentCode]];
                }
                else
                {
                    newEntry = reverseLookup[currentCode];
                    var newIndex = dictionary.Count;
                    dictionary.Add(currentEntry + currentCode, newIndex);
                    reverseLookup.Insert(newIndex, currentEntry + newEntry[0]);
                }

                decompressed.Append(newEntry);
                currentEntry = entry;
            }

            return decompressed.ToString();
        }
    }

    public static class HigherOrderAlphabets
    {
        public static Dictionary<string, int> BuildDictionary(string text)
        {
            var dictionary = new Dictionary<string, int>();
            var index = 0;

            foreach (var c in text)
            {
                var currentChar = c.ToString();
                if (!dictionary.ContainsKey(currentChar))
                {
                    dictionary.Add(currentChar, index++);
                }
            }

            return dictionary;
        }

        public static string CompressText(string text, Dictionary<string, int> dictionary)
        {
            var compressor = new Compressor();
            return compressor.Compress(text, dictionary);
        }

        // Delegate for decompression operation
        public static string DecompressText(string compressedText, Dictionary<string, int> dictionary)
        {
            var decompressor = new Decompressor();
            return decompressor.Decompress(compressedText, dictionary);
        }
    }
}