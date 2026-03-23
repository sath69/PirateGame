using UnityEngine;

public enum TreasureEffects
{
    GoToStart,
    GoToAgartha,
    TakeTripToBahamas,
    Potluck,
    SuccessfulRaid,
    HolidayPayment,
    LootInheritance,
}

public class Treasure : Tile
{
    [SerializeField] public override TileType tileType => TileType.Treasure;
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
    public TreasureEffects GenerateEffect()
    {
        int value = UnityEngine.Random.Range(0, 7);
        return (TreasureEffects)value;
    }
    
}
