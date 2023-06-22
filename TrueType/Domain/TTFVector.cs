using TrueType.Extension;
using TrueType.Mode;

namespace TrueType.Domain
{
    public class TTFVector
    {
        public char Character { get; set; }
        public Vertex[] Vertices { get; set; }

        public TTFVector(char character, Vertex[] vertices) 
        {
            Character = character;
            Vertices = vertices;
        }

    }
}
