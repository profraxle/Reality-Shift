using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

public class LocalPlayerManager : MonoBehaviour
{
    public static LocalPlayerManager Singleton;
    public DeckData localPlayerDeck;
    public Deck localPlayerDeckObj;

    public GameObject localPlayerHand;

    public Dictionary<string,DeckData> allDeckData = new Dictionary<string,DeckData> ();
    
    public GameObject localSpaceAnchor;

    public bool addingToken;

    private void Awake()
    {

        if (Singleton != null) {
            Destroy(gameObject);
            return;
        }
        Singleton = this;
        DontDestroyOnLoad(gameObject);
        
        addingToken = false;
    }

    public void SetLocalPlayerDeck(DeckData deckData)
    {
        localPlayerDeck = deckData;
    }

    public DeckData GetLocalPlayerDeck()
    {
        return localPlayerDeck;
    }
}

