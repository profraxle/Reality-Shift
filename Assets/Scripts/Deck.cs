using Oculus.Interaction.HandGrab;
using Oculus.Interaction.Input;
using System.Collections.Generic;
using UnityEngine;

public class Deck : MonoBehaviour
{
    private List<Card> decklist;

    int cardsInDeck = 100;

    [SerializeField]
    private GameObject model;


    [SerializeField]
    private GameObject cardPrefab;

    [SerializeField]
    private GameObject drawZone;

    UpdateDrawZone drawZoneUpdater;

    GameObject currentCard;

    [SerializeField]
    IHand hand;

    [SerializeField]
    IHandGrabInteractor interactor;

    private bool newCardNeeded = false;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        drawZoneUpdater = drawZone.GetComponent<UpdateDrawZone>();

        cardsInDeck--;
        model.transform.localScale = new Vector3(100, 100, 100 * cardsInDeck);
        //creates a first card to be grabbable on top of the deck
        currentCard = Instantiate(cardPrefab);
        currentCard.transform.position = transform.position;
        currentCard.GetComponent<Card>().SetLocked(true);
        

        
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 diff = currentCard.transform.position - transform.position;
        if (diff.magnitude > 0.01)
        {
            currentCard.GetComponent<Card>().SetLocked(false);
            newCardNeeded = true;
        }
    }


    /// <summary>
    /// Method is called from Poses Manager when DrawCard Pose is detected. Checks if the hand is overlapping the draw zone and creates a new card on top when a new card is taken
    /// </summary>
    public void DrawFromDeck()
    {
        //checks if the hand is w
        if (drawZoneUpdater.GetIsInDrawZone())
        {

            //checks if currentCard no longer in the spawn position
            if (newCardNeeded)
            {

                //spawn a new card to be grabbed and change scale of deck to reflect cards left to be drawn
                currentCard = Instantiate(cardPrefab);
                currentCard.transform.position = transform.position;
                cardsInDeck--;

                model.transform.localScale = new Vector3(100, 100, 100 * cardsInDeck);
                newCardNeeded = false;
            }
            
        }

    }


}
