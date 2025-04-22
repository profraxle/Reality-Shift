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
    //the maximum connections
    public int maxConnection = 2;
    
    //unity transport
    public UnityTransport transport;

    //the lobby
    private Lobby currentLobby;
    
    //the heartbeat to pulse on the lobby screen
    private float heartBeatTimer;
    
    private async void Awake()
    {
        //initialise unity services
        await UnityServices.InitializeAsync();
        
        //log in to the authentication service
        await AuthenticationService.Instance.SignInAnonymouslyAsync();
        
        //either join or create a lobby
        JoinOrCreate();
    }

    public async void JoinOrCreate()
    {
        if (LocalPlayerManager.Singleton.playerCount == 1)
        {
            CreateLocalHost();
        }
        else
        {

            //try to join a lobby with the lobbies service using the joincode got from the lobby listed 
            try
            {
                currentLobby = await Lobbies.Instance.QuickJoinLobbyAsync();
                string relayJoinCode = currentLobby.Data["JOIN_CODE"].Value;

                JoinAllocation allocation = await RelayService.Instance.JoinAllocationAsync(relayJoinCode);

                transport.SetClientRelayData(allocation.RelayServer.IpV4, (ushort)allocation.RelayServer.Port,
                    allocation.AllocationIdBytes, allocation.Key, allocation.ConnectionData,
                    allocation.HostConnectionData);

                //start a client to connect to the found lobby
                NetworkManager.Singleton.StartClient();
            }
            catch
            {
                //create a lobby if none are found
                Create();
            }
        }
    }

    public async void Create()
    {
        //make an allocation with the relay (rerouting) service
        Allocation allocation = await RelayService.Instance.CreateAllocationAsync(maxConnection);
        //create a join code for to connect through relay
        string newJoinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);

        //output the join code in the console
        Debug.LogError(newJoinCode);

        //initialise the data for the relay and set the transport data
        transport.SetHostRelayData(allocation.RelayServer.IpV4,(ushort) allocation.RelayServer.Port,
            allocation.AllocationIdBytes,allocation.Key,allocation.ConnectionData);

        //create the lobby information and tie the join code to the listing registered to the list of lobbies
        CreateLobbyOptions lobbyOptions = new CreateLobbyOptions();
        lobbyOptions.IsPrivate = false;
        lobbyOptions.Data = new Dictionary<string, DataObject>();
        DataObject dataObject = new DataObject(DataObject.VisibilityOptions.Public, newJoinCode);
        lobbyOptions.Data.Add("JOIN_CODE",dataObject);

        //register the create lobby to the lobbies service
        currentLobby = await Lobbies.Instance.CreateLobbyAsync("Lobby Name", maxConnection,lobbyOptions);

        //start hosting
        NetworkManager.Singleton.StartHost();
    }

    //use this for singleplayer games!
    public async void CreateLocalHost()
    {
        UnityTransport []transports = GetComponents<UnityTransport>();
        foreach (UnityTransport nTransport in transports)
        {
            if (nTransport.Protocol == UnityTransport.ProtocolType.UnityTransport)
            {
                NetworkManager.Singleton.NetworkConfig.NetworkTransport = nTransport;
                transport = nTransport;
            }
        }
        
        //starts hosting without registering to lobby
        NetworkManager.Singleton.StartHost();
    }

    //join a lobby in the lobbies list with the join code
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
        //countdown the heartbeat timer then send out a heartbeat ping to keep the server in the list
        if (heartBeatTimer > 15)
        {
            heartBeatTimer -= 15;

            if (currentLobby != null && currentLobby.HostId == AuthenticationService.Instance.PlayerId)
            {
                LobbyService.Instance.SendHeartbeatPingAsync(currentLobby.Id);
            }

        }
        heartBeatTimer += Time.deltaTime;

        //if this is the server, spawn the decks when 2 or more people are connected
        if (NetworkManager.Singleton.IsServer)
        {
            if (NetworkManager.Singleton.ConnectedClientsList.Count >= LocalPlayerManager.Singleton.playerCount   && !DeckManager.Singleton.spawnedDecks)
            {
                
                DeckManager.Singleton.SpawnDecks();
                
                
            }
        }
    }
    
    
}
