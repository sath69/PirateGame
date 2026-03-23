using UnityEngine;

public enum PortType
{
    IndianOcean,
    SouthEastAsia,
    EastAsia,
    Africa,
    SouthernEurope,
    SouthernAmerica,
    NorthernEurope,
    NorthAmerica

}

public class Port : Tile
{
    [SerializeField] public override TileType tileType => TileType.Port;
    [SerializeField] private int price;
    [SerializeField] private ulong? ownerId = null;
    [SerializeField] private bool isOwned = false;
    [SerializeField] private Transform flagTransform;
    [SerializeField] private PortType portType;
    //Base rent price set.
    [SerializeField] private int rentPrice;
    [SerializeField] private int[] rentPrices = new int[4];
    [SerializeField] private int[] upgradePrices = new int[3];
    private int portLevel;

    public void Start()
    {
        rentPrice = rentPrices[0];
    }
    public void OnDisable()
    {
        rentPrice = rentPrices[0];
        portLevel = 0;
        isOwned = false;
        ownerId = null;
    }
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

    public PortType GetPortType()
    {
        return portType;
    }

    public void SetOwnership(ulong clientId)
    {
        isOwned = true;
        SetOwnerId(clientId);
    }

    public void SetOwnerId(ulong Id)
    {
        ownerId = Id;
    }

    public int GetPrice()
    {
        return price;
    }
    public ulong GetOwnerId()
    {
        return ownerId.Value;
    }

    public void RemoveOwnership()
    {
        isOwned = false;
        ownerId = null;
        portLevel = 0;
        rentPrice = rentPrices[0];
    }

    public int GetRentPrice()
    {
        return rentPrice;
    }

    public bool IsOwned()
    {
        return isOwned;
    }
    public Vector3 GetFlagPosition()
    {
        return new Vector3(flagTransform.position.x, flagTransform.position.y, 0f);
    }

    public void OnUpgradeTriggered(int level)
    {
        switch (level)
        {
            case 1:
                rentPrice = rentPrices[1];
                break;
            case 2:
                rentPrice = rentPrices[2];
                break;
            case 3:
                rentPrice = rentPrices[3];
                break;  
        }
    }
    public void OnDowngradeTriggered(int level)
    {
        switch (level)
        {
            case 0:
                rentPrice = rentPrices[0];
                break;
            case 1:
                rentPrice = rentPrices[1];
                break;
            case 2:
                rentPrice = rentPrices[2];
                break;  
        }
    }
    public int GetPortLevel()
    {
        return portLevel;
    }

    public void UpgradePortLevel()
    {
        portLevel += 1;
    }

    public void DowngradePortLevel()
    {
        portLevel -= 1;
    }

    public int GetNextUpgradeOption()
    {
        return upgradePrices[portLevel];
    }

    public int GetCurrentUpgradeOption()
    {
        return upgradePrices[portLevel-1];
    }
}