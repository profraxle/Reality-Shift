using System;
using System.Collections.Generic;
using UnityEngine;

public class Surface : MonoBehaviour
{
    
    //list of cards on the surface
    List<Card> cardsOnSurface = new List<Card>();
    //list of the relative positions of cards to surface
    List<Vector3> relativePositions = new List<Vector3>();
    //list of the relative rotations of cards to surface
    List<Quaternion> relativeRotations = new List<Quaternion>();

    //variables used for aligning the table to real surface
    private bool aligning;
    private bool firstPointConfirmed,secondPointConfirmed;
    Vector3 firstPoint,secondPoint;
    
    //objects attached to fingertips
    [SerializeField]
    GameObject leftTipObject;
    [SerializeField]
    GameObject rightTipObject;
    
    //offset for each card to counteract Z-Fighting
    float surfOffsetAmount = 0.0001f;
    
    //list of all cardpiles, their relative positions and rotations
    List<GameObject> cardPiles = new List<GameObject>();
    List<Vector3> relativePilePositions = new List<Vector3>();
    List<Quaternion> relativePileRotations = new List<Quaternion>();

    public void Start()
    {
        //get the left and right fingertip objects
        leftTipObject = GameObject.FindGameObjectWithTag("LeftTip");
        rightTipObject = GameObject.FindGameObjectWithTag("RightTip");
    }

    public void Update()
    {
        //if aligning
        if (aligning)
        {
            //wait until both points are confirmed
            if (firstPointConfirmed && secondPointConfirmed)
            {
                //call the align function and reset all bools, update the cards position in the hand
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
                //check for validation
                if (cardsOnSurface[i])
                {
                    //if the card isnt grabbed
                    if (!cardsOnSurface[i].grabbed)
                    {
                        //if the card's position isnt on the table, align it to the table surface, apply a y value offset to avoid z fighting
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


    //add a card to the list of cards in surface
    public void AddCardToSurface(Card card)
    {
        cardsOnSurface.Add(card);
        relativePositions.Add(card.transform.position-transform.position);
        relativeRotations.Add(Quaternion.Euler(card.transform.rotation.eulerAngles - transform.rotation.eulerAngles) );
    }

    //remove the card from the surface if it exists within the list
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
    
    //specify two points and align the surface to the line between them
    public void AlignToLine(Vector3 point1,Vector3 point2)
    {
        //get the gradient between the two points
        float gradient =  (point2.x - point1.x) / (point2.z - point1.z);
    
        //calculate the gradient
        gradient = Mathf.Abs(gradient);
        
        //convert gradient from radians to degrees
        float angle = Mathf.Atan(gradient)*Mathf.Rad2Deg;
        
        //get the vector between both points
        Vector3 newVec = point2-point1;

        //get the inverse of this angle for offsetting
        Vector3 inverse = new Vector3(newVec.z,0,-newVec.x);

        //set the position to the midpoint of the two ligns multiplied by the inverse (perpendicular)
        transform.position = point1+ (point2 - point1)/2f + (inverse.normalized*-0.25f);
        
        //offset the rotation by the angle + the rotation of the player doing alignment
        transform.rotation = Quaternion.Euler(0, VRRigReferences.Singleton.root.eulerAngles.y + angle-90, 0);
        
        //find all other surfaces and offset them too for continuity
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

    //get all cards and untap them
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

    //begin alignment on button pressed
    public void StartAligning()
    {
        aligning = true;
        firstPointConfirmed = false;
        secondPointConfirmed = false;
    }

    
    //this function gets called when right thumbs up pose is detected when aligning, sets first point to left fingertip
    public void AddFirstPoint()
    {
        if (!firstPointConfirmed && !secondPointConfirmed)
        {
            firstPointConfirmed = true;
            firstPoint = leftTipObject.transform.position;
        }
    }

    //this function gets called when left thumbs up pose is detected when aligning, sets second point to right fingertip
    public void AddSecondPoint()
    {
        if (firstPointConfirmed && !secondPointConfirmed)
        {
            secondPointConfirmed = true;
            secondPoint = rightTipObject.transform.position;
        }
    }

    //add the a cardpile to this list of piles on surface
    public void AddToPiles(GameObject newPile)
    {
        cardPiles.Add(newPile);
        relativePilePositions.Add(newPile.transform.position - transform.position);
        relativePileRotations.Add(Quaternion.Euler(newPile.transform.rotation.eulerAngles - transform.rotation.eulerAngles));
    }

}
