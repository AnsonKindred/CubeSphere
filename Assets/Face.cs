using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace CubeSphere
{
    class Face
    {
        public float startPhi, endPhi, startTheta, endTheta;
        public Layer layer;
        public List<Triangle> triangles = new List<Triangle>();

        public Face(Layer layer, float startPhi, float endPhi, float startTheta, float endTheta)
        {
            this.layer = layer;
            this.startPhi = startPhi;
            this.endPhi = endPhi;
            this.startTheta = startTheta;
            this.endTheta = endTheta;

            triangulate();
        }

        /// <summary>Splits the face intro triangles.</summary>
        /// <remarks>
        /// Triangulation is done by treating each pair of slices in the face as a polygon. Each polygon is triangulated
        /// via ear clipping. This leaves only the left and right sides which are handled similarly.
        /// </remarks>
        public void triangulate()
        {
            // Get slices between start and end phi
            var slices = layer.slices.Where( kv => (kv.Key >= startPhi && kv.Key <= endPhi) ? true : false );

            // Create polygons from each pair of slices within this face and triangulate them
            for (int i = 0; i < slices.Count() - 1; i++)
            {
                Slice slice = slices.ElementAt(i).Value;
                Slice nextSlice = slices.ElementAt(i + 1).Value;

                // Create a polygon from the pair of slices
                List<Vertex> polygon = new List<Vertex>();

                // Get vertices in current slice between start and end theta
                var firstRow = GetVerticesInRange(slice, startTheta, endTheta);
                // Add the vertices to the polygon
                polygon.AddRange(firstRow);

                // Get vertices in next slice between start and end theta
                var secondRow = GetVerticesInRange(nextSlice, startTheta, endTheta);
                // Notice the reverse so that our polygon is a loop
                secondRow.Reverse();
                // Add the vertices to the polygon
                polygon.AddRange(secondRow); 

                // Triangulate the polygon and get back the list of triangles
                var polygonTriangles = Triangulator.Triangulate(polygon);
                // Add the triangles
                triangles.AddRange(polygonTriangles);
            }

            // TODO: Something with edges
        }

        // I would have just done this in linq but it required special handling for wrapping
        List<Vertex> GetVerticesInRange(Slice slice, float startTheta, float endTheta)
        {
            var vertices = new List<Vertex>();

            foreach(KeyValuePair<float, Vertex> kv in slice.v)
            {
                if (kv.Key >= startTheta && kv.Key <= endTheta)
                {
                    vertices.Add(kv.Value);
                }
            }

            if (endTheta == 2 * Mathf.PI)
            {
                vertices.Add(slice.v[0]);
            }

            return vertices;
        }
        
        public static void DebugFace(Face face)
        {
            var slices = face.layer.slices.Where(kv => (kv.Key >= face.startPhi && kv.Key <= face.endPhi) ? true : false);
            Slice firstSlice = slices.ElementAt(0).Value;
            Slice lastSlice = slices.ElementAt(slices.Count() - 1).Value;
            var firstRow = face.GetVerticesInRange(firstSlice, face.startTheta, face.endTheta);
            var lastRow = face.GetVerticesInRange(lastSlice, face.startTheta, face.endTheta);

            for (int i = 1; i < firstRow.Count(); i++)
            {
                Debug.DrawLine(firstRow.ElementAt(i - 1).pos, firstRow.ElementAt(i).pos, Color.green);
            }
            Debug.DrawLine(firstRow.ElementAt(firstRow.Count() - 1).pos, lastRow.ElementAt(lastRow.Count() - 1).pos, Color.magenta);
            for (int i = lastRow.Count() - 2; i >= 0; i--)
            {
                Debug.DrawLine(lastRow.ElementAt(i + 1).pos, lastRow.ElementAt(i).pos, Color.cyan);
            }
            Debug.DrawLine(lastRow.ElementAt(0).pos, firstRow.ElementAt(0).pos, Color.red);
        }
    }
}
