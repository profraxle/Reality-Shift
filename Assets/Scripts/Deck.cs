using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using System.Linq;
using System;
using System.Collections;
using Unity.Collections;
using Oculus.Interaction;
using Vector3 = UnityEngine.Vector3;


public class Deck : CardPile
{
    public NetworkVariable<ulong> deckID;

    public SelectorUnityEventWrapper selector;

    [SerializeField]
    private GameObject cardZonePrefab;

    void Awake()
    {
        faceUp = false;
    }
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        base.Start();
        faceUp = false;
        
        
        if (deckID.Value == NetworkManager.LocalClientId)
        {
            deckData = LocalPlayerManager.Singleton.GetLocalPlayerDeck();

            foreach (string card in deckData.cardsInDeck)
            {
                cardsInPile.Push(card);
            }
            shuffle();
            
            //creates a first card to be grabbable on top of the deck

            List<FixedString128Bytes> currentDeckSendable = new List<FixedString128Bytes>(cardsInPile.Count);

            foreach (var cardName in cardsInPile)
            {
                currentDeckSendable.Add(new FixedString128Bytes(cardName));
            }

            if (NetworkManager.Singleton.IsClient)
            {
                //send strings of current deck order
                ClientConnectedServerRpc(deckData.deckName, currentDeckSendable.ToArray());
                SpawnNextCardInPileServerRpc();
            }
            else
            {
                ClientConnectedClientRpc(deckData.deckName, currentDeckSendable.ToArray());
                SpawnNextCardInPile();
            }
            
        }

        StartCoroutine(SpawnZones());
    }

    IEnumerator SpawnZones()
    {
        //wait so position of deck will be correct when called
        yield return new WaitForSeconds(0.5f);

        for (int i = 0; i < 2; i++)
        {
            GameObject newZone =Instantiate(cardZonePrefab, transform.position + (0.8f*transform.forward) + (0.15f*transform.right*i), transform.rotation);
            newZone.GetComponent<CardZone>().PassDeckData(this);
            
            SpawnNetworkObjectServerRpc(newZone);
        }
        
        UpdatePileHeight();
    }

    void shuffle()
    {
        //fisher-yates shuffle algo
        string[] deckShuffle = cardsInPile.ToArray();

        for (int i = deckShuffle.Length - 1; i > 0; i--)
        {
            int j = UnityEngine.Random.Range(0, i + 1);
            (deckShuffle[i], deckShuffle[j]) = (deckShuffle[j], deckShuffle[i]);
        }

        cardsInPile = new Stack<string>(deckShuffle);
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
    
    




    

}
