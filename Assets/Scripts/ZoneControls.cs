using UnityEngine;

public class ZoneControls : MonoBehaviour
{

    //reference to owning card pile
    [SerializeField]
    private CardPile pile;
    
    //command to bind to the buttons to search cards in the piles
    public void SearchCards()
    {
        pile.SearchCards();
    }


}
