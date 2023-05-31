using TrueType2.Domain.Support;
using TrueType2.Extension;

namespace TrueType2.Domain
{
    public class TTFVector
    {
        public TTFVertex[] Vertices { get; set; }

        public TTFVector(TTFVertex[] vertices) 
        {
            Vertices = vertices;
        }



    }
}
