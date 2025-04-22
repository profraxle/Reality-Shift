using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Serialization;

public class CardHand : MonoBehaviour
{
    //list of cards within the habd
    public List<GameObject> cardsInHand;
    
    //width of every card
    public float cardWidth = 0.15f;
    
    //the card thats currently moving
    [FormerlySerializedAs("addingCard")] public GameObject movingCard;
    private Card movingCardObj;
    
    //the last index moving card swapped with
    private int lastSwap = -1;
    
    //the menu of gameplay shortcuts
    [SerializeField]
    SurfaceMenu surfaceMenu;

    //bind function to when the user recenters
    void Start()
    {
        OVRManager.display.RecenteredPose += RecenterEventDispatcher;
    }

    //update the position of cards
    public void FinalizeMove(GameObject card)
    {
        UpdateCardsPosition();
    }

    //get a reference to card being moved
    public void BeginMove(GameObject card)
    {
        movingCard = card;
        movingCardObj = card.GetComponent<Card>();
    }

    //add a card into the card hand
    public void AddToCardsInHand(GameObject card)
    {
        if(!cardsInHand.Contains(card)){ 
            cardsInHand.Add(card);
        }
        movingCard = card;
        movingCardObj = card.GetComponent<Card>();
        UpdateCardsPosition();
    }
    
    //remove the card from the card hand
    public void RemoveFromCardsInHand(GameObject card)
    {
        cardsInHand.Remove(card);
        UpdateCardsPosition();

        if (card == movingCard)
        {
            movingCard = null;
        }
    }

    //if a card is being moved, once its no longer being grabbed update the card position, otherwise update the index based on position
    void Update()
    {
        if (movingCard)
        {
            if (!movingCardObj.grabbed)
            {
                movingCard = null;
                UpdateCardsPosition();
            }
            else
            {
                FindIndexAddingCard();
            }
        }
    }

    //compare the moving card's position to the other ones in the hand, and find what index it should be at that position
    void FindIndexAddingCard()
    {
        Vector3 startPos = gameObject.transform.position -gameObject.transform.forward * ((-cardsInHand.Count+1 * cardWidth * 0.5f));
        
        float addDist = Vector3.Distance(startPos, movingCard.transform.position);
     
        for (int i = 0; i < cardsInHand.Count; i++)
        {
            if (cardsInHand[i] != movingCard)
            {
                float currDist = Vector3.Distance(startPos, gameObject.transform.position -
                                                            gameObject.transform.forward *
                                                            ((-cardsInHand.Count * cardWidth * 0.5f) +
                                                             ((i) * cardWidth) + (0.5f * cardWidth)));

                if (currDist > addDist)
                {
                    //change the index of the moving card to the found index, if its not the last swapped card
                    if (lastSwap != i - 1)
                    {
                        Debug.Log(i);
                        lastSwap = i;
                        cardsInHand.Remove(movingCard);
                        cardsInHand.Insert(i, movingCard);
                        UpdateCardsPosition();
                    }

                    return;
                }
            }

        }
    }

    //set the position of the card objects along the card hand based on their index in the list
    public void UpdateCardsPosition()
    {
        //set the collider scale to be bigger for each card in the hand
        if (cardsInHand.Count == 0)
        {
            gameObject.transform.localScale = new Vector3(transform.localScale.x, transform.localScale.y, cardWidth);
        }
        else
        {
            gameObject.transform.localScale = new Vector3(transform.localScale.x, transform.localScale.y, cardWidth * cardsInHand.Count);
        }
       
        
        for (int i = 0; i < cardsInHand.Count; i++)
        {
            if (cardsInHand[i] != movingCard)
            {
                cardsInHand[i].transform.position = gameObject.transform.position -
                                                    gameObject.transform.forward *
                                                    ((-cardsInHand.Count * cardWidth * 0.5f) + ((i) * cardWidth) +
                                                     (0.5f * cardWidth));
                
                cardsInHand[i].transform.eulerAngles = gameObject.transform.eulerAngles + new Vector3(-20,gameObject.transform.eulerAngles.y-VRRigReferences.Singleton.transform.eulerAngles.y,-270);
            }
        }
        
    }

    //update the position of the card hand, and cards on recenter
    void RecenterEventDispatcher()
    {
        StartCoroutine(DelayFixPos());
    }

    IEnumerator DelayFixPos()
    {
        yield return new WaitForSeconds(0.2f);
        gameObject.transform.position = VRRigReferences.Singleton.root.transform.position + (VRRigReferences.Singleton.root.forward * 0.2f) +
                                        (-VRRigReferences.Singleton.root.up * 0.3f);
        UpdateCardsPosition();
    }


}
