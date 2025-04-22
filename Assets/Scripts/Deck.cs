using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using System.Linq;
using System;
using System.Collections;
using Unity.Collections;
using Oculus.Interaction;
using Vector3 = UnityEngine.Vector3;

//extension of the cardpile to be used as the deck the players use to play the game
//creates the other zones the player uses
public class Deck : CardPile
{
    //network synced deck ID
    public NetworkVariable<ulong> deckID;

    //prefab for the card zones
    [SerializeField]
    private GameObject cardZonePrefab;

    //the life tracker that will be created for the players
    [SerializeField]
    private GameObject lifeTrackerPrefab;
    
    //texture for spawned tokens
    [SerializeField]
    private Texture tokenTexture;

    //set this to be facedown
    void Awake()
    {
        faceUp = false;
    }
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    new void Start()
    {
        base.Start();
        faceUp = false;
        
        //if this is local to the client
        if (deckID.Value == NetworkManager.LocalClientId)
        {
            //load in the deckdata from the previous scene
            deckData = LocalPlayerManager.Singleton.GetLocalPlayerDeck();
            //set this as the local player deck
            LocalPlayerManager.Singleton.localPlayerDeckObj = this;
            
            //construct the cards in pile from the deck data
            foreach (string card in deckData.cardsInDeck)
            {
                cardsInPile.Push(card);
            }
            //shuffle the deck
            shuffle();
            
            //creates a first card to be grabbable on top of the deck

            //send the information from this shuffled deck to the server
            List<FixedString128Bytes> currentDeckSendable = new List<FixedString128Bytes>(cardsInPile.Count);

            foreach (var cardName in cardsInPile)
            {
                currentDeckSendable.Add(new FixedString128Bytes(cardName));
            }

            if (NetworkManager.Singleton.IsServer)
            {
                //send strings of current deck order
                ClientConnectedClientRpc(deckData.deckName, currentDeckSendable.ToArray());
                SpawnNextCardInPile();
            }
            else
            {
                
                ClientConnectedServerRpc(deckData.deckName, currentDeckSendable.ToArray());
                SpawnNextCardInPileServerRpc();
            }
            
            //spawn the cardzones for GY and Exile
            StartCoroutine(SpawnZones());
        }

       
    }

    IEnumerator SpawnZones()
    {
        //wait so position of deck will be correct when called
        yield return new WaitForSeconds(0.5f);

        SpawnZonesServerRpc();
        SpawnLifeTrackerServerRpc();
        
    }

    void shuffle()
    {
        //fisher-yates shuffle algorithm
        string[] deckShuffle = cardsInPile.ToArray();

        for (int i = deckShuffle.Length - 1; i > 0; i--)
        {
            int j = UnityEngine.Random.Range(0, i + 1);
            (deckShuffle[i], deckShuffle[j]) = (deckShuffle[j], deckShuffle[i]);
        }

        cardsInPile = new Stack<string>(deckShuffle);
    }

    //call a shuffle on the server
    [ServerRpc(RequireOwnership = false)]
    void ShuffleServerRpc()
    {
        shuffle();
        
        UpdateDrawableCardServerRpc(cardsInPile.Peek());
    }

    //call a shuffle on the server
    public void ShuffleButtonPressed()
    {
        ShuffleServerRpc();
    }

    //load deck data from given name and send deck list info to other clients
    [ServerRpc]
    public void ClientConnectedServerRpc(string deckName, FixedString128Bytes[]nCurrentDeck )
    {
        ClientConnectedClientRpc(deckName,nCurrentDeck);
        
        cardsInPile = new Stack<String>();
        deckData = LocalPlayerManager.Singleton.allDeckData[deckName];
        for (int i = nCurrentDeck.Count() -1; i >= 0; i--)
        {
            cardsInPile.Push(nCurrentDeck[i].ToString());
        }
        
    }
    

    [ClientRpc]
    public void ClientConnectedClientRpc(string deckName, FixedString128Bytes[] nCurrentDeck)
    {
        if (deckID.Value != NetworkManager.LocalClientId)
        {
            cardsInPile = new Stack<String>();
            deckData = LocalPlayerManager.Singleton.allDeckData[deckName];
            for (int i = nCurrentDeck.Count() - 1; i >= 0; i--)
            {
                cardsInPile.Push(nCurrentDeck[i].ToString());
            }
        }
    }

    //spawn the other cardpiles to be used by the player
    [ServerRpc(RequireOwnership = false)]
    public void SpawnZonesServerRpc()
    {
        for (int i = 0; i < 2; i++)
        {
            GameObject newZone =Instantiate(cardZonePrefab, transform.position + (0.8f*transform.forward) + (0.15f*transform.right*i), transform.rotation);
            newZone.GetComponent<CardZone>().PassDeckData(this);
            newZone.GetComponent<CardZone>().playerID = new NetworkVariable<ulong>(playerID.Value);
            newZone.GetComponent<NetworkObject>().SpawnWithOwnership(playerID.Value);
            
            NetworkObjectReference zoneNetworkReference = new NetworkObjectReference(newZone);
            
            NewZoneClientRpc(zoneNetworkReference);
        }
    }

    //callback for the client for the zone creation
    [ClientRpc]
    public void NewZoneClientRpc(NetworkObjectReference cardObjectReference)
    {
        cardObjectReference.TryGet(out NetworkObject networkObject);

        networkObject.GetComponent<CardZone>().PassDeckData(this);
    }

    //create the lifetracker
    [ServerRpc(RequireOwnership = false)]
    public void SpawnLifeTrackerServerRpc()
    {
        GameObject newTracker =Instantiate(lifeTrackerPrefab, transform.position + (0.85f*transform.forward) + (0.1f * transform.up) ,  Quaternion.Euler(transform.eulerAngles.x, transform.eulerAngles.y+45, transform.eulerAngles.z));
        newTracker.GetComponent<NetworkObject>().SpawnWithOwnership(playerID.Value);
    }

    //spawn a token object
    public void SpawnToken()
    {
        SpawnTokenServerRpc();
    }
    
    //spawn a card with the token texture and name Token
    [ServerRpc(RequireOwnership = false)]
    public void SpawnTokenServerRpc()
    {
        GameObject newToken = Instantiate(cardPrefab,DeckManager.Singleton.surface.transform.position,Quaternion.Euler(-90,0,0));

        newToken.GetComponent<Card>().cardData = new CardData();
        newToken.GetComponent<Card>().cardData.name = "Token";
        
        newToken.GetComponent<NetworkObject>().SpawnWithOwnership(playerID.Value);
        
        NetworkObjectReference reference = new NetworkObjectReference(newToken);
        
        SpawnTokenClientRpc(reference);
    }

    //get the reference of the spawend token and set its position on the client
    [ClientRpc]
    public void SpawnTokenClientRpc(NetworkObjectReference reference)
    {
        reference.TryGet(out NetworkObject networkObject);

        networkObject.GetComponent<Card>().locked = false;
        networkObject.GetComponent<Card>().faceUp = true;
        
        networkObject.gameObject.transform.SetPositionAndRotation(DeckManager.Singleton.surface.transform.position,Quaternion.Euler(-90,0,DeckManager.Singleton.surface.transform.eulerAngles.y));
        
        List<Material> materials = new List<Material>();
        networkObject.gameObject.GetComponent<Renderer>().GetMaterials(materials);
        materials[2].mainTexture = tokenTexture;
    }


    

}
