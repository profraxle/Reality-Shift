using TMPro;
using UnityEngine;

public class DeckControls : MonoBehaviour
{
    private int cardsToDraw;

    [SerializeField]
    private TextMeshProUGUI drawText;
    
    [SerializeField]
    private TextMeshProUGUI toggleText;

    [SerializeField]
    private Deck deck;

    private bool targBottom;
    
    void Start()
    {
        cardsToDraw = 7;
        UpdateDrawText();
        
        targBottom = true;
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

    public void SearchCards()
    {
        deck.SearchCards();
    }

    public void ShuffleDeck()
    {
        deck.ShuffleButtonPressed();
    }

    public void ToggleAddToTop()
    {
        deck.ToggleAddingTop();
        targBottom = !targBottom;
        if (targBottom)
        {
            toggleText.text = "Add Card to Bottom";
        }
        else
        {
           toggleText.text = "Add Card to Top"; 
        }
    }




}
