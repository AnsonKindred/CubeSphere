using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace CubeSphere
{
    class Triangulator
    {
        public static List<Triangle> Triangulate(List<Vertex> polygon)
        {
            List<Triangle> triangles = new List<Triangle>();

            BuildLinks(polygon);
            Triangulate(polygon[0], triangles, 0);

            return triangles;
        }

        public static void BuildLinks(List<Vertex> polygon)
        {
            for (int i = 0; i < polygon.Count; i++)
            {
                if (i == 0) polygon[i].prev = polygon[polygon.Count - 1];
                else polygon[i].prev = polygon[i - 1];

                if (i == polygon.Count - 1) polygon[i].next = polygon[0];
                else polygon[i].next = polygon[i + 1];
            }
        }

        static void Triangulate(Vertex start, List<Triangle> triangles, int depth)
        {
            if (depth > 100)
            {
                Debug.Log("too deep sir");
                return;
            }

            Vertex v1 = null, v2 = null, v3 = null;

            // Base case, final triangle
            v1 = start.prev;
            v2 = start;
            v3 = start.next;
            if (v1.prev == v3)
            {
                Triangle triangle = new Triangle(v3, v2, v1);
                Debug.Log("adding final triangle: " + v1.polar + v2.polar + v3.polar);
                triangles.Add(triangle);

                return;
            }

            // Not base case, find ear to clip
            int sanityCount = 0;
            do
            {
                sanityCount++;
                float angle = Mathf.Deg2Rad*Vector2.Angle(v1.polar - v2.polar, v3.polar - v2.polar);
                if (angle < Mathf.PI && angle > Mathf.Epsilon)
                {
                    Debug.Log("angle is good: " + angle);
                    // Angle looks good, just need to make sure this triangle
                    // doesn't enclose any other points
                    bool containsOtherPoint = false;
                    Vertex current = start.next.next;
                    int sanityCount2 = 0;
                    while (current != start.prev && sanityCount2 < 100)
                    {
                        sanityCount++;
                        if (PointInTriangle(current.polar, v1.polar, v2.polar, v3.polar))
                        {
                            // Whoops, found a point in the triangle, this is not a valid ear
                            containsOtherPoint = true;
                            break;
                        }
                        current = current.next;
                    }
                    if (sanityCount2 == 100) Debug.Log("insane2");

                    if (!containsOtherPoint)
                    {
                        // Found an ear
                        Triangle triangle = new Triangle(v3, v2, v1);
                        Debug.Log("adding triangle: " + v1.polar + v2.polar + v3.polar);
                        triangles.Add(triangle);
                        
                        // Remove vertex
                        v1.next = v3;
                        v3.prev = v1;

                        // Process the rest of the polygon
                        Triangulate(v3.next, triangles, depth + 1);

                        break;
                    }
                }

                v1 = v1.next;
                v2 = v2.next;
                v3 = v3.next;

            } while (v2 != start && sanityCount < 100);

            if (sanityCount == 100) Debug.Log("insane 1");
        }

        public static bool PointInTriangle(Vector2 p, Vector2 t1, Vector2 t2, Vector2 t3)
        {
            var s = t1.y * t3.x - t1.x * t3.y + (t3.y - t1.y) * p.x + (t1.x - t3.x) * p.y;
            var t = t1.x * t2.y - t1.y * t2.x + (t1.y - t2.y) * p.x + (t2.x - t1.x) * p.y;

            if ((s < 0) != (t < 0))
                return false;

            var A = -t2.y * t3.x + t1.y * (t3.x - t2.x) + t1.x * (t2.y - t3.y) + t2.x * t3.y;
            if (A < 0.0)
            {
                s = -s;
                t = -t;
                A = -A;
            }
            return s > 0 && t > 0 && (s + t) <= A;
        }
    }
}
