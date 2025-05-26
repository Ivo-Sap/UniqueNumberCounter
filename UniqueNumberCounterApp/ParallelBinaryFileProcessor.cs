using System.Collections.Concurrent;

namespace UniqueNumberCounterApp
{
    public class ParallelBinaryFileProcessor
    {
        private const int BufferSize = 65536;
        private const int ChunkSize = 100000000;

        public (int uniqueNumbersCount, int onlyOnceNumbersCount) ProcessBinaryFileParallel(string filePath)
        {
            if (!File.Exists(filePath))
                throw new FileNotFoundException($"File not found: {filePath}");

            long fileSize = new FileInfo(filePath).Length;
            const int SeekStep = 256 * 1024;

            if (fileSize == 0)
                return (0, 0);
            if (fileSize % 4 != 0)
                throw new IOException("Failed to process binary file. Invalid format.");

            int chunkCount = fileSize < ChunkSize ? 1 : (int)Math.Ceiling((double)fileSize / ChunkSize);
            var frequencyCountMap = new ConcurrentDictionary<uint, int>();

            ParallelOptions options = new() { MaxDegreeOfParallelism = Math.Max(1, Environment.ProcessorCount / 4) };

            try
            {
                Parallel.For(0, chunkCount, options, chunkIndex =>
                {
                    long chunkStart = (long)chunkIndex * ChunkSize;
                    long chunkEnd = Math.Min(chunkStart + ChunkSize, fileSize);

                    if (chunkStart < 0 || chunkEnd > fileSize)
                        throw new IOException($"Chunk {chunkIndex}: Invalid chunk range. Start={chunkStart}, End={chunkEnd}, FileSize={fileSize}");

                    try
                    {
                        using FileStream fs = new(filePath, FileMode.Open, FileAccess.Read, FileShare.Read, BufferSize, FileOptions.RandomAccess);
                        using BinaryReader reader = new(fs);

                        long currentPos = fs.Position;
                        while (currentPos < chunkStart)
                        {
                            long nextSeek = Math.Min(SeekStep, chunkStart - currentPos);
                            fs.Seek(nextSeek, SeekOrigin.Current);
                            currentPos = fs.Position;
                        }

                        if (fs.Position != chunkStart)
                            throw new IOException($"Chunk {chunkIndex}: Seek failed. Expected {chunkStart}, found {fs.Position}");

                        var localMap = new Dictionary<uint, int>();

                        while (fs.Position < chunkEnd)
                        {
                            long remainingBytes = chunkEnd - fs.Position;
                            byte[] buffer = new byte[Math.Min(BufferSize, remainingBytes)];
                            int bytesRead = fs.Read(buffer, 0, buffer.Length);

                            if (bytesRead == 0)
                                throw new IOException($"Chunk {chunkIndex}: Empty or unreadable data.");

                            if (bytesRead % 4 != 0)
                                throw new IOException($"Chunk {chunkIndex}: Data misalignment detected. Invalid binary format.");

                            for (int i = 0; i < bytesRead; i += 4)
                            {
                                uint number = BitConverter.ToUInt32(buffer, i);
                                localMap[number] = localMap.TryGetValue(number, out int count) ? count + 1 : 1;
                            }
                        }

                        foreach (var kvp in localMap)
                        {
                            frequencyCountMap.AddOrUpdate(kvp.Key, kvp.Value, (_, count) => count + kvp.Value);
                        }
                    }
                    catch (IOException ex)
                    {
                        throw new IOException($"Failed to process binary file chunk {chunkIndex}. Invalid format detected.", ex);
                    }
                });
            }
            catch (AggregateException ex)
            {
                foreach (var inner in ex.InnerExceptions)
                {
                    if (inner is IOException ioEx)
                        throw ioEx;
                }
                throw new IOException("Failed to process binary file due to multiple errors.", ex);
            }

            int uniqueNumbersCount = frequencyCountMap.Count;
            int onlyOnceNumbersCount = frequencyCountMap.Values.Count(v => v == 1);

            return (uniqueNumbersCount, onlyOnceNumbersCount);
        }


        public void DisplayResults(string filePath)
        {
            try
            {
                var (uniqueNumbersCount, onlyOnceNumbersCount) = ProcessBinaryFileParallel(filePath);

                Console.WriteLine($"Unique numbers count: {uniqueNumbersCount}");
                Console.WriteLine($"Numbers found only once count: {onlyOnceNumbersCount}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error processing file: {ex.Message}");
            }
        }
    }
}
