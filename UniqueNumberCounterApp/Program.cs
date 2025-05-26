namespace UniqueNumberCounterApp
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length != 1)
            {
                Console.WriteLine("Usage: program <data_binary_file>");
                return;
            }

            string filePath = args[0];

            BinaryFileProcessor processor = new();
            processor.DisplayResults(filePath);
        }
    }
}
