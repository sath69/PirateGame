using UnityEngine;
using TMPro;
using Unity.Services.Lobbies.Models;

public class LobbyListSectionUI : MonoBehaviour {

    
    [SerializeField] private TextMeshProUGUI lobbyNameText;
    [SerializeField] private TextMeshProUGUI playersText;
    [SerializeField] private TextMeshProUGUI startMoneyText;

    private Lobby lobby;

    public void UpdateLobby(Lobby lobby) {
        this.lobby = lobby;
        lobbyNameText.text = lobby.Name;
        playersText.text = $"Players: {lobby.Players.Count + "/" + lobby.MaxPlayers}";
        startMoneyText.text = $"Starting Gold: {lobby.Data["Starting Money"].Value}";
    }

    public void OnLobbySectionButtonClicked()
    {
        LobbyManager.Instance.JoinLobbyById(lobby.Id);
    }
}