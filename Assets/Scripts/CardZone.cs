using System;
using System.Collections.Generic;
using UnityEngine;

public class CardZone : MonoBehaviour
{
    public Stack<string> cardsInZone;
    [SerializeField]
    private UpdateDrawZone drawZoneUpdater;
    public GameObject drawableCard;
    public Card cardInZone;

    public Deck owningDeck;
    
    [SerializeField]
    private GameObject model;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        cardInZone = null;
        cardsInZone = new Stack<string>();

        model.transform.localScale = new Vector3(100, 100, 0);
    }

    void Update()
    {
        if (cardInZone)
        {
            if (cardInZone.IsNotGrabbed())
            {
                //add the card name to the cards in Zone
                cardsInZone.Push(cardInZone.cardData.name);

                model.transform.localScale = new Vector3(100, 100, 100 * cardsInZone.Count);
                model.transform.localPosition = new Vector3(0, -100 * cardsInZone.Count * 3e-05f, 0);

                Texture2D tex = owningDeck.deckData.cardImages[cardInZone.cardData.name];
                
                List<Material> materials = new List<Material>();
                model.GetComponent<Renderer>().GetMaterials(materials);
                materials[2].mainTexture = tex;
                
                //destroy the owning card object
                Destroy(cardInZone.gameObject);
                cardInZone = null;
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        
        //if theres a card in the zone
        if (other.CompareTag("Card"))
        {
            cardInZone = other.GetComponent<Card>();
        }
    
    }

    private void OnTriggerExit(Collider other)
    {
        //if theres a card leaving the zone
        if (other.CompareTag("Card"))
        {
            cardInZone = null;
        }
    }
}
