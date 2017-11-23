using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CubeSphere
{
    class Layer
    {
        public SortedDictionary<float, Slice> slices = new SortedDictionary<float, Slice>();
        public SortedDictionary<float, Slice> primarySlices = new SortedDictionary<float, Slice>();
        public List<Face> faces = new List<Face>();

        public void AddSliceIfNotExists(int numBlocks, float phi, bool isPrimary)
        {
            if (!slices.ContainsKey(phi))
            {
                slices[phi] = new Slice(numBlocks, phi);
                if (isPrimary)
                {
                    slices[phi].isPrimary = true;
                    primarySlices[phi] = slices[phi];
                }
            }
        }
    }
}
