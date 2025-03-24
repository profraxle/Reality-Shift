using UnityEngine;
using Unity.Netcode;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using Unity.Netcode.Transports.UTP;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;

public class NetworkConnect : MonoBehaviour
{
    public int maxConnection = 4;
    public UnityTransport transport;

    private Lobby currentLobby;
    private float heartBeatTimer;

   

    private async void Awake()
    {
        await UnityServices.InitializeAsync();
        await AuthenticationService.Instance.SignInAnonymouslyAsync();

        if (IsPortInUse(transport.ConnectionData.Port))
        {
            transport.ConnectionData.Port += 10;
        }
        
        
        JoinOrCreate();
    }

    public async void JoinOrCreate()
    {
        try
        {
            currentLobby = await Lobbies.Instance.QuickJoinLobbyAsync();
            string relayJoinCode = currentLobby.Data["JOIN_CODE"].Value;

            JoinAllocation allocation = await RelayService.Instance.JoinAllocationAsync(relayJoinCode);

            transport.SetClientRelayData(allocation.RelayServer.IpV4, (ushort)allocation.RelayServer.Port,
                allocation.AllocationIdBytes, allocation.Key, allocation.ConnectionData, allocation.HostConnectionData);

            NetworkManager.Singleton.StartClient();
        }
        catch
        {
            Create();
        }
    }

    public async void Create()
    {

        Allocation allocation = await RelayService.Instance.CreateAllocationAsync(maxConnection);
        string newJoinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);

        Debug.LogError(newJoinCode);

        transport.SetHostRelayData(allocation.RelayServer.IpV4,(ushort) allocation.RelayServer.Port,
            allocation.AllocationIdBytes,allocation.Key,allocation.ConnectionData);

        CreateLobbyOptions lobbyOptions = new CreateLobbyOptions();
        lobbyOptions.IsPrivate = false;
        lobbyOptions.Data = new Dictionary<string, DataObject>();
        DataObject dataObject = new DataObject(DataObject.VisibilityOptions.Public, newJoinCode);
        lobbyOptions.Data.Add("JOIN_CODE",dataObject);

        currentLobby = await Lobbies.Instance.CreateLobbyAsync("Lobby Name", maxConnection,lobbyOptions);

        NetworkManager.Singleton.StartHost();
    }

    public async void Join()
    {
        currentLobby = await Lobbies.Instance.QuickJoinLobbyAsync();
        string relayJoinCode = currentLobby.Data["JOIN_CODE"].Value;

        JoinAllocation allocation = await RelayService.Instance.JoinAllocationAsync(relayJoinCode);

        transport.SetClientRelayData(allocation.RelayServer.IpV4, (ushort)allocation.RelayServer.Port,
            allocation.AllocationIdBytes, allocation.Key, allocation.ConnectionData,allocation.HostConnectionData);

        NetworkManager.Singleton.StartClient();
    }

    private void Update()
    {
        if (heartBeatTimer > 15)
        {
            heartBeatTimer -= 15;

            if (currentLobby != null && currentLobby.HostId == AuthenticationService.Instance.PlayerId)
            {
                LobbyService.Instance.SendHeartbeatPingAsync(currentLobby.Id);
            }

        }
        heartBeatTimer += Time.deltaTime;

        if (NetworkManager.Singleton.IsServer)
        {
            if (NetworkManager.Singleton.ConnectedClientsList.Count >= 2   && !DeckManager.Singleton.spawnedDecks)
            {
                
                DeckManager.Singleton.SpawnDecks();
                
                
            }
        }
    }
    
    public bool IsPortInUse(int port)
    {
        bool isInUse = false;

        TcpListener listener = null;
        try
        {
            listener = new TcpListener(IPAddress.Any, port);
            listener.Start();
        }
        catch (SocketException)
        {
            isInUse = true;
        }
        finally
        {
            listener?.Stop();
        }

        return isInUse;
    }
}
