using System.Runtime.InteropServices;
using System.Collections.Concurrent;

namespace UniqueNumberCounterApp
{
    public unsafe class LowLevelBinaryFileProcessor
    {
        private const int BufferSize = 4096;

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern IntPtr CreateFile(string lpFileName, uint dwDesiredAccess, uint dwShareMode,
            IntPtr lpSecurityAttributes, uint dwCreationDisposition, uint dwFlagsAndAttributes, IntPtr hTemplateFile);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool ReadFile(IntPtr hFile, IntPtr lpBuffer, uint nNumberOfBytesToRead,
            out uint lpNumberOfBytesRead, IntPtr lpOverlapped);

        private const uint GENERIC_READ = 0x80000000;
        private const uint OPEN_EXISTING = 3;
        private const uint FILE_FLAG_NO_BUFFERING = 0x20000000;

        public (int uniqueNumbersCount, int onlyOnceNumbersCount) ProcessBinaryFile(string filePath)
        {
            if (!File.Exists(filePath))
                throw new FileNotFoundException($"File not found: {filePath}");

            var frequencyCountMap = new ConcurrentDictionary<uint, int>();

            try
            {
                ReadBinaryFileUsingKernel(filePath, frequencyCountMap);
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

        private void ReadBinaryFileUsingKernel(string filePath, ConcurrentDictionary<uint, int> frequencyCountMap)
        {
            IntPtr fileHandle = CreateFile(filePath, GENERIC_READ, 0, IntPtr.Zero, OPEN_EXISTING, FILE_FLAG_NO_BUFFERING, IntPtr.Zero);
            if (fileHandle == IntPtr.Zero)
                throw new IOException("Failed to open file using kernel access.");

            IntPtr buffer = Marshal.AllocHGlobal(BufferSize);
            try
            {
                while (true)
                {
                    if (!ReadFile(fileHandle, buffer, (uint)BufferSize, out uint bytesRead, IntPtr.Zero) || bytesRead == 0)
                        break;

                    if (bytesRead % 4 != 0)
                        throw new IOException("Invalid binary format. Expected 4-byte aligned data.");

                    uint* numPtr = (uint*)buffer;
                    for (int i = 0; i < bytesRead / 4; i++)
                    {
                        frequencyCountMap.AddOrUpdate(numPtr[i], 1, (_, count) => count + 1);
                    }
                }
            }
            finally
            {
                Marshal.FreeHGlobal(buffer);
            }
        }

        public void DisplayResults(string filePath)
        {
            try
            {
                var (uniqueNumbers, numbersSeenOnce) = ProcessBinaryFile(filePath);
                Console.WriteLine($"Unique numbers count is: {uniqueNumbers}");
                Console.WriteLine($"Numbers found only once count is: {numbersSeenOnce}");
            }
            finally
            {
                GC.Collect();
                GC.WaitForPendingFinalizers();
            }
        }
    }
}
