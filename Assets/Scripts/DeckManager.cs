using System;
using Unity.Netcode;
using UnityEngine;

public class DeckManager : MonoBehaviour
{
    public static DeckManager Singleton;
    public GameObject DeckPrefab;

    public bool spawnedDecks;
    public Vector3[] spawns;
    public Deck[] decks = new Deck[4];
    public GameObject surface;

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

                
                //decks[i] = newDeckObject.GetComponent<Deck>();
            }
            spawnedDecks = true;

        }



        
    }

}
