using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Serialization;

public class CardHand : MonoBehaviour
{
    public List<GameObject> cardsInHand;
    public float cardWidth = 0.15f;
    [FormerlySerializedAs("addingCard")] public GameObject movingCard;
    private Card movingCardObj;
    private int lastSwap = -1;
    [SerializeField]
    SurfaceMenu surfaceMenu;

    void Start()
    {
        OVRManager.display.RecenteredPose += RecenterEventDispatcher;
    }

public void FinalizeMove(GameObject card)
    {
        //movingCard =  null;
        UpdateCardsPosition();
    }

    public void BeginMove(GameObject card)
    {
        movingCard = card;
        movingCardObj = card.GetComponent<Card>();
    }

    public void AddToCardsInHand(GameObject card)
    {
        if(!cardsInHand.Contains(card)){ 
            cardsInHand.Add(card);
        }
        movingCard = card;
        movingCardObj = card.GetComponent<Card>();
        UpdateCardsPosition();
    }
    
    
    public void RemoveFromCardsInHand(GameObject card)
    {
        cardsInHand.Remove(card);
        UpdateCardsPosition();

        if (card == movingCard)
        {
            movingCard = null;
        }
    }

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
