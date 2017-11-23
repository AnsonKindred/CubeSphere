using System;
using System.Collections.Generic;

namespace CubeSphere
{
    class Slice
    {
        public SortedDictionary<float, Vertex> v = new SortedDictionary<float, Vertex>();
        public SortedDictionary<float, Vertex> primaryV = new SortedDictionary<float, Vertex>();
        public int divs;
        public float phi;
        public bool isPrimary;

        public Slice(int divs, float phi)
        {
            this.divs = divs;
            this.phi = phi;
        }

        public void AddVertexIfNotExists(float phi, float theta, float radius, bool isPrimary)
        {
            if (!v.ContainsKey(theta))
            {
                v[theta] = new Vertex(phi, theta, radius);
            }

            if (isPrimary)
            {
                v[theta].isPrimary = true;
                primaryV[theta] = v[theta];
            }
        }
    }
}
