using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace CubeSphere
{
    [RequireComponent(typeof(MeshFilter))]
    [RequireComponent(typeof(MeshRenderer))]
    public class CubeSphere : MonoBehaviour
    {
        public int numLayers = 1;
        public float cubeSize = 1;

        MeshFilter meshFilter;
        MeshRenderer meshRenderer;
        Mesh mesh;
        Vector3[] vertices;
        int[] triangles;
        float debugLineLength = .05f;

        List<Layer> layers = new List<Layer>();

        List<List<Vector3>> v;
        List<Triangle> polygonTris;

        private void Start()
        {
            meshFilter = GetComponent<MeshFilter>();
            meshRenderer = GetComponent<MeshRenderer>();
            GenerateSphere();

            //List<Vertex> polygon = new List<Vertex>();
            //Vertex v;
            //v = new Vertex(2, 1, 2);
            //polygon.Add(v);
            //v = new Vertex(2, 1.1f, 2);
            //polygon.Add(v);
            //v = new Vertex(2, 1.2f, 2);
            //polygon.Add(v);
            //v = new Vertex(2, 1.3f, 2);
            //polygon.Add(v);
            //v = new Vertex(1, 1.3f, 2);
            //polygon.Add(v);
            //v = new Vertex(1, 1, 2);
            //polygon.Add(v);

            //polygonTris = Triangulator.Triangulate(polygon);

            //Debug.Log(polygonTris.Count);
        }

        public void Update()
        {
            ShowDebug();
        }

        void ShowDebug()
        {
            layers.ForEach(layer => layer.faces.ForEach(Face.DebugFace));
            //polygonTris.ForEach(Triangle.DebugTriangle);
        }

        void DebugVertex(float phi, float theta, Vertex v)
        {
            Color c = new Color(phi / (Mathf.PI), 0, 0);
            Debug.DrawLine(v.pos - Vector3.up * debugLineLength / 2.0f, v.pos + Vector3.up * debugLineLength / 2.0f, c);
            Debug.DrawLine(v.pos - Vector3.right * debugLineLength / 2.0f, v.pos + Vector3.right * debugLineLength / 2.0f, c);
            Debug.DrawLine(v.pos - Vector3.forward * debugLineLength / 2.0f, v.pos + Vector3.forward * debugLineLength / 2.0f, c);

            if (v.prev != null)
            {
                Debug.DrawLine(v.pos, v.prev.pos, Color.blue);
            }
        }

        void ForEachPoint(Action<float, float, Vertex> doThis)
        {
            for (int l = 1; l <= numLayers; l++)
            {
                foreach (KeyValuePair<float, Slice> kv in layers[l].slices)
                {
                    float phi = kv.Key;
                    Slice slice = kv.Value;
                    foreach (KeyValuePair<float, Vertex> kv2 in slice.v)
                    {
                        float theta = kv2.Key;
                        Vertex v = kv2.Value;
                        doThis(phi, theta, v);
                    }
                }
            }
        }
        
        public void GenerateSphere()
        {
            GenerateVertices();
            GenerateFaces();
            GenerateMesh();
        }

        public void GenerateVertices()
        {
            // 0th layer, a single vertex at the origin
            Layer layer = new Layer();
            layers.Add(layer);

            for (int l = 1; l <= numLayers; l++)
            {
                layer = new Layer();
                layers.Add(layer);

                float actualRadius = l * cubeSize;
                float effectiveRadius = actualRadius;

                // Add vertices for this layer
                AddVerticesForLayer(layer, actualRadius, effectiveRadius);
                if (l < numLayers)
                {
                    // Add secondary vertices dropped down from the next player
                    effectiveRadius = (l + 1) * cubeSize;
                    AddVerticesForLayer(layer, actualRadius, effectiveRadius);
                }
            }
        }
        
        void AddVerticesForLayer(Layer layer, float radius, float effectiveRadius)
        { 
            // Calculate number of slices in this layer
            int numSlices = Mathf.Max(1, Mathf.RoundToInt((float) Math.PI * effectiveRadius / cubeSize));

            for (int sliceIndex = 0; sliceIndex <= numSlices; sliceIndex++)
            {
                // Add primary vertices
                float percent = (float) sliceIndex / numSlices;
                int numBlocks = CalculateSliceDivisions(percent, effectiveRadius);
                float phi = Mathf.PI * percent;
                AddVerticesForSlice(layer, numBlocks, phi, radius, effectiveRadius, radius == effectiveRadius);
                
                // Add vertices projected from next slice
                if (sliceIndex < numSlices)
                {
                    float nextPercent = (float) (sliceIndex + 1) / numSlices;
                    int nextNumBlocks = CalculateSliceDivisions(nextPercent, effectiveRadius);
                    float nextPhi = Mathf.PI * nextPercent;
                    if (nextNumBlocks >= numBlocks)
                    {
                        AddVerticesForSlice(layer, nextNumBlocks, phi, radius, effectiveRadius, false);
                    }
                }

                // Add vertices projected from previous slice
                if (sliceIndex > 0)
                {
                    float previousPercent = (float) (sliceIndex - 1) / numSlices;
                    int previousNumBlocks = CalculateSliceDivisions(previousPercent, effectiveRadius);
                    float previousPhi = Mathf.PI * previousPercent;
                    if (previousNumBlocks > numBlocks)
                    {
                        AddVerticesForSlice(layer, previousNumBlocks, phi, radius, effectiveRadius, false);
                    }
                }
            }
        }

        void AddVerticesForSlice(Layer layer, int numBlocks, float phi, float radius, float effectiveRadius, bool isPrimary)
        {
            layer.AddSliceIfNotExists(numBlocks, phi, isPrimary);

            Slice slice = layer.slices[phi];

            for (int blockIndex = 0; blockIndex < numBlocks; blockIndex++)
            {
                float theta = 2 * Mathf.PI * blockIndex / numBlocks;

                slice.AddVertexIfNotExists(phi, theta, radius, isPrimary);
            }
        }

        public void GenerateFaces()
        {
            for (int l = 1; l <= numLayers; l++)
            {
                Layer layer = layers[l];
                for (int u = 0; u < layer.primarySlices.Keys.Count; u++)
                {
                    float phi = layer.primarySlices.Keys.ElementAt(u);
                    Slice slice = layer.primarySlices[phi];

                    for (int v = 0; v < slice.primaryV.Keys.Count; v++)
                    {
                        float theta = slice.primaryV.Keys.ElementAt(v);
                        float nextTheta = 2 * Mathf.PI;
                        if (v < slice.primaryV.Keys.Count - 1)
                        {
                            nextTheta = slice.primaryV.Keys.ElementAt(v + 1);
                        }
                        
                        // Find previous primary slice
                        if (u != 0)
                        {
                            float previousPhi = layer.primarySlices.Keys.ElementAt(u - 1);
                            Slice previousSlice = layer.primarySlices[previousPhi];
                            if (previousSlice.divs <= slice.divs)
                            {
                                Face face = new Face(layer, previousPhi, phi, theta, nextTheta);
                                layer.faces.Add(face);
                            }
                        }

                        // Find next primary slice
                        if (u != layer.primarySlices.Keys.Count - 1)
                        {
                            float nextPhi = layer.primarySlices.Keys.ElementAt(u + 1);
                            Slice nextSlice = layer.primarySlices[nextPhi];
                            if (nextSlice.divs < slice.divs)
                            {
                                Face face = new Face(layer, phi, nextPhi, theta, nextTheta);
                                layer.faces.Add(face);
                            }
                        }
                    }
                }
            }
        }

        public void GenerateMesh()
        {
            List<Vertex> vertexList = new List<Vertex>();
            List<int> indexes = new List<int>();
            List<Color> colorList = new List<Color>();

            for (int l = 1; l <= numLayers; l++)
            {
                Layer layer = layers[l];
                foreach (Slice slice in layer.slices.Values)
                {
                    foreach (Vertex v in slice.v.Values)
                    {
                        v.index = vertexList.Count;
                        vertexList.Add(v);
                        colorList.Add(UnityEngine.Random.ColorHSV(0, 1, 0, 1, 0, 1, 1, 1));
                    }
                }

                vertices = vertexList.Select(v => v.pos).ToArray();

                foreach (Face face in layer.faces)
                {
                    foreach (Triangle triangle in face.triangles)
                    {
                        indexes.Add(triangle[0].index);
                        indexes.Add(triangle[1].index);
                        indexes.Add(triangle[2].index);
                    }
                }
            }

            mesh = new Mesh();
            mesh.vertices = vertices;
            mesh.triangles = indexes.ToArray();
            mesh.colors = colorList.ToArray();

            meshFilter.mesh = mesh;
        }
        
        int CalculateSliceDivisions(float percent, float radius)
        {
            float phi = Mathf.PI * percent;
            float sliceRadius = Mathf.Sin(phi) * radius;
            float sliceCircumference = 2 * Mathf.PI * sliceRadius;
            return Mathf.Max(1, Mathf.RoundToInt(sliceCircumference / cubeSize));
        }
    }
}