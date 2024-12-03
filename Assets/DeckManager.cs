using UnityEngine;

public class DeckManager : MonoBehaviour
{
    public static DeckManager Singleton;

    public Deck[] decks;

    private void Awake()
    {
        Singleton = this;
    }

    public void Start()
    {
        for (int i = 0; i < decks.Length; i++)
        {
            decks[i].deckID = (ulong)i;
        }
    }

}
