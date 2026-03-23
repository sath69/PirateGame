using Unity.Collections;
using Unity.Netcode;
using UnityEngine;
using System.Collections.Generic;

public enum PlayerState
{
    Active,
    Bankrupt
}
public class PlayerManager : NetworkBehaviour
{
    private NetworkVariable<FixedString64Bytes> Username = new NetworkVariable<FixedString64Bytes>(default,NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    private NetworkVariable<int> Gold = new NetworkVariable<int>(0);
    public Dictionary<Port,PortType> inventory = new Dictionary<Port, PortType>();
    public Sprite[] icons = new Sprite[6];
    public int currentTilePosition;
    public GameObject icon;
    private Vector2 startPos;
    private NetworkVariable<bool> isResting = new NetworkVariable<bool>(false);
    private NetworkVariable<bool> isPrison = new NetworkVariable<bool>(false);
    private NetworkVariable<int> prisonCount = new NetworkVariable<int>(0);
    private NetworkVariable<PlayerState> playerState = new NetworkVariable<PlayerState>();
    public override void OnNetworkSpawn()
    {
        if (IsOwner)
        {
            Username.Value = Login.Instance.GetUsername();
        }
        if (IsServer)
        {
            Gold.Value = GameManager.Instance.SetStartingGoldForClients();
            startPos = new Vector2(Random.Range(transform.position.x-0.45f, transform.position.x+0.45f), Random.Range(transform.position.y-0.25f, transform.position.y+0.25f));
            transform.position = startPos;
            playerState.Value = PlayerState.Active;
        }
        if (IsClient)
        {
            
            GameManager.Instance.AddPlayer(this);
            Username.OnValueChanged += OnDataChanged;
            Gold.OnValueChanged += OnDataChanged;
        }
        
        icon.GetComponent<SpriteRenderer>().sprite = icons[OwnerClientId];
        currentTilePosition = 0;
        
    }
    private void OnDataChanged<T>(T oldValue, T newValue)
    {
       GameUIManager.Instance.UpdateOrCreatePlayerUI(this);
    }

    public void RemoveGold(int value)
    {
        Gold.Value -= value;
    }

    public void AddGold(int value)
    {
        Gold.Value += value;
    }

    public string GetUsername()
    {
        return Username.Value.ToString();
    }

    public int GetGold()
    {
        return Gold.Value;
    }

    public void SetGold(int gold)
    {
        Gold.Value = gold;
    }

    public bool IsResting()
    {
        return isResting.Value;
    }

    public void SetResting(bool value)
    {
        isResting.Value = value;
    }

    public bool IsPrison()
    {
        return isPrison.Value;
    }

    public void SetPrison(bool value)
    {
        isPrison.Value = value;
    }

    public int GetPrisonCount()
    {
        return prisonCount.Value;
    }

    public void ResetPrisonCount()
    {
        prisonCount.Value = 0;
    }

    public void IncrementPrisonCount()
    {
        prisonCount.Value += 1;
    }

    public void MoveToTile(int tilePosition)
    {
        currentTilePosition = tilePosition;
        transform.position = GameManager.Instance.tiles[currentTilePosition].GetTilePosition();
        GameManager.Instance.OnTileLandedClientRpc(OwnerClientId, GameManager.Instance.tiles[currentTilePosition].GetTileType(), currentTilePosition);
    }

    public bool IsActive()
    {
        if(playerState.Value == PlayerState.Active)
        {
            return true;
        }
        return false;
    }

    public void SetPlayerState(PlayerState state)
    {
        playerState.Value = state;
    }

    public void MoveBackSteps(int steps)
    {
        // 0 - 2 = -2 => 30 (add 32)
        // 5 - 6 = -1 => 31
        // 6 - 5 = 1
        if(currentTilePosition - steps < 0)
        {
            currentTilePosition = currentTilePosition - steps + 32;
            transform.position =  GameManager.Instance.tiles[currentTilePosition].GetTilePosition();
        }
        else if (currentTilePosition - steps > 0)
        {
            currentTilePosition -= steps;
            transform.position = GameManager.Instance.tiles[currentTilePosition].GetTilePosition();
        }
        GameManager.Instance.OnTileLandedClientRpc(OwnerClientId, GameManager.Instance.tiles[currentTilePosition].GetTileType(), currentTilePosition);
    }

    public void Move(int steps)
    {
        if(currentTilePosition + steps > 31)
        {
            currentTilePosition = currentTilePosition + steps - 32;
            transform.position =  GameManager.Instance.tiles[currentTilePosition].GetTilePosition();
            GameManager.Instance.AddGoReward(OwnerClientId);
        }
        else if(currentTilePosition + steps < 31)
        {
            currentTilePosition += steps;
            transform.position = GameManager.Instance.tiles[currentTilePosition].GetTilePosition();
        }
        GameManager.Instance.OnTileLandedClientRpc(OwnerClientId, GameManager.Instance.tiles[currentTilePosition].GetTileType(), currentTilePosition);
    }

    public void CheckForUpgrades(Dictionary<Port, PortType> inventory)
    {
        int indianOceanCount = 0;
        int southEastAsiaCount = 0;
        int eastAsiaCount = 0;
        int africaCount = 0;
        int southEuropeCount = 0;
        int southAmericaCount = 0;
        int northEuropeCount = 0;
        int northAmericaCount = 0;
        foreach(PortType portType in inventory.Values)
        {
            switch (portType)
            {
                case PortType.IndianOcean:
                    indianOceanCount += 1;
                    break;
                case PortType.SouthEastAsia:
                    southEastAsiaCount += 1;
                    break;
                case PortType.EastAsia:
                    eastAsiaCount += 1;
                    break;
                case PortType.Africa:
                    africaCount += 1;
                    break;
                case PortType.SouthernEurope: 
                    southEuropeCount += 1;
                    break;
                case PortType.SouthernAmerica:
                    southAmericaCount += 1;
                    break;
                case PortType.NorthernEurope:
                    northEuropeCount += 1;
                    break;
                case PortType.NorthAmerica:
                    northAmericaCount += 1;
                    break;
            }
        }
        foreach(Port port in inventory.Keys)
        {
            if(port.GetPortType() == PortType.IndianOcean)
            {
                Inventory inventoryObj = GameUIManager.Instance.inventoryUI[port].GetComponent<Inventory>();
                if(indianOceanCount == 2)
                {
                    inventoryObj.UpdateUpgradeButtons(port.GetPortLevel());
                }
                else
                {
                    inventoryObj.HideButtons();
                }
            }

            if(port.GetPortType() == PortType.SouthEastAsia)
            {
                Inventory inventoryObj = GameUIManager.Instance.inventoryUI[port].GetComponent<Inventory>();
                if(southEastAsiaCount == 2)
                {
                    inventoryObj.UpdateUpgradeButtons(port.GetPortLevel());
                }
                else
                {
                    inventoryObj.HideButtons();
                }
            }

            if(port.GetPortType() == PortType.EastAsia)
            {
                Inventory inventoryObj = GameUIManager.Instance.inventoryUI[port].GetComponent<Inventory>();
                if(eastAsiaCount == 3)
                {
                    inventoryObj.UpdateUpgradeButtons(port.GetPortLevel());
                }
                else
                {
                    inventoryObj.HideButtons();
                }
            }

            if(port.GetPortType() == PortType.Africa)
            {
                Inventory inventoryObj = GameUIManager.Instance.inventoryUI[port].GetComponent<Inventory>();
                if(africaCount == 3)
                {
                    inventoryObj.UpdateUpgradeButtons(port.GetPortLevel());
                }
                else
                {
                    inventoryObj.HideButtons();
                }
            }

            if(port.GetPortType() == PortType.SouthernEurope)
            {
                Inventory inventoryObj = GameUIManager.Instance.inventoryUI[port].GetComponent<Inventory>();
                if(southEuropeCount == 2)
                {
                    inventoryObj.UpdateUpgradeButtons(port.GetPortLevel());
                }
                else
                {
                    inventoryObj.HideButtons();
                }
            }

            if(port.GetPortType() == PortType.SouthernAmerica)
            {
                Inventory inventoryObj = GameUIManager.Instance.inventoryUI[port].GetComponent<Inventory>();
                if(southAmericaCount == 2)
                {
                    inventoryObj.UpdateUpgradeButtons(port.GetPortLevel());
                }
                else
                {
                    inventoryObj.HideButtons();
                }
            }

            if(port.GetPortType() == PortType.NorthernEurope)
            {
                Inventory inventoryObj = GameUIManager.Instance.inventoryUI[port].GetComponent<Inventory>();
                if(northEuropeCount == 3)
                {
                    inventoryObj.UpdateUpgradeButtons(port.GetPortLevel());
                }
                else
                {
                    inventoryObj.HideButtons();
                }
            }

            if(port.GetPortType() == PortType.NorthAmerica)
            {
                Inventory inventoryObj = GameUIManager.Instance.inventoryUI[port].GetComponent<Inventory>();
                if(northAmericaCount == 3)
                {
                    inventoryObj.UpdateUpgradeButtons(port.GetPortLevel());
                }
                else
                {
                    inventoryObj.HideButtons();
                }
            }
                
        }
    }
}