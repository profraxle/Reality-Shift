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
    
    //all of the loaded deck data
    public Dictionary<string,DeckData> allDeckData = new Dictionary<string,DeckData> ();
    
    //the local anchor 
    public GameObject localSpaceAnchor;

    //a bool to pass if a token is being added
    public bool addingToken;

    private void Awake()
    {
        //get reference to self and set so persists through scenes
        if (Singleton != null) {
            Destroy(gameObject);
            return;
        }
        Singleton = this;
        DontDestroyOnLoad(gameObject);
        
        addingToken = false;
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
}

