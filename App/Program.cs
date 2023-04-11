using TrueType;

namespace App
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello, World!");

            var font = new Font("sans", @"Fonts/DroidSerif-Regular.ttf");

            Console.WriteLine($"TTF Tables Count: {font.TTF._rawTables.Count()}");
            font.TTF._rawTables.ToList().ForEach(x => Console.WriteLine($"{x.Key}: {x.Value}"));
        }
    }
}