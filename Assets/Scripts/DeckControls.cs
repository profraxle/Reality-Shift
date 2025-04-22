using TMPro;
using UnityEngine;

public class DeckControls : MonoBehaviour
{
    
    //the amount of cards to draw at a time
    private int cardsToDraw;

    //the text of the cards being drawn
    [SerializeField]
    private TextMeshProUGUI drawText;
    
    //the text for the toggle button
    [SerializeField]
    private TextMeshProUGUI toggleText;

    //the deck reference
    [SerializeField]
    private Deck deck;

    //if this is targeting adding to bottom
    private bool targBottom;
    
    void Start()
    {
        //initialise the cards to draw
        cardsToDraw = 7;
        
        //update the text on the draw button
        UpdateDrawText();
        
        targBottom = true;
    }

    //incremend and decrement the amount of cards to be drawn
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

    //update the text for the drawing multiple cards button
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

    //toggle adding to top and change the text displayed to the player
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
