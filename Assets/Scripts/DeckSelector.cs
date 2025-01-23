using Oculus.Platform;
using System.Collections.Generic;
using System.IO;
using UnityEngine;



public class DeckSelector : MonoBehaviour
{

    public List<DeckData> decks;

    [SerializeField]
    CardDataFetcher cardFetcher;

    [SerializeField]
    GameObject deckChoicePrefab;

    [SerializeField]
    GameObject canvasObject;

    string saveFolder;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
       

       

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
            deckChoicePanel.transform.localPosition = new Vector3(-600 + (i * 350), 150, 0);


            string cardImagePath = Path.Combine(saveFolder, decks[i].cardsStartOut[0] + ".png");
            byte[] fileData;
            if (File.Exists(cardImagePath))
            {
                fileData = File.ReadAllBytes(cardImagePath);
                Texture2D tex = new Texture2D(2, 2);
                tex.LoadImage(fileData);
                Sprite sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f), 100f);
                deckChoicePanel.SetImage(sprite);

            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
