using UnityEngine;

public class Go : Tile
{
    [SerializeField] public override TileType tileType => TileType.Go;
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
