using System.Diagnostics;
using UniqueNumberCounterApp;

namespace UniqueNumberCounterTests
{
    [Collection("SequentialTests")]
    public class BinaryFileProcessorTests
    {
        private const string LogFilePath = "performance_log.txt";

        [Fact]
        public void ProcessBinaryFile_EmptyFile()
        {
            string tempFilePath = CreateTestBinaryFile([]);

            var processor = new BinaryFileProcessor();
            var (uniqueNumbers, numbersSeenOnce) = processor.ProcessBinaryFile(tempFilePath);

            Assert.Equal(0, uniqueNumbers);
            Assert.Equal(0, numbersSeenOnce);

            CleanupTestFile(tempFilePath);
        }

        [Theory]
        [InlineData(100)]
        [InlineData(1000)]
        [InlineData(10000)]
        [InlineData(100000)]
        [InlineData(1000000)]
        [InlineData(10000000)]
        [InlineData(100000000)]
        [InlineData(1000000000)]
        public void ProcessBinaryFile_VariousDatasetSizes(int rowCount)
        {
            var stopwatch = new Stopwatch();
            var newLine = Environment.NewLine;

            stopwatch.Start();
            string tempFilePath = CreateTestBinaryFile(GenerateSequentialNumbers(rowCount));
            File.AppendAllText(LogFilePath, $"BinaryFileProcessor Temporary file path for {rowCount} is: {tempFilePath}{newLine}");
            stopwatch.Stop();
            File.AppendAllText(LogFilePath, $"BinaryFileProcessor File creation duration for {rowCount} numbers: {stopwatch.ElapsedMilliseconds} ms{newLine}");

            stopwatch.Restart();
            var processor = new BinaryFileProcessor();
            var (uniqueNumbers, numbersSeenOnce) = processor.ProcessBinaryFile(tempFilePath);
            stopwatch.Stop();
            File.AppendAllText(LogFilePath, $"BinaryFileProcessor File processing duration for {rowCount} numbers: {stopwatch.ElapsedMilliseconds} ms{newLine}");

            Assert.Equal(rowCount, uniqueNumbers);
            Assert.Equal(rowCount, numbersSeenOnce);

            CleanupTestFile(tempFilePath);
        }

        [Theory]
        [InlineData(new byte[] { 0x49, 0x44 }, "Failed to process binary file. Invalid format.")]
        public void ProcessBinaryFile_InvalidData(byte[] fileData, string expectedMessage)
        {
            string tempFilePath = CreateInvalidBinaryFile(fileData);
            var processor = new BinaryFileProcessor();

            Exception? exception = Record.Exception(() =>
            {
                processor.ProcessBinaryFile(tempFilePath);
            });

            Assert.NotNull(exception);
            Assert.IsType<IOException>(exception);
            Assert.Contains(expectedMessage, exception.Message);

            CleanupTestFile(tempFilePath);
        }

        [Fact]
        public void DisplayResults_CorrectlyOutputsCounts()
        {
            string tempFilePath = CreateTestBinaryFile([0x100, 0x100, 0x800, 0xFFF]);

            var processor = new BinaryFileProcessor();
            var output = CaptureConsoleOutput(() => processor.DisplayResults(tempFilePath));

            Assert.Contains("Unique numbers count is: 3", output);
            Assert.Contains("Numbers found only once count is: 2", output);

            CleanupTestFile(tempFilePath);
        }

        [Theory]
        [InlineData(new uint[] { }, 0, 0)] // 0 unique, 0 appears once
        [InlineData(new uint[] { 0x100, 0x100 }, 1, 0)] // 1 unique, 0 appears once
        [InlineData(new uint[] { 0x222, 0x222, 0x333, 0x333 }, 2, 0)] // 2 unique, none appear once
        [InlineData(new uint[] { 0x777, 0x888, 0x999, 0x888, 0x999, 0x777, 0x999 }, 3, 0)] // 3 mixed unique, none appear once
        [InlineData(new uint[] { 0x100 }, 1, 1)] // 1 unique, 1 appears once
        [InlineData(new uint[] { 0xAAA, 0xAAA, 0xBBB }, 2, 1)] // 2 unique numbers, 1 appearing only once
        [InlineData(new uint[] { 0x100, 0x100, 0x100, 0x200, 0x300, 0x300 }, 3, 1)] // 3 unique numbers, 1 appears once
        [InlineData(new uint[] { 0x222, 0x333 }, 2, 2)] // 2 unique, 2 appear only once
        [InlineData(new uint[] { 0x222, 0x333, 0x444 }, 3, 3)] // 3 unique, 3 appear only once
        [InlineData(new uint[] { 0xABC, 0xABC, 0x500, 0x600, 0x700 }, 4, 3)] // 4 unique, 3 appear only once
        [InlineData(new uint[] { 0x400, 0x500, 0x600, 0xBCD, 0xBCD, 0x700, 0x400, 0xBCD }, 5, 3)] // 5 mixed unique, 3 appear only once
        public void ProcessBinaryFile_DifferentDataPatterns(uint[] testData, int expectedUniqueNumbersCount, int expectedOnlyOnceNumbersCount)
        {
            string tempFilePath = CreateTestBinaryFile(testData);

            var processor = new BinaryFileProcessor();
            var (uniqueNumbersCount, singleNumbersCount) = processor.ProcessBinaryFile(tempFilePath);

            Assert.Equal(expectedUniqueNumbersCount, uniqueNumbersCount);
            Assert.Equal(expectedOnlyOnceNumbersCount, singleNumbersCount);

            CleanupTestFile(tempFilePath);
        }
        private static string CreateTestBinaryFile(uint[] numbers)
        {
            string filePath = Path.GetTempFileName();

            using FileStream fs = new(filePath, FileMode.Create, FileAccess.Write);
            using BinaryWriter writer = new(fs);

            foreach (uint num in numbers)
            {
                writer.Write(num);
            }

            writer.Flush();
            fs.Flush();
            fs.Close();

            return filePath;
        }

        private static void CleanupTestFile(string filePath)
        {
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }
        }

        private static uint[] GenerateSequentialNumbers(int count)
        {
            HashSet<uint> uniqueNumbers = [];

            for (int i = 0; i < count; i++)
            {
                uniqueNumbers.Add((uint)i);
            }

            return [.. uniqueNumbers];
        }
        private static string CaptureConsoleOutput(Action action)
        {
            using var sw = new StringWriter();
            Console.SetOut(sw);
            action();
            Console.SetOut(new StreamWriter(Console.OpenStandardOutput()));
            return sw.ToString();
        }
        private static string CreateInvalidBinaryFile(byte[] data)
        {
            string filePath = Path.GetTempFileName();

            using FileStream fs = new(filePath, FileMode.Create, FileAccess.Write);
            using BinaryWriter writer = new(fs);

            writer.Write(data);
            writer.Flush();
            fs.Close();

            return filePath;
        }
    }
}
