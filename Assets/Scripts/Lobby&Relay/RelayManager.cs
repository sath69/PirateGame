using UnityEngine;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using System.Threading.Tasks;
using Unity.Netcode;
using Unity.Networking.Transport.Relay;
using Unity.Netcode.Transports.UTP;

public class RelayManager : MonoBehaviour
{
    public static RelayManager Instance {get; set;}
    public string region;

    private void Awake()
    {
        Instance = this;
    }
    
    public async Task<string> CreateRelay(int maxPlayers)
    {
        try
        {
            Allocation allocation = await RelayService.Instance.CreateAllocationAsync(maxPlayers-1);
            string joinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);
            region = allocation.Region;
            GameUIManager.Instance.regionText.text= $"Server region: {region}";
            RelayServerData relayServerData = new RelayServerData(allocation, "dtls");

            NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(relayServerData);
            NetworkManager.Singleton.StartHost();
            Debug.Log("Successfully created Relay Instance!");

   
            return joinCode;
        }
        catch(RelayServiceException e)
        { 
            Debug.Log($"Error creating Relay allocation, {e}");
            return null;
        }
    }

    public async void JoinRelay(string joinCode)
    {
        try
        {
            Debug.Log($"Joining Relay Server using code {joinCode}");
            JoinAllocation joinAllocation = await RelayService.Instance.JoinAllocationAsync(joinCode);
            region = joinAllocation.Region;
            GameUIManager.Instance.regionText.text = $"Server region: {region}";
            RelayServerData relayServerData = new RelayServerData(joinAllocation, "dtls");
            NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(relayServerData);
            NetworkManager.Singleton.StartClient();
        } 
        catch(RelayServiceException e)
        { 
            Debug.Log($"Error joining Relay server, {e}");
        }
        
    }
}
