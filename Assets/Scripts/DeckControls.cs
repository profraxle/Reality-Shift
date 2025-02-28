using TMPro;
using UnityEngine;

public class DeckControls : MonoBehaviour
{
    private int cardsToDraw;

    [SerializeField]
    private TextMeshProUGUI drawText;

    [SerializeField]
    private Deck deck;
    
    void Start()
    {
        cardsToDraw = 7;
        UpdateDrawText();
    }

    public void IncreaseCardsToDraw()
    {
        cardsToDraw++;
        UpdateDrawText();
    }

    public void DecreaseCardsToDraw()
    {
        if (cardsToDraw > 1)
        {
            cardsToDraw--;
            UpdateDrawText();
        }

    }

    private void UpdateDrawText()
    {
        drawText.text = "Draw " + cardsToDraw + " Cards";
    }

    public void DrawCards()
    {
        deck.QuickDrawCards(cardsToDraw);
    }
    
    
    

}
