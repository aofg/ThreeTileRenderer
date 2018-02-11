using ThreeTileRenderer.DataTypes;
using UnityEngine;

namespace ThreeTileRenderer
{
    [CreateAssetMenu(fileName = "Tile.asset", menuName = "Create Tile Asset")]
    public class TileAsset : ScriptableObject
    {
        public TilePrototypeData Data;

        public static implicit operator Mesh(TileAsset asset)
        {
            var mesh = new Mesh();
            mesh.vertices = asset.Data.Vertices;
            mesh.colors = asset.Data.Colors;
            mesh.uv = asset.Data.Uvs;
            mesh.tangents = asset.Data.Tangents;
            mesh.normals = asset.Data.Normals;
            mesh.triangles = asset.Data.Triangles;
            return mesh;
        }
    }
}