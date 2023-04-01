using TrueType;

namespace App
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello, World!");

            var font = new Font(@"Fonts/DroidSerif-Regular.ttf");

            Console.WriteLine($"TTF Tables Count: {font.TTF.Raw.Tables.Count()}");
            font.TTF.Raw.Tables.ToList().ForEach(x => Console.WriteLine($"{x.Key}: {x.Value}"));
        }
    }
}