using System.Collections;
using Oculus.Platform;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;



public class GradSelector : MonoBehaviour
{
    
    void Start()
    {
        DeckData selectedDeck = new DeckData();
        
        Sprite[] sprites = Resources.LoadAll<Sprite>("Balatro/Balala");
        
        Dictionary<string,Texture2D> cardTextures = new Dictionary<string, Texture2D>();
        Dictionary<string, CardData> cardDatas= new Dictionary<string, CardData>();
        List<string> cards = new List<string>();
        
        for (int i = 0; i < sprites.Length; i++)
        {
            Sprite sprite = sprites[i];
            Texture2D sourceTex = sprite.texture;

            Rect rect = sprite.textureRect;
            Texture2D tex = new Texture2D((int)rect.width, (int)rect.height, TextureFormat.RGBA32, false);
            tex.SetPixels(sourceTex.GetPixels(
                (int)rect.x,
                (int)rect.y,
                (int)rect.width,
                (int)rect.height
            ));
            tex.Apply();

            cardTextures[i.ToString()] = tex;
            cardDatas[i.ToString()] = new CardData();
            cardDatas[i.ToString()].name = i.ToString();
            
            cards.Add(i.ToString());
        }
        
        selectedDeck.cardImages = cardTextures;
        selectedDeck.cardData = cardDatas;
        selectedDeck.cardsInDeck = cards;
        
        LocalPlayerManager.Singleton.playerCount = 1;
        
        LocalPlayerManager.Singleton.SetLocalPlayerDeck(selectedDeck);
        StartCoroutine(LoadSceneAsync());
    }    


    //load scene asynchronously to stop stuttering
    IEnumerator LoadSceneAsync()
    {
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync("GRADSHOWGAMEPLAY");

        while (!asyncLoad.isDone)
        {
            yield return null;
        }
    }
}


