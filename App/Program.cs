using TrueType;

namespace App
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello, World!");

            var ttf = new TrueTypeFont(@"Fonts/DroidSerif-Regular.ttf");
        }
    }
}