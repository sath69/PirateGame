using UnityEngine;

public class Tornado : Tile
{
    [SerializeField] public override TileType tileType => TileType.Tornado;
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

    public int GeneratePosition()
    {
        int value = UnityEngine.Random.Range(1,31);
        return value;
    }
}
