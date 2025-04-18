using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine.Networking;
using NUnit.Framework.Constraints;
using UnityEditor;
using UnityEngine.XR;

public class CardDataFetcher : MonoBehaviour
{

    
    //save URL for scryfall api
    private const string baseURL = "https://api.scryfall.com/cards/named?fuzzy=";
    
    //declare path for saving content
    private string saveFolder;
    
    //declare path for storing deckLists
    private string decklistFolder;
    
    //list of loaded card names
    private List<string> cardNames = new List<string>{};
    
    //list to store every deck
    private List<DeckData> decks = new List<DeckData>{};

    //dictionarys to store all of the combined card data and iomages
    Dictionary<string, CardData> allCardData = new Dictionary<string, CardData> { };
    Dictionary<string, Texture2D> allCardImages = new Dictionary<string, Texture2D> { };

    //flag if currently loading
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
        
        
        //read decklists from files
        ReadDecklists();
        
        //save the card data from remote database
        StartCoroutine(DownloadAndSaveCards(cardNames));
    }

    //return the save folder path
    public string GetSaveFolder()
    {
        return saveFolder;
    }

    //return list of decks
    public List<DeckData> GetDecks()
    {
        return decks;
    }

    
    private void ReadDecklists()
    {
        //flag for detecting the commander creature
        bool isCommander = false;

        //get all files from the directory
        var fileInfo = Directory.GetFiles(decklistFolder);
        
        //get the directory of decklists
        TextAsset[] textAssets = Resources.LoadAll<TextAsset>("DeckLists/");
        
        foreach (TextAsset textFile in textAssets)
        {
            //create a deck struct for the current decklist being read
            DeckData newDeck = new DeckData { };

            //create lists for the deck's different starting zones (on table and in deck)
            List<string> cardsInDeck = new List<string> { };
            List<string> cardsStartOut = new List<string> { };


            //read all of the lines in the current decklist
            string[] lines = textFile.text.Split(new[]{'\n','\r'});
            
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
                            
                        }
                        cardsInDeck.Add(cardName);
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
            
            //add the first card to be the face card if none exist
            if (cardsStartOut.Count <= 0)
            {
                cardsStartOut.Add(cardsInDeck[0]);
            }
            
            //load the new created deck value to the deck array
            newDeck.cardsInDeck = cardsInDeck;
            newDeck.cardsStartOut = cardsStartOut;
            decks.Add(newDeck);
        }
    }
    
    private IEnumerator DownloadAndSaveCards(List<string> cardNames)
    {
        foreach (string cardName in cardNames)
        {
            //clean the card name so it will be valid
            string sanitizedCardName = SanitizeFileName(cardName);
            
            //set path for data and card image storage
            string cardDataPath = Path.Combine(saveFolder, sanitizedCardName + ".json");
            string cardImagePath = Path.Combine(saveFolder, sanitizedCardName + ".png");
            
            if (File.Exists(cardDataPath)&& File.Exists(cardImagePath))
            {

                //if the card data already exists, load the texture from the downloaded image, and add to card data and images dictionary
                byte[] fileData;
                if (File.Exists(cardImagePath))
                {
                    fileData = File.ReadAllBytes(cardImagePath);
                    Texture2D tex = new Texture2D(2, 2);
                    tex.LoadImage(fileData);

                    tex = ConfigureTexture(tex);

                    string cardContents = File.ReadAllText(cardDataPath);
                    CardData readCard = JsonUtility.FromJson<CardData>(cardContents);

                    if (!(allCardImages.ContainsKey(cardName)))
                    {
                        allCardImages.Add(cardName, tex);
                        allCardData.Add(cardName,readCard);
                    }
                    
                }



                Debug.Log($"Card '{cardName}'already downloaded.");
                continue;
            }
            
            //add the card name to the api request url
            string url = baseURL + UnityWebRequest.EscapeURL(cardName);
            
            //make request to the scryfall api
            using (UnityWebRequest webRequest = UnityWebRequest.Get(url))
            {
                yield return webRequest.SendWebRequest();

                //handle error
                if (webRequest.result == UnityWebRequest.Result.ConnectionError ||
                    webRequest.result == UnityWebRequest.Result.ProtocolError)
                {
                    Debug.Log($"Error:{webRequest.error}");
                
                }
                else
                {
                    //read the response as a json, and save the json as card data
                    string jsonResponse = webRequest.downloadHandler.text;
                    Debug.Log($"Response: {jsonResponse}");
                    File.WriteAllText(cardDataPath,jsonResponse);

                    //serialise the card data from json into CardData type
                    CardData card = JsonUtility.FromJson<CardData>(jsonResponse);
                    
                    //if the card exists, display the information in the debug log, and add to dictionary under the card name
                    if (card != null)
                    {
                        if (!allCardData.ContainsKey(cardName))
                        {
                            allCardData.Add(cardName, card);
                        }
                        Debug.Log($"Name: {card.name}");
                        Debug.Log($"Card Type:{card.type_line}");
                        Debug.Log($"Mana Cost: {card.mana_cost}");
                        Debug.Log($"Image URI: {card.image_uris?.normal}");
                        
                        //wait for download and save image function to execute
                        yield return DownloadAndSaveImage(card.image_uris.normal,cardImagePath, cardName);
                    }
                }
            }
        }

        //for every deck, load the correct information with the newdeckversion object
        for (int i = 0; i< decks.Count; i++)
        {
            DeckData newDeckVersion = new DeckData { };
            Dictionary<string, CardData> cardData = new Dictionary<string, CardData> { };
            Dictionary<string, Texture2D> cardImages = new Dictionary<string, Texture2D> { };


            //load both arrays for cards already in deck and cards starting on the table
            foreach (string cardName in decks[i].cardsInDeck)
            {
                if (!(cardData.ContainsKey(cardName))){
                    cardData.Add(cardName, allCardData[cardName]);
                    cardImages.Add(cardName, allCardImages[cardName]);
                }
            }

            foreach (string cardName in decks[i].cardsStartOut)
            {
                if (!(cardData.ContainsKey(cardName)))
                {
                    cardData.Add(cardName, allCardData[cardName]);
                    cardImages.Add(cardName, allCardImages[cardName]);
                }
            }

            //set the information of the new deck version
            newDeckVersion.cardImages = cardImages;
            newDeckVersion.cardData = cardData;
            newDeckVersion.cardsInDeck = decks[i].cardsInDeck;
            newDeckVersion.cardsStartOut = decks[i].cardsStartOut;
            newDeckVersion.deckName = decks[i].deckName;

            //overwrite the deck at the index, and add to the local player manager
            decks[i] = newDeckVersion;
            LocalPlayerManager.Singleton.allDeckData.Add(newDeckVersion.deckName,newDeckVersion);

        }

        //call function to display the decks to be chosen by the player
        this.gameObject.GetComponent<DeckSelector>().ShowDecks();
    }

    private IEnumerator DownloadAndSaveImage(string imageUrl, string savePath, string cardName)
    {
        //get the image from the image url request
        using (UnityWebRequest webRequest = UnityWebRequestTexture.GetTexture(imageUrl))
        {
            yield return webRequest.SendWebRequest();
            
            //handle errors
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
            
            texture = ConfigureTexture(texture);

            allCardImages.Add(cardName, texture);
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
    
    
    private Texture2D ConfigureTexture(Texture2D texture)
    {
        //set the texture settings to make texture clear
        texture.filterMode = FilterMode.Trilinear;
        texture.anisoLevel = 16;
        texture.wrapMode = TextureWrapMode.Repeat;
        
        
        
/*      this condition doesnt seem to work as intended, removed
        if (SystemInfo.SupportsTextureFormat(TextureFormat.ASTC_4x4))
        {
            Texture2D newTexture = new Texture2D(texture.width, texture.height, TextureFormat.ASTC_4x4, false);
            newTexture.SetPixels(texture.GetPixels());
            newTexture.Apply();
            texture = newTexture;
        }
*/
        return texture;
    }
    

}



//create serialisable data types to load information easily into
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

//structure of deck information
public class DeckData
{
    public string deckName;
    public List<string> cardsInDeck;
    public List<string> cardsStartOut;
    public Dictionary<string, Texture2D> cardImages;
    public Dictionary<string, CardData> cardData;
}

