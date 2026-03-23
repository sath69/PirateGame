using UnityEngine;
using Unity.Services.Core;
using Unity.Services.Authentication;
using Unity.Services.Lobbies.Models;
using Unity.Services.Lobbies;
using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine.UI;
using TMPro;

public class LobbyManager : MonoBehaviour
{
    [Header("Lobby")]
    public static LobbyManager Instance { get; set;}
    public event EventHandler<OnLobbyListChangedEventArgs> OnLobbyListChanged;
    private Lobby joinedLobby;
    private float heartBeatTimer;
    private float pollTimer;
    private bool hasLeftLobby;
    public Button startButton;
    public TMP_Text lobbyStatus;
    public GameObject leaveLobbyButton;
    private List<Player> lobbyPlayers = new List<Player>();

    [Header("UI")]
    public GameObject menuUI;
    public GameObject gameUI;
    public GameObject lobbyUI;
    public TMP_Text joinCodeText;

    public class OnLobbyListChangedEventArgs : EventArgs
    {
        public List<Lobby> lobbyList;
    }


    private async void Start()
    {
        InitializationOptions initializationOptions = new InitializationOptions();
        initializationOptions.SetProfile(UnityEngine.Random.Range(1,1000).ToString());
        await UnityServices.InitializeAsync(initializationOptions);
        await AuthenticationService.Instance.SignInAnonymouslyAsync();
    }

    private void Update()
    {
        HandleHeartBeatLobby();
        HandlePollForUpdates();
    }

    private void Awake()
    {
        Instance = this;
        Application.wantsToQuit += OnWantsToQuit;
    }

    private bool OnWantsToQuit()
    {
        if (!hasLeftLobby && joinedLobby != null)
        {
            LeaveLobbyAndQuit();
            return false;
        }
        return true;
    }

    private async void LeaveLobbyAndQuit()
    {
        try
        {
            await LobbyService.Instance.RemovePlayerAsync(joinedLobby.Id, AuthenticationService.Instance.PlayerId);
        }
        catch { } 
        finally 
        {
          hasLeftLobby = true;
          Application.Quit();
        }
    }

    private async void HandlePollForUpdates()
    {
        if (joinedLobby != null)
        {
            pollTimer -= Time.deltaTime;
            if(pollTimer < 0f)
            {
                float pollTimerMax = 2f;
                pollTimer = pollTimerMax;

                Lobby lobby  = await LobbyService.Instance.GetLobbyAsync(joinedLobby.Id);
                joinedLobby = lobby;
            }

            if(joinedLobby.Data["Relay Code"].Value != "0")
            {
                if (!IsLobbyHost())
                {
                   leaveLobbyButton.SetActive(false);
                   RelayManager.Instance.JoinRelay(joinedLobby.Data["Relay Code"].Value);
                }
                joinedLobby = null;
                QueryLobbies();
            }
        }

    }

    private async void HandleHeartBeatLobby()
    {
        if (joinedLobby != null && IsLobbyHost())
        {
            heartBeatTimer -= Time.deltaTime;
            if(heartBeatTimer < 0f)
            {
                float heartBeatTimerMax = 15;
                heartBeatTimer = heartBeatTimerMax;

                await LobbyService.Instance.SendHeartbeatPingAsync(joinedLobby.Id);
            }
        }
    }

    public async void CreateLobby(string lobbyName, int maxPlayers, bool isPrivate, int startingMoney)
    {
        CreateLobbyOptions options = new CreateLobbyOptions();
        options.IsPrivate = isPrivate;
        options.Player = GetPlayer();
        options.Data = new Dictionary<string, DataObject>()
        {
            {
                "Starting Money", new DataObject( DataObject.VisibilityOptions.Public, $"{startingMoney}")
            },
            {
                "Relay Code", new DataObject(DataObject.VisibilityOptions.Member, "0")
            },
        };
        Lobby lobby = await LobbyService.Instance.CreateLobbyAsync(lobbyName,maxPlayers,options);
        joinedLobby = lobby;
        lobbyUI.SetActive(true);
        Debug.Log($"Successfully created a lobby! {lobby.Name},{lobby.MaxPlayers} {lobby.LobbyCode}, {lobby.Players}");
        joinCodeText.text = $"{lobby.LobbyCode}";
        LoadLobby(true);
    }

    public void OnLeaveLobbyClicked()
    {
        LeaveLobby();
        NetworkManager.Singleton.Shutdown();
    }

    public void CopyToClipboard()
    {
        TextEditor editor = new TextEditor();
        editor.text = joinCodeText.text;
        editor.SelectAll();
        editor.Copy();
    }

    private async void LeaveLobby()
    {
        string playerId = AuthenticationService.Instance.PlayerId;
        await LobbyService.Instance.RemovePlayerAsync(joinedLobby.Id, playerId);
        joinedLobby = null;
        lobbyPlayers.Clear();
        lobbyUI.SetActive(false);
        gameUI.SetActive(false);
        menuUI.SetActive(true);
    }


    private bool IsLobbyHost()
    {
        return joinedLobby != null && joinedLobby.HostId == AuthenticationService.Instance.PlayerId;
    }

    public async void JoinLobbyById(string lobbyId)
    {
        try
        {
           JoinLobbyByIdOptions joinIdLobbyOptions = new JoinLobbyByIdOptions
           {
               Player = GetPlayer()
           };
           Lobby lobby = await LobbyService.Instance.JoinLobbyByIdAsync(lobbyId, joinIdLobbyOptions);
           joinedLobby = lobby;
           joinCodeText.text = $"{lobby.LobbyCode}";
           LoadLobby(false);
        }
        catch
        {
            return;
        }
    }

    public async void JoinLobbyByCode(string code)
    {
        try
        {
           JoinLobbyByCodeOptions joinCodeLobbyOptions = new JoinLobbyByCodeOptions
           {
               Player = GetPlayer()
           };
           Lobby lobby = await LobbyService.Instance.JoinLobbyByCodeAsync(code, joinCodeLobbyOptions);
           joinedLobby = lobby;
           joinCodeText.text = $"{lobby.LobbyCode}";
           LoadLobby(false);
        }
        catch
        {
            JoinLobby.Instance.errorText.text = "Unable to join to lobby.";
            JoinLobby.Instance.joinButton.enabled = false;
            JoinLobby.Instance.ClearText();
        }
    }

    public Lobby GetLobby()
    {
        return joinedLobby;
    }

    private void LoadLobby(bool isHost)
    {
        if (isHost)
        {
            menuUI.SetActive(false);
            gameUI.SetActive(true);
            lobbyUI.SetActive(true);
            startButton.gameObject.SetActive(true);
            lobbyStatus.text = "Press 'Start' to start the game.";

        }
        else
        {
            menuUI.SetActive(false);
            gameUI.SetActive(true);
            lobbyUI.SetActive(true);
            startButton.gameObject.SetActive(false);
            lobbyStatus.text = "Waiting for the host to begin the game";
        }
    }

    public async void QueryLobbies()
    {
        QueryResponse queryResponse = await LobbyService.Instance.QueryLobbiesAsync();
        OnLobbyListChanged?.Invoke(this, new OnLobbyListChangedEventArgs {lobbyList = queryResponse.Results});
    }

    public void OnStartButtonClicked()
    {
        StartGame();
    }

    public async void StartGame()
    {
       if(IsLobbyHost())
       {
            try
            {
                if (joinedLobby.Players.Count < 2)
                {
                    lobbyStatus.text = "You must need at least 2 players to start the game!";
                    startButton.enabled = false;
                    Invoke(nameof(ResetLobbyStatusText), 5f);
                    return;
                }
                else
                {

                    Debug.Log("Starting Game...");
                    startButton.gameObject.SetActive(false);
                    leaveLobbyButton.SetActive(false);
                    GameManager.Instance.SetStartingGold(Convert.ToInt32(joinedLobby.Data["Starting Money"].Value));
                    string relayCode = await RelayManager.Instance.CreateRelay(joinedLobby.Players.Count);
                    Lobby lobby = await Lobbies.Instance.UpdateLobbyAsync(joinedLobby.Id, new UpdateLobbyOptions{
                    IsLocked = true,
                    IsPrivate = true,
                    Data = new Dictionary<string, DataObject>
                    {
                        {"Relay Code", new DataObject(DataObject.VisibilityOptions.Member, relayCode) }
                    }
                });

                   joinedLobby = lobby;
                }

            }
            catch (LobbyServiceException e)
            {
                Debug.Log(e);
            }
       }
    }   

    private void ResetLobbyStatusText()
    {
        lobbyStatus.text = "Press 'Start' to start the game.";
        startButton.enabled = true;
    }

    private Player GetPlayer()
    {
        return new Player
        {
            Data = new Dictionary<string, PlayerDataObject>
            {
                {"PlayerName", new PlayerDataObject(PlayerDataObject.VisibilityOptions.Member, Login.Instance.GetUsername())}
            }
        };
    } 
}