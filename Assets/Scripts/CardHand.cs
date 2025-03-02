using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class CardHand : MonoBehaviour
{
    public List<GameObject> cardsInHand;
    public float cardWidth = 0.15f;

    public void AddToCardsInHand(GameObject card)
    {
        cardsInHand.Add(card);
        
        card.transform.eulerAngles = gameObject.transform.eulerAngles + new Vector3(-20,-90,-270);
        UpdateCardsPosition();
    }
    
    public void RemoveFromCardsInHand(GameObject card)
    {
        cardsInHand.Remove(card);
        UpdateCardsPosition();
    }

    void UpdateCardsPosition()
    {
        //set the collider scale to be bigger for each card in the hand
        gameObject.transform.localScale = new Vector3(transform.localScale.x, transform.localScale.y, cardWidth * cardsInHand.Count);
        
        for (int i = 0; i < cardsInHand.Count; i++)
        {
            cardsInHand[i].transform.position = gameObject.transform.position -
                                                gameObject.transform.forward *
                                                ((-cardsInHand.Count * cardWidth*0.5f) + ((i) * cardWidth) + (0.5f * cardWidth));
        }
        
    }

    
}
