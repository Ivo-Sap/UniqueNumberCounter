using System.Buffers;

namespace UniqueNumberCounterApp
{
    public class BinaryFileProcessor
    {
        private const int BufferSize = 4096;

        public (int uniqueNumbersCount, int onlyOnceNumbersCount) ProcessBinaryFile(string filePath)
        {
            if (!File.Exists(filePath))
                throw new FileNotFoundException($"File not found: {filePath}");

            var frequencyCountMap = new Dictionary<uint, int>(capacity: 1000000);

            try
            {
                ReadBinaryFile(filePath, frequencyCountMap);
            }
            catch (Exception ex)
            {
                throw new IOException("Failed to process binary file. Invalid format.", ex);
            }

            int uniqueNumbersCount = 0, onlyOnceNumbersCount = 0;
            foreach (var pair in frequencyCountMap)
            {
                if (pair.Value == 1) onlyOnceNumbersCount++;
                uniqueNumbersCount++;
            }

            return (uniqueNumbersCount, onlyOnceNumbersCount);
        }

        private void ReadBinaryFile(string filePath, Dictionary<uint, int> frequencyCountMap)
        {
            using FileStream fs = new(filePath, FileMode.Open, FileAccess.Read);
            using BinaryReader reader = new(fs);
            byte[] buffer = ArrayPool<byte>.Shared.Rent(BufferSize);

            while (fs.Position < fs.Length)
            {
                int bytesRead = fs.Read(buffer, 0, BufferSize);

                if (bytesRead % 4 != 0)
                {
                    throw new IOException("Failed to process binary file. Invalid format.");
                }

                for (int i = 0; i < bytesRead; i += 4)
                {
                    try
                    {
                        uint number = BitConverter.ToUInt32(buffer, i);
                        frequencyCountMap[number] = frequencyCountMap.TryGetValue(number, out int count) ? count + 1 : 1;
                    }
                    catch (Exception)
                    {
                        throw new IOException("Binary file contains invalid uint data.");
                    }
                }
            }

            ArrayPool<byte>.Shared.Return(buffer);
        }


        public void DisplayResults(string filePath)
        {
            var (uniqueNumbers, numbersSeenOnce) = ProcessBinaryFile(filePath);
            Console.WriteLine($"Unique numbers count is: {uniqueNumbers}");
            Console.WriteLine($"Numbers found only once count is: {numbersSeenOnce}");
        }
    }
}
