using UnityEngine;

public class Agartha : Tile
{
    [SerializeField] public override TileType tileType => TileType.Agartha;
    public override string GetTileName()
    {
        return base.GetTileName();
    }
    public override Vector2 GetTilePosition()
    {
        return base.GetTilePosition();
    }
    public override TileType GetTileType()
    {
        return base.GetTileType();
    }
}
