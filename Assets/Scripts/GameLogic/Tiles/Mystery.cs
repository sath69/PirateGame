using UnityEngine;
public enum MysteryEffects
{
    GoToPrison,
    GoToAgartha,
    GoBackToSteps,
    Mutiny,
    Elected,
    GoToStart,
    LandTax,

}

public class Mystery : Tile
{
    [SerializeField]public override TileType tileType => TileType.Mystery;
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

    public MysteryEffects GenerateEffect()
    {
        int value = UnityEngine.Random.Range(0, 7);
        return (MysteryEffects)value;
    }

}
