using UnityEngine;
using System.Collections;
using UnityEditor;

namespace CubeSphere
{
    [CustomEditor(typeof(CubeSphere))]
    public class CubeSphereEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            CubeSphere myScript = (CubeSphere)target;
            if (GUILayout.Button("Build Object"))
            {
                myScript.GenerateSphere();
            }
        }
    }
}