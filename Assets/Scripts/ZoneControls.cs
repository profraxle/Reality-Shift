using UnityEngine;

public class ZoneControls : MonoBehaviour
{
    private int cardsToDraw;

    [SerializeField]
    private CardPile pile;
    

    public void SearchCards()
    {
        pile.SearchCards();
    }


}
