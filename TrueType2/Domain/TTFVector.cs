using TrueType2.Domain.Cache;
using TrueType2.Extension;
using TrueType2.Mode;

namespace TrueType2.Domain
{
    public class TTFVector
    {
        public Vertex[] Vertices { get; set; }

        public TTFVector(Vertex[] vertices) 
        {
            Vertices = vertices;
        }

    }
}
