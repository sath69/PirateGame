using System;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class CreateLobby : MonoBehaviour
{
    public TMP_Dropdown maxPlayers;
    public TMP_Dropdown isPrivate;
    public TMP_Dropdown startingGold;
    public Button createLobbyButton;

    public static CreateLobby Instance {get; set;}

    private void OnEnable()
    {
        createLobbyButton.enabled = true;
    }

    public bool IsPrivate()
    {
        if(isPrivate.value == 0)
        {
            return false;
        }
        else
        {
            return true;
        }
    }
    public int MaxPlayers()
    {
        return Convert.ToInt32(maxPlayers.options[maxPlayers.value].text);
    }
    public string LobbyName()
    {
        return $"{Login.Instance.GetUsername()}'s Lobby";
    }
    public int StartingGold()
    {
        return Convert.ToInt32(startingGold.options[startingGold.value].text);
    }

    public void OnCreateLobbyClicked()
    {
        createLobbyButton.enabled = false;
        LobbyManager.Instance.CreateLobby(LobbyName(), MaxPlayers(),  IsPrivate(), StartingGold());
    }
}
