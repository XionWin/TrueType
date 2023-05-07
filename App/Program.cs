using Common;
using TrueType;

namespace App
{
    internal class Program
    {
        static void Main(string[] args)
        {
            System.Diagnostics.Debug.WriteLine("Hello, World!");

            var font = new Font("sans", @"Fonts/DroidSerif-Regular.ttf");

            System.Diagnostics.Debug.WriteLine($"TTF Tables Count: {font.TTF._rawTables.Count()}");
            font.TTF._rawTables.ToList().ForEach(x => System.Diagnostics.Debug.WriteLine($"{x.Key}: {x.Value}"));


            // To create a new window, create a class that extends GameWindow, then call Run() on it.
            using (var window = new Window(720, 720))
                window.Run();
        }
    }
}