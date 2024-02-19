using System.Diagnostics; 

namespace Coursework
{
    class Program
    {
        enum MenuOption
        {
            BinaryCompression = 1,
            LZW = 2,
            Exit = 3
        }

        static async Task Main()
        {
            while (true)
            {
                Console.WriteLine("Select an option:");
                Console.WriteLine("1 - Binary Compression");
                Console.WriteLine("2 - Alphabets of Higher order.");
                Console.WriteLine("3 - Exit");
                Console.Write("\nEnter your choice: ");

                if (!int.TryParse(Console.ReadLine(), out int choice) || !Enum.IsDefined(typeof(MenuOption), choice))
                {
                    Console.WriteLine("Incorrect input. Please enter a number from 1 to 3");
                    continue;
                }

                var selectedOption = (MenuOption)choice;
                switch (selectedOption)
                {
                    case MenuOption.BinaryCompression:
                        BinaryStart();
                        break;
                    case MenuOption.LZW:
                        await LZW_Start();
                        break;
                    case MenuOption.Exit:
                        return;
                }
            }
        }

        static async Task LZW_Start()
        {
            var compressionDelegate = HigherOrderAlphabets.CompressText;
            var decompressionDelegate = HigherOrderAlphabets.DecompressText;

            var compressor = new Compressor();
            var decompressor = new Decompressor();

            Console.WriteLine("\nEnter the original text to compress:");
            var text = Console.ReadLine() ?? string.Empty;

            if (!string.IsNullOrEmpty(text))
            {
                var dictionary = HigherOrderAlphabets.BuildDictionary(text);

                var compressed = await Task.Run(() => compressor.Compress(text, dictionary));

                Console.WriteLine("\nCompressed text: " + compressed);

                Console.WriteLine("\nDo you want to decompress the text? (y/n)");
                if (Console.ReadLine()?.ToLower() == "y")
                {
                    var decompressed = await Task.Run(() => decompressor.Decompress(compressed, dictionary));
                    Console.WriteLine("\nDecompressed text: " + decompressed + "\n");
                }
            }
        }

        static void BinaryStart()
        {
            try
            {
                Console.Write("\nEnter text to compress:\n");
                var originalText = Console.ReadLine();
                var stopwatch = new Stopwatch();

                stopwatch.Start();
                var compressor = new BinaryCompressor();
                var dict = compressor.BuildDictionary(originalText);
                var compressedText = compressor.Compress(originalText, dict);
                stopwatch.Stop();

                if (!string.IsNullOrEmpty(originalText))
                {
                    Console.WriteLine($"Compression Ratio: {BinaryCompressor.CalculateCompressionRatio(originalText, compressedText):P2}");
                    Console.WriteLine($"Work Time: {stopwatch.ElapsedMilliseconds} ms\n" +
                                      $"Compression Speed: {BinaryCompressor.CalculateCompressionSpeed(stopwatch.ElapsedMilliseconds, originalText.Length):F2} characters / ms\n");
                }

                Console.Write("Decompress? (y/n) ");
                var choice = Console.ReadLine()?.ToLower();

                if (choice == "y")
                {
                    stopwatch.Restart();
                    var decompressedText = compressor.Decompress(compressedText, dict);
                    stopwatch.Stop();
                    Console.WriteLine($"\nDecompressed text:\n{decompressedText}\n");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
            }
        }
    }
}