using UnityEngine;
using TMPro;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine.UI;

public class GameUIManager : MonoBehaviour
{
    [Header("UI")]
    public GameObject lobbyUI;
    public GameObject interactiveUI;
    public GameObject chatUI;
    public GameObject menuUI;
    public GameObject gameUI;
    [Space(10)]
    [Header("Buttons")]
    public Button purchaseButton;
    public Button biddingButton;
    [Space(10)]
    [Header("GameStatus")]
    public TMP_Text gameStatus;
    [Header("PlayerInfo")]
    public Transform container;
    public GameObject playerInfo;
    private Dictionary<ulong, GameObject> playerUI = new Dictionary<ulong, GameObject>();
    public static GameUIManager Instance {get; set;}
    [Space(10)]
    [Header("Inventory")]
    public Transform inventoryContainer;
    public GameObject inventoryGO;
    public Dictionary<Port, GameObject> inventoryUI = new Dictionary<Port, GameObject>();

    public TMP_Text regionText;
    private void Awake()
    {
        Instance = this;
    }
    public void LoadGameUI()
    {
        lobbyUI.SetActive(false);
        interactiveUI.SetActive(true);
        chatUI.SetActive(true);
    }
    public void BackToMenu()
    {
        gameUI.SetActive(false);
        menuUI.SetActive(true);
        interactiveUI.SetActive(false);
        gameStatus.text = "";
        regionText.text = "";
        ClearPlayerList();
        ClearInventory();
    }

    public void UpdateOrCreatePlayerUI(PlayerManager player)
    {
        GameObject newPlayerInfo;
        if (!playerUI.TryGetValue(player.OwnerClientId ,out newPlayerInfo))
        {
            newPlayerInfo = Instantiate(playerInfo, container);
            newPlayerInfo.SetActive(true);
            playerUI[player.OwnerClientId] = newPlayerInfo;
        }
        PlayerInfo playerUIElement = newPlayerInfo.GetComponent<PlayerInfo>();
        playerUIElement.SetUsername(player.GetUsername());
        playerUIElement.SetGold(player.GetGold());
    }

    public void UpdateInventoryUI(Port port, int id)
    {
        GameObject portObj;
        if (!inventoryUI.TryGetValue(port, out portObj))
        {
            portObj = Instantiate(inventoryGO, inventoryContainer);
            portObj.SetActive(true);
            inventoryUI[port] = portObj;
        }
        Inventory inventory = portObj.GetComponent<Inventory>();
        inventory.SetPortColour(port.GetPortType());
        inventory.SetPortName(port.GetTileName());
        inventory.SetPortId(id);
    }

    public void OnBankruptBtnClicked()
    {
        GameManager.Instance.ProcessBankruptServerRpc(new RpcParams());
        interactiveUI.SetActive(false);
    }

    public void RemovePlayerUI(ulong clientId)
    {
        if (playerUI.ContainsKey(clientId))
        {
            Destroy(playerUI[clientId]);
            playerUI.Remove(clientId);
        }
    }

    public void ClearPlayerList()
    {
        foreach(var player in playerUI.Values)
        {
            Destroy(player);
        }
        playerUI.Clear();
    }

    public void ClearInventory()
    {
        foreach(var obj in inventoryUI.Values)
        {
            Destroy(obj);
        }
        inventoryUI.Clear();
    }
    public void SetGameStatus(string text)
    {
        gameStatus.text = text;
    }
    public void EnablePurchaseButton()
    {
        purchaseButton.gameObject.SetActive(true);
    }

    public void EnableBiddingButton()
    {
        biddingButton.gameObject.SetActive(true);
    }

    public void OnPurchaseButtonClicked()
    {
        purchaseButton.gameObject.SetActive(false);
        GameManager.Instance.PurchaseServerRpc(new RpcParams());
    }

    public void OnBidButtonClicked()
    {
        biddingButton.gameObject.SetActive(false);
        BiddingManager.Instance.BiddingServerRpc(new RpcParams());
    }

}
