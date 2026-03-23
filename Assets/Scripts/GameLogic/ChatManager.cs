using UnityEngine;
using TMPro;
using System.Collections.Generic;
using Unity.Netcode;

public class ChatManager : NetworkBehaviour
{
    //TO:DO Add rate limiter to prevent overhead.
    private string username;
    public TMP_InputField chatBox;
    public GameObject chatPanel;
    public GameObject textObject;
    private List<Message> messages = new List<Message>();
    private int maxMessages = 30;
    public static ChatManager Instance {get; set;}
    private void Awake()
    {
        Instance = this;
    }

    public override void OnNetworkSpawn()
    {
        username = Login.Instance.GetUsername();
    }
    public override void OnNetworkDespawn()
    {
        for(int i = 0; i < messages.Count; i++)
        {
            Destroy(messages[i].textObject.gameObject);
            messages.Remove(messages[i]);
        }
    } 

    private void Update()
    {
        if(chatBox.text != "")
        {
            if (Input.GetKeyDown(KeyCode.Return))
            {
                SendMessageServerRpc($"[{username}]: {chatBox.text}", Color.white);
                chatBox.text = "";
            }
                
        }
    }

    [Rpc(SendTo.Server)]
    private void SendMessageServerRpc(string text, Color colour)
    {
        BroadcastMessageClientRpc(text, colour);
    }

    [ClientRpc]
    public void BroadcastMessageClientRpc(string text, Color colour)
    {
        ProcessMessage(text, colour);
    }

    public void ProcessMessage(string text, Color colour)
    {
        if (messages.Count > maxMessages)
        {
            Destroy(messages[0].textObject.gameObject);
            messages.Remove(messages[0]);
        }
        Message message = new Message
        {
            text = text
        };
        GameObject newText = Instantiate(textObject, chatPanel.transform);
        message.textObject = newText.GetComponent<TMP_Text>();
        message.textObject.text = message.text;
        message.textObject.color = colour;
        messages.Add(message);
    }

    public class Message
    {
        public string text;
        public TMP_Text textObject;
    }
}
