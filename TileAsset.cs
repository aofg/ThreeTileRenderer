using UnityEngine;

namespace Assets.ThreeTileRenderer
{
    [CreateAssetMenu(fileName = "Tile.asset", menuName = "Create Tile Asset")]
    public class TileAsset : ScriptableObject
    {
        public TilePrototypeData Data;
    }
}