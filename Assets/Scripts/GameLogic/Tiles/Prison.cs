using UnityEngine;

public class Prison : Tile
{
    [SerializeField] public override TileType tileType => TileType.Prison;
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
