using UnityEngine;

namespace CubeSphere
{
    class Vertex
    {
        public Vertex prev, next;
        public Vector3 pos;

        public float phi {
            get {
                return polar.x;
            }
            set {
                polar.x = value;
            }
        }
        public float theta {
            get {
                return polar.y;
            }
            set {
                polar.y = value;
            }
        }
        public float radius {
            get {
                return polar.z;
            }
            set {
                polar.z = value;
            }
        }

        public bool isPrimary;
        public int index;
        public Vector3 polar;

        public Vertex(float phi, float theta, float r)
        {
            pos.x = r * Mathf.Cos(theta) * Mathf.Sin(phi);
            pos.y = r * Mathf.Sin(theta) * Mathf.Sin(phi);
            pos.z = r * Mathf.Cos(phi);
            this.phi = phi;
            this.theta = theta;
            this.radius = r;
        }
    }
}
