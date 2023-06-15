using TrueType.Domain.Cache;
using TrueType.Extension;
using TrueType.Mode;

namespace TrueType.Domain
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
