using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;
//manager to persistently store local player information across scenes
public class LocalPlayerManager : MonoBehaviour
{
    //singleton reference to self
    public static LocalPlayerManager Singleton;
    
    //the local players deck
    public DeckData localPlayerDeck;
    public Deck localPlayerDeckObj;

    //the local players hand
    public GameObject localPlayerHand;
    public GameObject localPlayerHandParent;
    
    //all of the loaded deck data
    public Dictionary<string,DeckData> allDeckData = new Dictionary<string,DeckData> ();
    
    //the local anchor 
    public GameObject localSpaceAnchor;

    //count of players before the game begins
    public int playerCount;

    //a bool to pass if a token is being added
    public bool addingToken;

    public bool lockedTable = false;

    private void Awake()
    {
        if (GameObject.FindObjectsByType<LocalPlayerManager>(FindObjectsSortMode.None).Length > 1)
        {
            Destroy(gameObject);
        }
        
        //get reference to self and set so persists through scenes
        if (Singleton != null) {
            Destroy(gameObject);
            return;
        }
        Singleton = this;
        DontDestroyOnLoad(gameObject);
        
        addingToken = false;
        playerCount = 2;
    }

    //set the local player deck
    public void SetLocalPlayerDeck(DeckData deckData)
    {
        localPlayerDeck = deckData;
    }

    public DeckData GetLocalPlayerDeck()
    {
        return localPlayerDeck;
    }

    public void SetPlayerCount(bool singleplayer)
    {
        if (singleplayer)
        {
            playerCount = 1;
        }
        else
        {
            playerCount = 2;
        }
    }
}

