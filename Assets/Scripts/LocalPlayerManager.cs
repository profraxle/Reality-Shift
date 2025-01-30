using UnityEngine;

public class LocalPlayerManager : MonoBehaviour
{
    public static LocalPlayerManager instance;
    public DeckData localPlayerDeck;

    private void Awake()
    {

        if (instance != null) {
            Destroy(gameObject);
            return;
        }
        instance = this;
        DontDestroyOnLoad(gameObject);
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

