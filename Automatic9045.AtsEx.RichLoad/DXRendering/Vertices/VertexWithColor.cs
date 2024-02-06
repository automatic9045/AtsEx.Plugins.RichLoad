using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SlimDX;

namespace Automatic9045.AtsEx.DXRendering.Vertices
{
    internal struct VertexWithColor
    {
        public float X;
        public float Y;
        public float Z;
        public float Rhw;
        public int Color;

        public VertexWithColor(float x, float y, float z, float rhw, int color)
        {
            X = x;
            Y = y;
            Z = z;
            Rhw = rhw;
            Color = color;
        }

        public VertexWithColor(Vector2 source, Color4 color) : this(source.X, source.Y, 0, 1, (int)color)
        {
        }

        public override string ToString() => $"{X}, {Y}, {Z}; {new Color4(Color)}";
    }
}
