using Unity.Netcode;
using System.Collections.Generic;
using UnityEngine.UI;


public class Dice : NetworkBehaviour
{
    private List<ulong> turnManager = new List<ulong>();
    private int currentTurnIndex = 0;
    public Button rollButton;
    public Button endTurnButton;
    public Button tryAgainButton;

    public static Dice Instance {get; set;}

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            NetworkManager.Singleton.OnClientConnectedCallback += AddPlayerToList;
            NetworkManager.Singleton.OnClientDisconnectCallback += RemovePlayerFromList;
        }


      
        rollButton.gameObject.SetActive(false);
        endTurnButton.gameObject.SetActive(false);
        tryAgainButton.gameObject.SetActive(false);
        Invoke(nameof(StartGame),10f);
    }

    private void Awake()
    {
        Instance = this;
    }
    private void StartGame()
    {
        Shuffle(turnManager);
        Turn();
    }

    private void Shuffle<T>(List<T> list)
    {
        System.Random rng = new System.Random();
        int n = list.Count;

        while (n > 1)
        {
            n--;
            int k = rng.Next(n + 1);
            T value = list[k];
            list[k] = list[n];
            list[n] = value;
        }
    }

    private void AddPlayerToList(ulong clientId)
    {
        turnManager.Add(clientId);
    }

    public void RemovePlayerFromList(ulong clientId)
    {
        turnManager.Remove(clientId);
        ProcessEndPlayerTurn();
    }
    public void OnRollButtonClicked()
    {
        rollButton.gameObject.SetActive(false);
        RequestDiceRollServerRpc(NetworkManager.Singleton.LocalClientId);
    }

    public void OnEndTurnButtonClicked()
    {
        endTurnButton.gameObject.SetActive(false);
        ProcessPlayerTurnServerRpc();
    }

    public void OnTryAgainButtonClicked()
    {
        tryAgainButton.gameObject.SetActive(false);
        Turn();
    }

    private void Turn()
    {
        if (turnManager.Count > 0)
        {
            if (currentTurnIndex >= 0 && currentTurnIndex < turnManager.Count)
            {
                NotifyPlayerClientRpc(turnManager[currentTurnIndex]);
            }
        }
    }

    private void ProcessEndPlayerTurn()
    {
        currentTurnIndex++;
        if (currentTurnIndex >= turnManager.Count)
        {
            currentTurnIndex = 0;
        }

        Turn();
    }
    //locally enable end turn
    public void EnableEndTurn(ulong clientId)
    {
        if(NetworkManager.Singleton.LocalClientId == clientId)
        {
            endTurnButton.gameObject.SetActive(true);
        }
    }

    #region Server RPCs

    [Rpc(SendTo.Server)]
    private void RequestDiceRollServerRpc(ulong clientId)
    {
        int roll1 = UnityEngine.Random.Range(1,5);
        int roll2 = UnityEngine.Random.Range(1,5);
        int roll = roll2 + roll1;
        SendDiceRollClientRpc(clientId, roll1, roll2);
        if (GameManager.Instance.players[clientId].IsPrison())
        {
            if(roll1 == roll2)
            {
                GameManager.Instance.players[clientId].SetPrison(false);
                GameManager.Instance.players[clientId].ResetPrisonCount();
            }
            else if(GameManager.Instance.players[clientId].GetPrisonCount() == 3)
            {
                GameManager.Instance.players[clientId].SetPrison(false);
                GameManager.Instance.players[clientId].ResetPrisonCount();
            }
            else
            {
                GameManager.Instance.players[clientId].IncrementPrisonCount();
            }   
            EnableEndTurnClientRpc(clientId);
            return;   
        }
        PlayerManager player = GameManager.Instance.players[clientId];
        player.Move(roll);
    }

    [Rpc(SendTo.Server)]
    private void ProcessPlayerTurnServerRpc()
    {
        ProcessEndPlayerTurn();
    }

    #endregion

    #region Client RPCs
    [ClientRpc]
    public void EnableEndTurnClientRpc(ulong clientId)
    {
        if (NetworkManager.Singleton.LocalClientId == clientId)
        {
            endTurnButton.gameObject.SetActive(true);
        }
    }

    [ClientRpc]
    private void SendDiceRollClientRpc(ulong clientId, int roll1, int roll2)
    {
        GameUIManager.Instance.SetGameStatus($"{GameManager.Instance.players[clientId].GetUsername()} has rolled a {roll1} and a {roll2}.");
    }

    [ClientRpc]
    private void NotifyPlayerClientRpc(ulong clientId)
    {
        if (NetworkManager.Singleton.LocalClientId == clientId)
        {
            if(GameManager.Instance.players[clientId].GetGold()> 0)
            {
                if(GameManager.Instance.players[clientId].IsResting())
                {
                    ProcessPlayerTurnServerRpc();
                    GameManager.Instance.DisableRestServerRpc(clientId);
                    return;
                }
                GameUIManager.Instance.SetGameStatus($"It is now your turn.");
                rollButton.gameObject.SetActive(true);
            }
            else
            {
                GameUIManager.Instance.SetGameStatus($"You have ran out of gold! You need at least 1 gold to roll.");
                rollButton.gameObject.SetActive(false);
                tryAgainButton.gameObject.SetActive(true);
            }
        }
        else
        {
            GameUIManager.Instance.SetGameStatus($"{GameManager.Instance.players[clientId].GetUsername()}'s turn.");
        }
    }

    #endregion
}