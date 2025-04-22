using System.Collections;
using Oculus.Platform;
using System.Collections.Generic;
using System.IO;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;



public class DeckSelector : MonoBehaviour
{
    
    //list of decks to be selected
    public List<DeckData> decks;

    
    //the card fetcher component
    [SerializeField]
    CardDataFetcher cardFetcher;

    //prefab of the panels to display decks to player
    [SerializeField]
    GameObject deckChoicePrefab;

    //canvas to parent new panels to
    [SerializeField]
    GameObject canvasObject;

    //text to display the contents of the selected deck
    [SerializeField]
    GameObject text;

    //text to display the name of the selected deck
    [SerializeField]
    GameObject selectedText;

    //reference to the data of the selected deck
    DeckData selectedDeck;

    //update the selected deck
    public void SetSelectedDeck(DeckData newDeck)
    {
        selectedDeck = newDeck;
    }

    //for every deck in the decklist create a panel and load it's details
    public void ShowDecks()
    {
        //load decks from the card fetcher
        decks = cardFetcher.GetDecks();

        int xCounter = 0;
        int yCounter = 0;
        
        for (int i = 0; i < decks.Count; i++)
        {
            if (xCounter >= 3)
            {
                xCounter = 0;
                yCounter++;
            }
            
            //create the panel
            GameObject newPanel = Instantiate(deckChoicePrefab);

            //set panel details
            DeckChoicePanel deckChoicePanel = newPanel.GetComponent<DeckChoicePanel>();
            deckChoicePanel.deckData = decks[i];
            deckChoicePanel.UpdatePanel();

            //set panel position
            deckChoicePanel.transform.SetParent(canvasObject.transform, false);
            deckChoicePanel.transform.localScale = Vector3.one;
            deckChoicePanel.transform.localPosition = new Vector3(-800 + (xCounter * 350), 200-yCounter * 450, 0);

            //parent this to the panel
            deckChoicePanel.selector = this;

            //set the texture of the panel to the deck's first card in cardsStartOut
            Texture2D tex = decks[i].cardImages[decks[i].cardsStartOut[0]];
            Sprite sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f), 100f);
            deckChoicePanel.SetImage(sprite);

            xCounter++;
        }
    }

    //if the selected deck is valid, load to gameplay
    public void SubmitChosenDeck()
    {
        if (!selectedDeck.IsUnityNull())
        {
            LocalPlayerManager.Singleton.SetLocalPlayerDeck(selectedDeck);
            StartCoroutine(LoadSceneAsync());
        }
    }

    //update the deck list with the title of the newly selected deck
    public void UpdateDeckText()
    {
        TextMeshProUGUI textComp = text.GetComponent<TextMeshProUGUI>();
        textComp.text = "";

        selectedText.GetComponent<TextMeshProUGUI>().text = "Selected: " + selectedDeck.deckName;
        //add the card name to the text and a line break
        foreach (string cardName in selectedDeck.cardsInDeck)
        {
            textComp.text += cardName + "\n";
        }
    }

    //load scene asynchronously to stop stuttering
    IEnumerator LoadSceneAsync()
    {
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync("02 - Gameplay");

        while (!asyncLoad.isDone)
        {
            yield return null;
        }
    }
}


