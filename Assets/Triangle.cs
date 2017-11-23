using UnityEngine;

namespace CubeSphere
{
    class Triangle
    {
        Vertex[] vertices = new Vertex[3];

        public Triangle(Vertex v1, Vertex v2, Vertex v3)
        {
            vertices[0] = v1;
            vertices[1] = v2;
            vertices[2] = v3;
        }

        public Vertex this[int i]
        {
            get { return vertices[i]; }
            set { vertices[i] = value; }
        }

        public static void DebugTriangle(Triangle t)
        {
            Debug.DrawLine(t[0].pos, t[1].pos, Color.yellow);
            Debug.DrawLine(t[1].pos, t[2].pos, Color.yellow);
            Debug.DrawLine(t[2].pos, t[0].pos, Color.yellow);
        }
    }
}
