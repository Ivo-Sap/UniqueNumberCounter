using System.Diagnostics;
using UniqueNumberCounterApp;

namespace UniqueNumberCounterTests
{
    [Collection("SequentialTests")]
    public class ParallelBinaryFileProcessorTests
    {
        private const string LogFilePath = "performance_log.txt";

        [Fact]
        public async Task ProcessBinaryFile_EmptyFile()
        {
            string tempFilePath = CreateTestBinaryFile([]);

            var processor = new ParallelBinaryFileProcessor();
            var (uniqueNumbers, numbersSeenOnce) = await Task.Run(() => processor.ProcessBinaryFileParallel(tempFilePath));

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
        public async Task ProcessBinaryFile_VariousDatasetSizes(int rowCount)
        {
            var stopwatch = new Stopwatch();
            var newLine = Environment.NewLine;

            stopwatch.Start();
            string tempFilePath = CreateTestBinaryFile(GenerateSequentialNumbers(rowCount));
            File.AppendAllText(LogFilePath, $"ParallelBinaryFileProcessor Temporary file path for {rowCount} is: {tempFilePath}{newLine}");
            stopwatch.Stop();
            File.AppendAllText(LogFilePath, $"ParallelBinaryFileProcessor File creation duration for {rowCount} numbers: {stopwatch.ElapsedMilliseconds} ms{newLine}");

            stopwatch.Restart();
            var processor = new ParallelBinaryFileProcessor();
            var (uniqueNumbers, numbersSeenOnce) = await Task.Run(() => processor.ProcessBinaryFileParallel(tempFilePath));
            stopwatch.Stop();

            File.AppendAllText(LogFilePath, $"ParallelBinaryFileProcessor File processing duration for {rowCount} numbers: {stopwatch.ElapsedMilliseconds} ms{newLine}");

            Assert.Equal(rowCount, uniqueNumbers);
            Assert.Equal(rowCount, numbersSeenOnce);

            CleanupTestFile(tempFilePath);
        }

        [Theory]
        [InlineData(new byte[] { 0x49, 0x44 }, "Failed to process binary file. Invalid format.")]
        public async Task ProcessBinaryFile_InvalidData(byte[] fileData, string expectedMessage)
        {
            string tempFilePath = CreateInvalidBinaryFile(fileData);
            var processor = new ParallelBinaryFileProcessor();

            Exception? exception = await Task.Run(() =>
            {
                return Record.Exception(() => processor.ProcessBinaryFileParallel(tempFilePath));
            });

            Assert.NotNull(exception);
            Assert.IsType<IOException>(exception);
            Assert.Contains(expectedMessage, exception.Message);

            CleanupTestFile(tempFilePath);
        }

        [Fact]
        public async Task DisplayResults_CorrectlyOutputsCounts()
        {
            string tempFilePath = CreateTestBinaryFile([0x100, 0x100, 0x800, 0xFFF]);
            var processor = new ParallelBinaryFileProcessor();

            var output = await Task.Run(() =>
            {
                using var sw = new StringWriter();
                Console.SetOut(sw);

                processor.DisplayResults(tempFilePath);

                return sw.ToString();
            });

            Assert.Contains("Unique numbers count: 3", output);
            Assert.Contains("Numbers found only once count: 2", output);

            CleanupTestFile(tempFilePath);
        }

        [Theory]
        [InlineData(new uint[] { }, 0, 0)]
        [InlineData(new uint[] { 0x100, 0x100 }, 1, 0)]
        [InlineData(new uint[] { 0x222, 0x222, 0x333, 0x333 }, 2, 0)]
        [InlineData(new uint[] { 0x777, 0x888, 0x999, 0x888, 0x999, 0x777, 0x999 }, 3, 0)]
        [InlineData(new uint[] { 0x100 }, 1, 1)]
        [InlineData(new uint[] { 0xAAA, 0xAAA, 0xBBB }, 2, 1)]
        [InlineData(new uint[] { 0x100, 0x100, 0x100, 0x200, 0x300, 0x300 }, 3, 1)]
        [InlineData(new uint[] { 0x222, 0x333 }, 2, 2)]
        [InlineData(new uint[] { 0x222, 0x333, 0x444 }, 3, 3)]
        [InlineData(new uint[] { 0xABC, 0xABC, 0x500, 0x600, 0x700 }, 4, 3)]
        [InlineData(new uint[] { 0x400, 0x500, 0x600, 0xBCD, 0xBCD, 0x700, 0x400, 0xBCD }, 5, 3)]
        public async Task ProcessBinaryFile_DifferentDataPatterns(uint[] testData, int expectedUniqueNumbersCount, int expectedOnlyOnceNumbersCount)
        {
            string tempFilePath = CreateTestBinaryFile(testData);

            var processor = new ParallelBinaryFileProcessor();
            var (uniqueNumbersCount, singleNumbersCount) = await Task.Run(() => processor.ProcessBinaryFileParallel(tempFilePath));

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
