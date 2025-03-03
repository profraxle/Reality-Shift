using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class CardHand : MonoBehaviour
{
    public List<GameObject> cardsInHand;
    public float cardWidth = 0.15f;
    public GameObject addingCard;
    private int lastSwap = -1;
    
    public void FinalizeAdd(GameObject card)
    {
        card.transform.eulerAngles = gameObject.transform.eulerAngles + new Vector3(-20,-90,-270);
        addingCard =  null;
        UpdateCardsPosition();
    }

    public void AddToCardsInHand(GameObject card)
    {
        if (!addingCard)
        {
            if(!cardsInHand.Contains(card)){
                cardsInHand.Add(card);
            }
            addingCard = card;
        }
    }
    
    
    public void RemoveFromCardsInHand(GameObject card)
    {
        cardsInHand.Remove(card);
        UpdateCardsPosition();
    }

    void Update()
    {
        if (addingCard)
        {
            FindIndexAddingCard();
        }
    }

    void FindIndexAddingCard()
    {
        Vector3 startPos = gameObject.transform.position -gameObject.transform.forward * ((-cardsInHand.Count+1 * cardWidth * 0.5f));
        
        float addDist = Vector3.Distance(startPos, addingCard.transform.position);
     
        for (int i = 0; i < cardsInHand.Count; i++)
        {
            if (cardsInHand[i] != addingCard)
            {
                float currDist = Vector3.Distance(startPos, gameObject.transform.position -
                                                            gameObject.transform.forward *
                                                            ((-cardsInHand.Count * cardWidth * 0.5f) +
                                                             ((i) * cardWidth) + (0.5f * cardWidth)));

                if (currDist > addDist)
                {
                    if (lastSwap != i - 1)
                    {
                        Debug.Log(i);
                        lastSwap = i;
                        cardsInHand.Remove(addingCard);
                        cardsInHand.Insert(i, addingCard);
                        UpdateCardsPosition();
                    }

                    return;
                }
            }

        }
    }

    void UpdateCardsPosition()
    {
        //set the collider scale to be bigger for each card in the hand
        gameObject.transform.localScale = new Vector3(transform.localScale.x, transform.localScale.y, cardWidth * cardsInHand.Count);
        
        for (int i = 0; i < cardsInHand.Count; i++)
        {
            if (cardsInHand[i].GetComponent<Card>().inHand)
            {
                cardsInHand[i].transform.position = gameObject.transform.position -
                                                    gameObject.transform.forward *
                                                    ((-cardsInHand.Count * cardWidth * 0.5f) + ((i) * cardWidth) +
                                                     (0.5f * cardWidth));
            }
        }
        
    }

    
}
