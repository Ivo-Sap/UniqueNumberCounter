namespace UniqueNumberCounterTests
{
    public class TestCleanupFixture : IDisposable
    {
        public void Dispose()
        {
            string tempPath = Path.GetTempPath();
            foreach (var file in Directory.GetFiles(tempPath, "tmp*.tmp"))
            {
                try
                {
                    File.Delete(file);
                }
                catch (IOException)
                {
                    Console.WriteLine($"Skipping locked file: {file}");
                }
            }
        }
    }
}
