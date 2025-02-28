using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class CardHand : MonoBehaviour
{
    public List<GameObject> cardsInHand;

    public void AddToCardsInHand(GameObject card)
    {
        cardsInHand.Add(card);
        card.transform.eulerAngles = gameObject.transform.eulerAngles + new Vector3(0,0,0);
        UpdateCardsPosition();
    }
    
    public void RemoveFromCardsInHand(GameObject card)
    {
        cardsInHand.Remove(card);
        card.transform.eulerAngles = gameObject.transform.eulerAngles + new Vector3(0,0,0);
        UpdateCardsPosition();
    }

    void UpdateCardsPosition()
    {
        for (int i = 0; i < cardsInHand.Count; i++)
        {
            cardsInHand[i].transform.position = gameObject.transform.position -
                                                gameObject.transform.forward *
                                                ((-cardsInHand.Count * 0.04f) + (i * 0.04f));
        }
        
    }

    
}
