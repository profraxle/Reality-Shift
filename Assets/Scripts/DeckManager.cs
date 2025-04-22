using System;
using Unity.Netcode;
using UnityEngine;

public class DeckManager : MonoBehaviour
{
    //this manager class manages the decks and is used to get reference to them
    public static DeckManager Singleton;
    public GameObject DeckPrefab;

    public bool spawnedDecks;
    public Vector3[] spawns;
    public Deck[] decks = new Deck[4];
    
    //the surface reference and anchor references are also stored here as it doesnt make sense to make new managers
    public GameObject surface;
    
    public GameObject [] anchors = new GameObject[4];

    private void Awake()
    {
        Singleton = this;
    }

    public void Start()
    {
        spawnedDecks = false;
    }

    public void SpawnDecks()
    {
        //if this deck manager is the server's spawn all the decks in the game
        if (NetworkManager.Singleton.IsServer) {

            for (int i = 0; i < NetworkManager.Singleton.ConnectedClientsList.Count; i++)
            {
                GameObject newDeckObject = Instantiate(DeckPrefab);
                newDeckObject.transform.position = spawns[i];
                
                if (i % 2 != 0)
                {
                    newDeckObject.transform.eulerAngles = new Vector3(0, 180, 0);
                }

                newDeckObject.GetComponent<Deck>().deckID = new NetworkVariable<ulong>((ulong)i);
                newDeckObject.GetComponent<Deck>().playerID = new NetworkVariable<ulong>((ulong)i);

                newDeckObject.GetComponent<NetworkObject>().SpawnWithOwnership((ulong)i);
                
            }
            spawnedDecks = true;

        }



        
    }

}
