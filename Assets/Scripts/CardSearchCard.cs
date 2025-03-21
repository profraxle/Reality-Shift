using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class CardSearchCard : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created

    public CardData cardData;
    public Texture2D cardTexture;
    public CardPile owningPile;
    public SearchableMenu owningMenu;

    public void SetCardImage()
    {
        StartCoroutine(SetCardImageCoroutine());
    }

    IEnumerator SetCardImageCoroutine()
    {
        this.GetComponent<Image>().sprite = Sprite.Create(cardTexture, new Rect(0, 0, cardTexture.width, cardTexture.height), new Vector2(0.5f, 0.5f));
        yield return null;
    }

    public void SpawnCard()
    {
        owningMenu.RemoveFromMenu(gameObject);
        owningMenu.SetMenuItemsPositions();
        Destroy(this.gameObject);
    }
}
