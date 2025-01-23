using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine.Networking;
using NUnit.Framework.Constraints;
using UnityEditor;
public class CardDataFetcher : MonoBehaviour
{
    //save URL for scryfall api
    private const string baseURL = "https://api.scryfall.com/cards/named?fuzzy=";
    
    //declare path for saving content
    private string saveFolder;
    
    //declare path for storing deckLists
    private string decklistFolder;
    private List<string> cardNames = new List<string>{};
    private List<DeckData> decks = new List<DeckData>{};

    public bool isFetching;

    public void Start()
    {
        isFetching = true;

        //combine path to create save location
        saveFolder = Path.Combine(Application.persistentDataPath,  "CardData");
        Directory.CreateDirectory(saveFolder);
        
        //combine path to create save location
        decklistFolder = Path.Combine(Application.persistentDataPath,  "Decklists");
        Directory.CreateDirectory(decklistFolder);
        
        //define cardNames to search
        
        
        ReadDecklists();
        StartCoroutine(DownloadAndSaveCards(cardNames));


    }

    public string GetSaveFolder()
    {
        return saveFolder;
    }

    public List<DeckData> GetDecks()
    {
        return decks;
    }

    private void ReadDecklists()
    {
        bool isCommander = false;

        //get all files from the directory
        var fileInfo = Directory.GetFiles(decklistFolder);
        foreach (string file in fileInfo)
        {
            //create a deck struct for the current decklist being read
            DeckData newDeck = new DeckData { };

            //create lists for the deck's different starting zones (on table and in deck)
            List<string> cardsInDeck = new List<string> { };
            List<string> cardsStartOut = new List<string> { };

            //read all of the lines in the current decklist
            string[] lines = File.ReadAllLines(file);
            
            foreach (string line in lines)
            {
                //skip blank lines
                if (string.IsNullOrWhiteSpace(line)) continue;
                
                //split the line into two parts
                string[] parts = line.Split(' ',2);
                
                //if string is splittable and a quantity can be extracted, extract it
                if (parts.Length == 2 && int.TryParse(parts[0], out int quantity))
                {
                    //get the card name from the first part of the line
                    string cardName = parts[1].Trim();

                    //add the cardName to the list to be downloaded
                    cardNames.Add(cardName);

                    //for the quantity extracted add that many copies of the card to the relevant list
                    for (int i = 0; i < quantity; i++)
                    {
                        //if isCommander flag is true, add to the commander list
                        if (isCommander)
                        {
                            cardsStartOut.Add(cardName);
                            isCommander = false;
                        }
                        else
                        {
                            //add to the deck list
                            cardsInDeck.Add(cardName);
                        }
                    }
                }
                else{
                    //if the line is simply "commander" add the next card as a commander
                    if (line == "commander")
                    {
                        isCommander = true;
                    }
                    else
                    {
                        //else, the line is interpreted as the Name of the deck
                        newDeck.deckName = line;
                        Debug.Log(line);
                    }
                }
            }
            newDeck.cardsInDeck = cardsInDeck;
            newDeck.cardsStartOut = cardsStartOut;
            decks.Add(newDeck);
        }
    }


    private IEnumerator DownloadAndSaveCards(List<string> cardNames)
    {
        foreach (string cardName in cardNames)
        {
            string sanitizedCardName = SanitizeFileName(cardName);
            string cardDataPath = Path.Combine(saveFolder, sanitizedCardName + ".json");
            string cardImagePath = Path.Combine(saveFolder, sanitizedCardName + ".png");
            
            if (File.Exists(cardDataPath)&& File.Exists(cardImagePath))
            {
                Debug.Log($"Card '{cardName}'already downloaded.");
                continue;
            }
            
            string url = baseURL + UnityWebRequest.EscapeURL(cardName);

            using (UnityWebRequest webRequest = UnityWebRequest.Get(url))
            {
                yield return webRequest.SendWebRequest();

                if (webRequest.result == UnityWebRequest.Result.ConnectionError ||
                    webRequest.result == UnityWebRequest.Result.ProtocolError)
                {
                    Debug.Log($"Error:{webRequest.error}");
                
                }
                else
                {
                    string jsonResponse = webRequest.downloadHandler.text;
                    Debug.Log($"Response: {jsonResponse}");
                    File.WriteAllText(cardDataPath,jsonResponse);

                    CardData card = JsonUtility.FromJson<CardData>(jsonResponse);
                    if (card != null)
                    {
                        Debug.Log($"Name: {card.name}");
                        Debug.Log($"Card Type:{card.type_line}");
                        Debug.Log($"Mana Cost: {card.mana_cost}");
                        Debug.Log($"Image URI: {card.image_uris?.normal}");

                        yield return DownloadAndSaveImage(card.image_uris.normal,cardImagePath);
                    }
                }
            }
        }

        this.gameObject.GetComponent<DeckSelector>().ShowDecks();
    }

    private IEnumerator DownloadAndSaveImage(string imageUrl, string savePath)
    {
        using (UnityWebRequest webRequest = UnityWebRequestTexture.GetTexture(imageUrl))
        {
            yield return webRequest.SendWebRequest();

            if (webRequest.result == UnityWebRequest.Result.ConnectionError ||
                webRequest.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.LogError($"Error downloading image: {webRequest.error}");
                yield break;
            }
            
            // Save the downloaded texture as a PNG
            Texture2D texture = DownloadHandlerTexture.GetContent(webRequest);
            byte[] imageBytes = texture.EncodeToPNG();
            File.WriteAllBytes(savePath, imageBytes);
            Debug.Log($"Saved card image to '{savePath}'.");
        }
    }
    
    private string SanitizeFileName(string fileName)
    {
        //get rid of invalid characters to path isn't invalid
        foreach (char c in Path.GetInvalidFileNameChars())
        {
            fileName = fileName.Replace(c, '_');
        }
        return fileName;
    }
    



    
}
[System.Serializable]
public class ImageUris
{
    public string small;
    public string normal;
    public string large;
    public string art_crop;
    public string border_crop;
}

[System.Serializable]
public class CardData
{
    public string name;
    public string type_line;
    public string oracle_text;
    public string mana_cost;
    public ImageUris image_uris;
}

public struct DeckData
{
    public string deckName;
    public List<string> cardsInDeck;
    public List<string> cardsStartOut;
}