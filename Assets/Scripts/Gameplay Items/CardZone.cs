using System;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;

public class CardZone : CardPile
{
    public Deck owningDeck;


    private void Awake()
    {
        //set to being faceup
        faceUp = true;
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    protected override void Start()
    {
        base.Start();
        faceUp = true;
    }

    public void PassDeckData(Deck owningDeck)
    {
        //pass in the deck data from the owning deck
        deckData = owningDeck.deckData;
    }

}
