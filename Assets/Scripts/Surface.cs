using System;
using System.Collections.Generic;
using UnityEngine;

public class Surface : MonoBehaviour
{
    
    List<Card> cardsOnSurface = new List<Card>();
    List<Vector3> relativePositions = new List<Vector3>();
    List<Quaternion> relativeRotations = new List<Quaternion>();

    private bool aligning;
    private bool firstPointConfirmed,secondPointConfirmed;
    Vector3 firstPoint,secondPoint;
    
    [SerializeField]
    GameObject leftTipObject;
    
    [SerializeField]
    GameObject rightTipObject;
    
    float surfOffsetAmount = 0.0001f;
    
    List<GameObject> cardPiles = new List<GameObject>();
    List<Vector3> relativePilePositions = new List<Vector3>();
    List<Quaternion> relativePileRotations = new List<Quaternion>();

    public void Start()
    {
        leftTipObject = GameObject.FindGameObjectWithTag("LeftTip");
        rightTipObject = GameObject.FindGameObjectWithTag("RightTip");
    }

    public void Update()
    {
        if (aligning)
        {
            if (firstPointConfirmed && secondPointConfirmed)
            {
                Debug.Log("Aligning finish");
                AlignToLine(firstPoint, secondPoint);
                aligning = false;
                firstPointConfirmed = false;
                secondPointConfirmed = false;
                LocalPlayerManager.Singleton.localPlayerHand.GetComponentInChildren<CardHand>().UpdateCardsPosition();
            }
        }
    }
    
    public void LateUpdate(){

    //iterate through cards on surface, align them to table if not
    if (cardsOnSurface.Count != 0)
        {
            for (int i = 0; i < cardsOnSurface.Count; i++)
            {
                if (cardsOnSurface[i])
                {
                    if (!cardsOnSurface[i].grabbed)
                    {
                        bool needsUpdate = false;

                        GameObject cardObj = cardsOnSurface[i].gameObject;

                        float yTarget = transform.position.y + (i * surfOffsetAmount);

                        Vector3 posOnTable = cardObj.transform.position;
                        Quaternion rotOnTable = cardObj.transform.rotation;
                        if (cardObj.transform.position.y != yTarget)
                        {
                            posOnTable = new Vector3(cardObj.transform.position.x, yTarget,
                                cardObj.transform.position.z);
                            needsUpdate = true;
                        }

                        if (cardObj.transform.eulerAngles.x != 0)
                        {
                            rotOnTable = Quaternion.Euler(cardsOnSurface[i].flipRot, cardObj.transform.eulerAngles.y, cardObj.transform.eulerAngles.z);
                            needsUpdate = true;
                        }

                        if (needsUpdate)
                        {
                            cardObj.transform.SetPositionAndRotation(posOnTable, rotOnTable);
                            relativePositions[i] = (posOnTable - transform.position);
                            relativeRotations[i] = Quaternion.Euler(rotOnTable.eulerAngles-transform.eulerAngles);
                        }
                    }
                }
                else
                {
                    cardsOnSurface.RemoveAt(i);
                }
            }
        }
    }


    public void AddCardToSurface(Card card)
    {
        cardsOnSurface.Add(card);
        relativePositions.Add(card.transform.position-transform.position);
        relativeRotations.Add(Quaternion.Euler(card.transform.rotation.eulerAngles - transform.rotation.eulerAngles) );
    }

    public void RemoveCardFromSurface(Card card)
    { 
        int index = cardsOnSurface.IndexOf(card);
        if (index != -1)
        {
            cardsOnSurface.Remove(card);
            relativePositions.RemoveAt(index);
            relativeRotations.RemoveAt(index);
        }
    }
    
    public void AlignToLine(Vector3 point1,Vector3 point2)
    {
        float gradient =  (point2.x - point1.x) / (point2.z - point1.z);
    
        gradient = Mathf.Abs(gradient);
        
        float angle = Mathf.Atan(gradient)*Mathf.Rad2Deg;
        
        Vector3 newVec = point2-point1;

        Vector3 inverse = new Vector3(newVec.z,0,-newVec.x);

        transform.position = point1+ (point2 - point1)/2f + (inverse.normalized*-0.25f);
        
        transform.rotation = Quaternion.Euler(0, VRRigReferences.Singleton.root.eulerAngles.y + angle-90, 0);


        for (int i = 0; i <cardsOnSurface.Count;i++)
        {
          //  Vector3 newPos = transform.position + relativePositions[i];
           // Quaternion newRot = Quaternion.Euler(transform.rotation.eulerAngles + relativeRotations[i].eulerAngles);
           // cardsOnSurface[i].transform.SetPositionAndRotation(newPos, newRot);
        }

        for (int i = 0; i < cardPiles.Count; i++)
        {
          //  Vector3 newPos = transform.position + relativePilePositions[i];
         //   Quaternion newRot = Quaternion.Euler(transform.rotation.eulerAngles + relativePileRotations[i].eulerAngles);
            
          //  cardPiles[i].transform.SetPositionAndRotation(newPos, newRot);
           // cardPiles[i].GetComponent<CardPile>().UpdateDrawablePosition();
            
           // relativePilePositions[i] = cardPiles[i].transform.position-transform.position;
         //   relativePileRotations[i] = Quaternion.Euler(cardPiles[i].transform.rotation.eulerAngles - transform.rotation.eulerAngles);
        }

        GameObject[] surfaces = GameObject.FindGameObjectsWithTag("Surface");

        foreach (GameObject surface in surfaces)
        {
            if (surface != gameObject)
            {
                surface.transform.position = transform.position+transform.right*0.525f;
        
                surface.transform.rotation = Quaternion.Euler(0, VRRigReferences.Singleton.root.eulerAngles.y + angle-90, 0);
            }
        }
        
    }

    public void UntapAllCards()
    {
        int index = 0;
        foreach (Card card in cardsOnSurface)
        {
            index++;
            Debug.Log(index);
            card.transform.SetPositionAndRotation(card.transform.position, Quaternion.Euler(new Vector3(card.transform.eulerAngles.x,0,transform.eulerAngles.y)));
        }
    }

    public void StartAligning()
    {
        aligning = true;
        firstPointConfirmed = false;
        secondPointConfirmed = false;
    }

    public void AddFirstPoint()
    {
        if (!firstPointConfirmed && !secondPointConfirmed)
        {
            firstPointConfirmed = true;
            firstPoint = leftTipObject.transform.position;
        }
    }

    public void AddSecondPoint()
    {
        if (firstPointConfirmed && !secondPointConfirmed)
        {
            secondPointConfirmed = true;
            secondPoint = rightTipObject.transform.position;
        }
    }

    public void AddToPiles(GameObject newPile)
    {
        cardPiles.Add(newPile);
        relativePilePositions.Add(newPile.transform.position - transform.position);
        relativePileRotations.Add(Quaternion.Euler(newPile.transform.rotation.eulerAngles - transform.rotation.eulerAngles));
    }

}
