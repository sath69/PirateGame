using Unity.Netcode;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : NetworkBehaviour
{
    public static GameManager Instance {get; set;}
    private int startingGold;
    public Dictionary<ulong, PlayerManager> players = new Dictionary<ulong, PlayerManager>();
    public Dictionary<int, Tile> tiles = new Dictionary<int, Tile>();
    public Tile[] tilesArray = new Tile[32];
    public Sprite[] flags = new Sprite[6];
    public GameObject flag;
    public Transform flagTransform;
    private void Awake()
    {
        Instance = this;
    }
    public override void OnNetworkSpawn()
    {

        //Subscriber events for network
        NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
        NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnected;
        GameUIManager.Instance.LoadGameUI();

        foreach(Tile t in tilesArray)
        {
            tiles.Add(t.tileId, t);
        }
    }

    public override void OnNetworkDespawn()
    {
        NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnected;
        NetworkManager.Singleton.OnClientDisconnectCallback -= OnClientDisconnected;
        GameUIManager.Instance.BackToMenu();
        players.Clear();
        ClearFlags(flagTransform);
        
        foreach(Tile t in tilesArray)
        {
            tiles.Remove(t.tileId);
        }
    }

    private void OnClientConnected(ulong clientId)
    {
        var player = NetworkManager.Singleton.ConnectedClients[clientId].PlayerObject.GetComponent<PlayerManager>();
        AddPlayer(player);
    }

    private void OnClientDisconnected(ulong clientId)
    {
        RemovePlayer(clientId);
    }

    public void AddPlayer(PlayerManager player)
    {
        if (!players.ContainsKey(player.OwnerClientId))
        {
            players[player.OwnerClientId] = player;
            GameUIManager.Instance.UpdateOrCreatePlayerUI(player);
        }
    }
    public void RemovePlayer(ulong clientId)
    {
        foreach(Transform f in flagTransform)
        {
            if(f.GetComponent<SpriteRenderer>().sprite == flags[clientId])
            {
                Destroy(f.gameObject);
            }
        }
        foreach(Port port in players[clientId].inventory.Keys)
        {
            port.RemoveOwnership();
        }
        players[clientId].inventory.Clear();
        if (players.ContainsKey(clientId))
        {
            players.Remove(clientId);
            GameUIManager.Instance.RemovePlayerUI(clientId);
        }
    }
    public void SetStartingGold(int startingGold)
    {
        this.startingGold = startingGold;
    }
    public int SetStartingGoldForClients()
    {
        return startingGold;
    }

    private void ApplyTreasureEffect(TreasureEffects effect, ulong clientId)
    {
        switch (effect)
        {
           case TreasureEffects.GoToStart:
                ChatManager.Instance.BroadcastMessageClientRpc($"[GAME]: {players[clientId].GetUsername()}, you can advance to Go!", Color.magenta);
                players[clientId].MoveToTile(0);
                break; 
           case TreasureEffects.GoToAgartha:
                ChatManager.Instance.BroadcastMessageClientRpc($"[GAME]: {players[clientId].GetUsername()}, you can advance to the Land Down Under!", Color.magenta);
                players[clientId].MoveToTile(16);
                break;
           case TreasureEffects.SuccessfulRaid:
                ChatManager.Instance.BroadcastMessageClientRpc($"[GAME]: {players[clientId].GetUsername()}, you have successfully raided your enemies. Receive 50 gold.", Color.magenta);
                players[clientId].AddGold(50);
                Dice.Instance.EnableEndTurnClientRpc(clientId);
                break;
           case TreasureEffects.LootInheritance:
                ChatManager.Instance.BroadcastMessageClientRpc($"[GAME]: {players[clientId].GetUsername()}, you have received loot inheritance. Receive 100 gold.", Color.magenta);
                players[clientId].AddGold(100);
                Dice.Instance.EnableEndTurnClientRpc(clientId);
                break;
           case TreasureEffects.TakeTripToBahamas:
                ChatManager.Instance.BroadcastMessageClientRpc($"[GAME]: Take a break, {players[clientId].GetUsername()}! Advance to the Bahamas.", Color.magenta);
                players[clientId].MoveToTile(28);
                break;
           case TreasureEffects.HolidayPayment:
                ChatManager.Instance.BroadcastMessageClientRpc($"[GAME]: Merry Xmas, {players[clientId].GetUsername()}! Here is a gift of 20 gold for yah, matey!", Color.magenta);
                players[clientId].AddGold(20);
                Dice.Instance.EnableEndTurnClientRpc(clientId);
                break;
           case TreasureEffects.Potluck:
                ChatManager.Instance.BroadcastMessageClientRpc($"[GAME]: You have recieved a potluck of 70 gold, {players[clientId].GetUsername()}.", Color.magenta);
                players[clientId].AddGold(70);
                Dice.Instance.EnableEndTurnClientRpc(clientId);
                break;
        }
    }

    private void ApplyMysteryEffect(MysteryEffects effect, ulong clientId)
    { 
        switch (effect)
        {
            case MysteryEffects.GoToPrison:
                ChatManager.Instance.BroadcastMessageClientRpc($"[GAME]: {players[clientId].GetUsername()}, you go to Prison!", Color.grey);
                players[clientId].MoveToTile(23);
                break;
            case MysteryEffects.GoToAgartha:
                ChatManager.Instance.BroadcastMessageClientRpc($"[GAME]: {players[clientId].GetUsername()}, you can advance to the Land Down Under!", Color.grey);
                players[clientId].MoveToTile(16);
                break;
            case MysteryEffects.Mutiny:
                ChatManager.Instance.BroadcastMessageClientRpc($"[GAME]: Your crew has commited mutiny, {players[clientId].GetUsername()}! You lose 25 gold.", Color.gray);
                players[clientId].RemoveGold(25);
                Dice.Instance.EnableEndTurnClientRpc(clientId);
                break;
            case MysteryEffects.GoToStart:
                ChatManager.Instance.BroadcastMessageClientRpc($"[GAME]: {players[clientId].GetUsername()}, you can advance to Go!", Color.gray);
                players[clientId].MoveToTile(0);
                break;
            case MysteryEffects.Elected:
                ChatManager.Instance.BroadcastMessageClientRpc($"[GAME]: You have been elected to captain of the crew, {players[clientId].GetUsername()}. Pay your enemies 30 gold each so they don't raid you!", Color.gray);
                foreach(ulong id in NetworkManager.Singleton.ConnectedClientsIds)
                {
                    if(id == clientId)
                    {
                        continue;
                    }
                    if (players[id].IsActive())
                    {
                        players[id].AddGold(30);
                    }
                }
                int total = 30 * (NetworkManager.Singleton.ConnectedClientsList.Count-1);
                players[clientId].RemoveGold(total);
                Dice.Instance.EnableEndTurnClientRpc(clientId);
                break;
            case MysteryEffects.GoBackToSteps:
                int value = UnityEngine.Random.Range(1,6);
                ChatManager.Instance.BroadcastMessageClientRpc($"[GAME]: Go back {value} steps, {players[clientId].GetUsername()}", Color.gray);
                players[clientId].MoveBackSteps(value);
                break;
            case MysteryEffects.LandTax:
                ChatManager.Instance.BroadcastMessageClientRpc($"[GAME]: Pay your land tax, {players[clientId].GetUsername()}. It is 50 gold", Color.gray);
                players[clientId].RemoveGold(50);
                Dice.Instance.EnableEndTurnClientRpc(clientId);
                break;

        }
    }
    

    public void ClearFlags(Transform flags)
    {
        foreach(Transform flag in flags)
        {
            Destroy(flag.gameObject);
        }
        
    }

    public void AddGoReward(ulong clientId)
    {
        players[clientId].AddGold(250); 
    }

    private void AddFlag(ulong id, Port port)
    {
        GameObject newFlag;
        newFlag = Instantiate(flag, port.GetFlagPosition(), Quaternion.identity, flagTransform);
        newFlag.GetComponent<SpriteRenderer>().sprite = flags[id];
    }

    public void Upgrade(int portId)
    {
        UpgradeServerRpc(portId, new RpcParams());
    }

    public void Downgrade(int portId)
    {
        DowngradeServerRpc(portId, new RpcParams());
    }
    public void PurchasePort(ulong id, int price, int tileId)
    {
        players[id].RemoveGold(price);
        PurchaseClientRpc(tileId, id);
    }

    #region RPCs

    [ClientRpc]
    public void OnTileLandedClientRpc(ulong clientId, TileType tileType, int tileId)
    {
        PlayerManager player = players[clientId];
        if(NetworkManager.Singleton.LocalClientId == clientId){
          switch(tileType)
          {
            case TileType.Go:
                Dice.Instance.EnableEndTurn(clientId);
                break;
            case TileType.Port:
                Port port = tiles[tileId].GetComponent<Port>();
                if (!port.IsOwned())
                {
                    if(player.GetGold() > port.GetPrice())
                    {
                        GameUIManager.Instance.EnablePurchaseButton();
                        return;
                    }
                    else
                    {
                        GameUIManager.Instance.EnableBiddingButton();
                        return;
                    }
                }
                else if(port.IsOwned() && port.GetOwnerId() == clientId)
                {
                    Dice.Instance.EnableEndTurn(clientId);
                    return;
                }
                ChargeRentServerRpc(clientId, port.GetOwnerId(), port.GetRentPrice());
                Dice.Instance.EnableEndTurn(clientId); 
                break;
            case TileType.Mystery:
                MysteryServerRpc(tileId, clientId);
                break;
            case TileType.Treasure:
                TreasureServerRpc(tileId, clientId);
                break;
            case TileType.Tax:
                Debug.Log("Landed on tax.");
                TaxServerRpc(clientId, tileId);
                Dice.Instance.EnableEndTurn(clientId);
                break;
            case TileType.Agartha:
                EnableRestServerRpc(clientId);
                Dice.Instance.EnableEndTurn(clientId);
                break;
            case TileType.Prison:
                EnablePrisonServerRpc(clientId);
                Dice.Instance.EnableEndTurn(clientId);
                break;
            case TileType.Tornado:
                int randomPosition = tiles[tileId].GetComponent<Tornado>().GeneratePosition();
                TornadoServerRpc(clientId, randomPosition);
                break;
            case TileType.PassingBy:
                Dice.Instance.EnableEndTurn(clientId);
                break;
          }

        }
    }

    [ClientRpc]
    private void UpgradeClientRpc(int id, ulong senderId)
    {
        Port port = tiles[id].GetComponent<Port>();
        port.UpgradePortLevel();
        port.OnUpgradeTriggered(port.GetPortLevel());
        if (NetworkManager.Singleton.LocalClientId == senderId)
        {
            GameUIManager.Instance.inventoryUI[port].GetComponent<Inventory>().UpdateUpgradeButtons(port.GetPortLevel());
        }
    }

    [ClientRpc]
    public void PurchaseClientRpc(int tileId, ulong id)
    {
        Port port = tiles[tileId].GetComponent<Port>();
        port.SetOwnership(id);
        players[id].inventory.Add(port, port.GetPortType());
        AddFlag(id, port);
        if(NetworkManager.Singleton.LocalClientId == id)
        {
            GameUIManager.Instance.UpdateInventoryUI(port,tileId);
            players[id].CheckForUpgrades(players[id].inventory);
        }
    }

    [ClientRpc]
    private void DowngradeClientRpc(int id, ulong senderId)
    {
        Port port = tiles[id].GetComponent<Port>();
        port.DowngradePortLevel();
        port.OnUpgradeTriggered(port.GetPortLevel());
        if (NetworkManager.Singleton.LocalClientId == senderId)
        {
            GameUIManager.Instance.inventoryUI[port].GetComponent<Inventory>().UpdateUpgradeButtons(port.GetPortLevel());
        }
    }

    [ClientRpc]
    private void UpgradeFailedClientRpc(ulong senderId)
    {
        if(NetworkManager.Singleton.LocalClientId == senderId)
        {
            Debug.Log("Not enough money!");
        }
    }


    [ClientRpc]
    private void ProcessBankruptClientRpc(ulong senderClientId)
    {
        players[senderClientId].icon.SetActive(false);
        foreach(Transform f in flagTransform)
        {
            if(f.GetComponent<SpriteRenderer>().sprite == flags[senderClientId])
            {
                Destroy(f.gameObject);
            }
        }
        foreach(Port port in players[senderClientId].inventory.Keys)
        {
            port.RemoveOwnership();
        }
        players[senderClientId].inventory.Clear();
    }

    [Rpc(SendTo.Server)]
    private void ChargeRentServerRpc(ulong id, ulong ownerId, int value)
    {
        players[id].RemoveGold(value);
        players[ownerId].AddGold(value);
    }
    
    [Rpc(SendTo.Server)]
    public void TornadoServerRpc(ulong clientId, int value)
    {
        players[clientId].Move(value);
        ChatManager.Instance.BroadcastMessageClientRpc($"[GAME]: {players[clientId].GetUsername()} has landed on a Tornado!", Color.red);
    }

    [Rpc(SendTo.Server)]
    public void TaxServerRpc(ulong clientId, int tileId)
    {
        int value = tiles[tileId].GetComponent<Tax>().CalculateTax(players[clientId].GetGold());
        players[clientId].SetGold(value);
        ChatManager.Instance.BroadcastMessageClientRpc($"[GAME]: {players[clientId].GetUsername()} has landed on Tax!", Color.yellow);
    }

    [Rpc(SendTo.Server)]
    public void DowngradeServerRpc(int portId, RpcParams rpcParams)
    {
        ulong senderId = rpcParams.Receive.SenderClientId;
        int downgradePrice = tiles[portId].GetComponent<Port>().GetCurrentUpgradeOption();
        players[senderId].AddGold(downgradePrice);
        DowngradeClientRpc(portId, senderId);
    }

    [Rpc(SendTo.Server)]
    public void EnableRestServerRpc(ulong clientId)
    {
        players[clientId].SetResting(true);
    }

    [Rpc(SendTo.Server)]
    public void DisableRestServerRpc(ulong clientId)
    {
        players[clientId].SetResting(false);
    }

    [Rpc(SendTo.Server)]
    public void EnablePrisonServerRpc(ulong clientId)
    {
        players[clientId].SetPrison(true);
        ChatManager.Instance.BroadcastMessageClientRpc($"[GAME]: {players[clientId].GetUsername()} has landed on Prison!", Color.gray);
    }

    [Rpc(SendTo.Server)]
    public void DisablePrisonServerRpc(ulong clientId)
    {
        players[clientId].SetResting(false);
    }

    [Rpc(SendTo.Server)]
    public void UpgradeServerRpc(int portId, RpcParams rpcParams)
    {
        ulong senderId = rpcParams.Receive.SenderClientId;
        int upgradePrice = tiles[portId].GetComponent<Port>().GetNextUpgradeOption();
        if(players[senderId].GetGold() > upgradePrice)
        {
            players[senderId].RemoveGold(upgradePrice);
            UpgradeClientRpc(portId, senderId);
        }
        else
        {
            UpgradeFailedClientRpc(senderId);
        }
    }

    [Rpc(SendTo.Server)]
    public void PurchaseServerRpc(RpcParams rpcParams)
    {
        ulong senderClientId = rpcParams.Receive.SenderClientId;
        int tileId = players[senderClientId].currentTilePosition;
        int price = tiles[tileId].GetComponent<Port>().GetPrice();
        players[senderClientId].RemoveGold(price);
        PurchaseClientRpc(tileId, senderClientId);
        Dice.Instance.EnableEndTurnClientRpc(senderClientId);
    }

    [Rpc(SendTo.Server)]
    public void ProcessBankruptServerRpc(RpcParams rpcParams)
    {
        ulong senderId = rpcParams.Receive.SenderClientId;
        players[senderId].SetPlayerState(PlayerState.Bankrupt);
        Dice.Instance.RemovePlayerFromList(senderId);
        players[senderId].SetGold(0);
        ProcessBankruptClientRpc(senderId);
    }

    [Rpc(SendTo.Server)]
    private void TreasureServerRpc(int tileId, ulong clientId)
    {
        Treasure treasure = tiles[tileId].GetComponent<Treasure>();
        TreasureEffects effect = treasure.GenerateEffect();
        ApplyTreasureEffect(effect, clientId);
    }
    
    [Rpc(SendTo.Server)]
    private void MysteryServerRpc(int tileId, ulong clientId)
    {
        Mystery mystery = tiles[tileId].GetComponent<Mystery>();
        MysteryEffects effect = mystery.GenerateEffect();
        ApplyMysteryEffect(effect, clientId);
    }
    
    #endregion
}