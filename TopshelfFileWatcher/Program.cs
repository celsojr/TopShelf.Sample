namespace TopshelfFileWatcher
{
    internal class Program
    {
        internal static void Main()
        {
            var config = new ConfigureService();
            config.Configure();

            System.Console.ReadKey(true);
        }
    }
}
