using UnityEngine;
using System.Collections.Generic;
using Unity.Services.Lobbies.Models;

public class LobbyUI : MonoBehaviour
{
    public static LobbyUI Instance { get; set;}
    public Transform container;
    public GameObject lobbySect;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        LobbyManager.Instance.OnLobbyListChanged += LobbyManager_OnLobbyListChanged;
    }

    private void LobbyManager_OnLobbyListChanged(object sender, LobbyManager.OnLobbyListChangedEventArgs e)
    {
        UpdateLobbyList(e.lobbyList);
    }

    private void UpdateLobbyList(List<Lobby> lobbyList)
    {
        foreach(Transform child in container)
        {
            if(child == lobbySect) continue;
            Destroy(child.gameObject);
        }

        foreach(Lobby lobby in lobbyList)
        {
            GameObject newLobbySect = Instantiate(lobbySect, container.transform);
            newLobbySect.SetActive(true);
            LobbyListSectionUI lobbySectUI = newLobbySect.GetComponent<LobbyListSectionUI>();
            lobbySectUI.UpdateLobby(lobby);

        }
    }
    public void OnRefreshButtonClicked()
    {
        LobbyManager.Instance.QueryLobbies();
    }
}
