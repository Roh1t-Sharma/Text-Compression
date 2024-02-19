using System.Text;

namespace Coursework
{
    public interface IBinaryCompressor
    {
        Dictionary<char, string> BuildDictionary(string originalText);
        byte[] Compress(string text, Dictionary<char, string> dict);
        string Decompress(byte[] compressed, Dictionary<char, string> dict);
    }

    public class BinaryCompressor : IBinaryCompressor
    {
        public char Symbol { get; set; }
        public int Frequency { get; set; }
        public BinaryCompressor? Right { get; set; }
        public BinaryCompressor? Left { get; set; }

        public List<bool>? Traverse(char symbol, List<bool> data)
        {
            if (Right == null && Left == null)
            {
                return symbol.Equals(Symbol) ? data : null;
            }

            var left = Left?.Traverse(symbol, new List<bool>(data) { false });
            var right = Right?.Traverse(symbol, new List<bool>(data) { true });

            return left ?? right;
        }

        public Dictionary<char, string> BuildDictionary(string originalText)
        {
            var frequencies = originalText.GroupBy(c => c).ToDictionary(g => g.Key, g => g.Count());
            var nodes = frequencies.Select(frequency => new BinaryCompressor { Symbol = frequency.Key, Frequency = frequency.Value }).ToList();

            while (nodes.Count > 1)
            {
                var orderedNodes = nodes.OrderBy(node => node.Frequency).Take(2).ToList();
                var parent = new BinaryCompressor
                {
                    Symbol = '*',
                    Frequency = orderedNodes[0].Frequency + orderedNodes[1].Frequency,
                    Left = orderedNodes[0],
                    Right = orderedNodes[1]
                };

                nodes.Add(parent);
                nodes.RemoveAll(node => orderedNodes.Contains(node));
            }

            var root = nodes.FirstOrDefault();
            return frequencies.Keys.Select(symbol => new
            {
                Symbol = symbol,
                Code = string.Join("", root.Traverse(symbol, new List<bool>()).Select(b => b ? "1" : "0"))
            })
            .OrderBy(item => item.Code.Length).ThenBy(item => item.Code)
            .ToDictionary(item => item.Symbol, item => item.Code);
        }

        public byte[] Compress(string text, Dictionary<char, string> dict)
        {
            if (string.IsNullOrEmpty(text))
            {
                throw new ArgumentException("Text cannot be null or empty.");
            }

            var bitString = new StringBuilder();
            foreach (var c in text)
            {
                if (!dict.TryGetValue(c, out var bitSequence))
                {
                    throw new InvalidOperationException($"Character '{c}' not found in dictionary.");
                }
                bitString.Append(bitSequence);
            }

            int extraBits = 8 - bitString.Length % 8;
            if (extraBits > 0 && extraBits < 8)
            {
                bitString.Append(new string('0', extraBits));
            }

            var byteArray = new byte[bitString.Length / 8 + 1];
            for (int i = 0; i < byteArray.Length - 1; i++)
            {
                for (int bit = 0; bit < 8; bit++)
                {
                    if (bitString[i * 8 + bit] == '1')
                    {
                        byteArray[i] |= (byte)(1 << 7 - bit);
                    }
                }
            }

            byteArray[^1] = (byte)extraBits;
            return byteArray;
        }

        public string Decompress(byte[] compressed, Dictionary<char, string> dict)
        {
            if (compressed == null || compressed.Length == 0)
            {
                throw new ArgumentException("Compressed array cannot be null or empty.");
            }
            var bitString = new StringBuilder();
            for (int i = 0; i < compressed.Length - 1; i++)
            {
                bitString.Append(Convert.ToString(compressed[i], 2).PadLeft(8, '0'));
            }

            int extraBits = compressed[^1];
            bitString.Remove(bitString.Length - extraBits, extraBits);

            var decompressed = new StringBuilder();
            var temp = new StringBuilder();
            foreach (char bit in bitString.ToString())
            {
                temp.Append(bit);
                foreach (var pair in dict)
                {
                    if (pair.Value == temp.ToString())
                    {
                        decompressed.Append(pair.Key);
                        temp.Clear();
                        break;
                    }
                }
            }

            return decompressed.ToString();
        }

        public static double? CalculateCompressionRatio(string originalText, byte[] compressedText)
        {
            int sizeUtf16 = Encoding.Unicode.GetByteCount(originalText);
            Console.WriteLine($"Original text length: {originalText.Length}\nOriginal text byte size (UTF-16): {sizeUtf16}\n" +
                $"Compressed text byte size: {compressedText.Length}");
            return (double)sizeUtf16 / compressedText.Length;
        }

        public static double? CalculateCompressionSpeed(long elapsedTime, int processedSize)
        {
            return processedSize / (double)elapsedTime;
        }
    }
}
