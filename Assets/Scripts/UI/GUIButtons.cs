using UnityEngine;
using Unity.Netcode;

public class GUI : MonoBehaviour
{
    private NetworkManager networkManager;

    private void Awake() 
    {
        networkManager = GetComponent<NetworkManager>();
    }


    void OnGUI()
    {
        GUILayout.BeginArea(new Rect(10, 10, 300,300));
        if(!networkManager.IsClient && !networkManager.IsServer)
        { 
             StartButtons();
        } 

        GUILayout.EndArea();
    }

    private void StartButtons()
    {
        if(GUILayout.Button("Host")) networkManager.StartHost();
        if(GUILayout.Button("Server")) networkManager.StartServer();
        if(GUILayout.Button("Client")) networkManager.StartClient();
    }
}
