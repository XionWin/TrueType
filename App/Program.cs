using Common;
using System.IO;
using System.Xml.Linq;
using TrueType;

namespace App
{
    internal class Program
    {
        static void Main(string[] args)
        {
            System.Diagnostics.Debug.WriteLine("Hello, World!");

            var path = @"Resources/Fonts/Zpix.ttf";
            var font = new Font("sans", path);

            if (File.Exists(path))
            {
                var ttf = new TrueType2.Domain.TTF("sans", path);
                foreach (var c in "我")
                {
                    ttf.GetGlyph(c);
                }
            }

            System.Diagnostics.Debug.WriteLine($"TTF Tables Count: {font.TTF._rawTables.Count()}");
            font.TTF._rawTables.ToList().ForEach(x => System.Diagnostics.Debug.WriteLine($"{x.Key}: {x.Value}"));


            // To create a new window, create a class that extends GameWindow, then call Run() on it.
            using (var window = new Window(512, 512))
                window.Run();
        }
    }
}