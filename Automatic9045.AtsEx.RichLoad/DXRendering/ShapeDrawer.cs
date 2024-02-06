using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SlimDX;
using SlimDX.Direct3D9;

using Automatic9045.AtsEx.DXRendering.Vertices;

namespace Automatic9045.AtsEx.DXRendering
{
    internal class ShapeDrawer
    {
        private readonly Device Device;

        public ShapeDrawer(Device device)
        {
            Device = device;
        }

        private void FillPolygon(VertexWithColor[] vertices, int primitiveCount)
        {
            Device.SetTexture(0, null);
            Device.VertexFormat = VertexFormat.PositionRhw | VertexFormat.Diffuse;
            Device.DrawUserPrimitives(PrimitiveType.TriangleFan, primitiveCount, vertices);
        }

        public void FillPolygon(Color4 color, Vector2[] points, int primitiveCount)
        {
            VertexWithColor[] vertices = new VertexWithColor[points.Length];
            for (int i = 0; i < points.Length; i++)
            {
                vertices[i] = new VertexWithColor(points[i], color);
            }

            FillPolygon(vertices, primitiveCount);
        }

        public void FillRectangle(Color4 color, float x, float y, float width, float height)
        {
            Vector2[] points = new Vector2[]
            {
                new Vector2(x, y),
                new Vector2(x + width, y),
                new Vector2(x + width, y + height),
                new Vector2(x, y + height),
            };

            FillPolygon(color, points, 2);
        }

        public void DrawLine(Color4 color, float thickness, float x1, float y1, float x2, float y2)
        {
            if (x1 == x2)
            {
                FillRectangle(color, x1 - thickness / 2, Math.Min(y1, y2), thickness, Math.Abs(y2 - y1));
            }
            else if (y1 == y2)
            {
                FillRectangle(color, Math.Max(x1, x2), y1 - thickness / 2, Math.Abs(x2 - x1), thickness);
            }
            else
            {
                throw new NotImplementedException();
            }
        }
    }
}
