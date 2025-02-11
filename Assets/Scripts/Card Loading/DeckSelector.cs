using Oculus.Platform;
using System.Collections.Generic;
using System.IO;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;



public class DeckSelector : MonoBehaviour
{

    public List<DeckData> decks;

    [SerializeField]
    CardDataFetcher cardFetcher;

    [SerializeField]
    GameObject deckChoicePrefab;

    [SerializeField]
    GameObject canvasObject;

    [SerializeField]
    GameObject text;

    [SerializeField]
    GameObject selectedText;

    string saveFolder;

    DeckData selectedDeck;

    public void SetSelectedDeck(DeckData newDeck)
    {
        selectedDeck = newDeck;
    }

    public void ShowDecks()
    {
        decks = cardFetcher.GetDecks();

        saveFolder = cardFetcher.GetSaveFolder();

        for (int i = 0; i < decks.Count; i++)
        {
            GameObject newPanel = Instantiate(deckChoicePrefab);

            DeckChoicePanel deckChoicePanel = newPanel.GetComponent<DeckChoicePanel>();
            deckChoicePanel.deckData = decks[i];
            deckChoicePanel.UpdatePanel();

            deckChoicePanel.transform.SetParent(canvasObject.transform, false);
            deckChoicePanel.transform.localScale = Vector3.one;
            deckChoicePanel.transform.localPosition = new Vector3(-800 + (i * 350), 200, 0);

            deckChoicePanel.selector = this;

            Texture2D tex = decks[i].cardImages[decks[i].cardsStartOut[0]];

            Sprite sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f), 100f);
            deckChoicePanel.SetImage(sprite);

            
        }
    }

    public void SubmitChosenDeck()
    {
        if (!selectedDeck.IsUnityNull())
        {
            LocalPlayerManager.Singleton.SetLocalPlayerDeck(selectedDeck);
            SceneManager.LoadScene("Gameplay");
        }
    }

    public void UpdateDeckText()
    {
        TextMeshProUGUI textComp = text.GetComponent<TextMeshProUGUI>();
        textComp.text = "";

        selectedText.GetComponent<TextMeshProUGUI>().text = "Selected: " +selectedDeck.deckName;
        foreach (string cardName in selectedDeck.cardsInDeck) {
            textComp.text += cardName + "\n";
         }
    }
}


