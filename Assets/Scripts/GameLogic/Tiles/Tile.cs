using UnityEngine;

public enum TileType
{
    Go,
    Treasure,
    Mystery,
    Tornado,
    Tax,
    Port,
    Prison,
    Agartha,
    PassingBy
}

public abstract class Tile : MonoBehaviour
{
    public string tileName;
    public int tileId;
    public abstract TileType tileType {get;}

    public virtual Vector2 GetTilePosition()
    {
       Vector2 position = new Vector2(Random.Range(transform.position.x-0.25f, transform.position.x+0.25f), Random.Range(transform.position.y-0.02f, transform.position.y+0.02f));
       return position;
    }

    public virtual string GetTileName()
    {
        return tileName;
    }

    public virtual TileType GetTileType()
    {
        return tileType;
    }
}
