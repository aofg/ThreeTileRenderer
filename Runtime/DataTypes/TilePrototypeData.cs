using UnityEngine;

namespace ThreeTileRenderer.DataTypes
{
    [System.Serializable]
    public class TilePrototypeData
    {
        public Vector3[] Vertices;
        public int[] Triangles;
        public Vector3[] Normals;
        public Vector2[] Uvs;
        public Vector4[] Tangents;
        public Color[] Colors;

        public bool IsValid => Vertices.Length > 0 && Normals.Length == Vertices.Length && Uvs.Length == Vertices.Length &&
                               Tangents.Length == Vertices.Length && (Colors.Length == 0 || Colors.Length == Vertices.Length);
    }
}
