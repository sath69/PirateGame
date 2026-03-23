using System;
using UnityEngine;

public class Tax : Tile
{
    [SerializeField] public override TileType tileType => TileType.Tax;
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

    public int CalculateTax(int value)
    {
        int newValue = value - (int)(0.2 * value);
        return newValue;
    }
}
