using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class CardZone : MonoBehaviour
{
    public Stack<string> cardsInZone;
    [SerializeField]
    private UpdateDrawZone drawZoneUpdater;
    public GameObject drawableCard;
    public Card cardInZone;

    public Deck owningDeck;
    public DeckData owningDeckData;
    
    [SerializeField]
    private GameObject model;

    private List<Material> deckMaterials;

    private float surfHeight;
    public float surfOffset;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        cardInZone = null;
        cardsInZone = new Stack<string>();

        model.transform.localScale = new Vector3(100, 100, 0);
        //model.SetActive(false);

        owningDeckData = owningDeck.deckData;
        
        deckMaterials = new List<Material>();
        model.GetComponent<Renderer>().GetMaterials(deckMaterials);

        surfHeight = GameObject.FindGameObjectWithTag("Surface").transform.position.y;
        surfOffset = 0.001f;
        
        transform.position = new Vector3(transform.position.x, surfHeight+surfOffset, transform.position.z);
    }

    void Update()
    {
        if (cardInZone)
        {
            if (cardInZone.IsNotGrabbed())
            {
                if (!drawableCard)
                {
                    SpawnDrawableCard(cardInZone);
                }
                else
                {
                    UpdateDrawableCard(cardInZone);
                }

                
                //add the card name to the cards in Zone
                cardsInZone.Push(cardInZone.cardData.name);

                //set size of the pile to correct location and move to be on tabletop
                model.transform.localScale = new Vector3(100, 100, 100 * cardsInZone.Count);
                transform.position = new Vector3(transform.position.x, surfHeight+(5f*cardsInZone.Count*3e-05f)+surfOffset , transform.position.z);
                
                //get texture of adding card and set pile top texture to it
                Texture2D tex = owningDeckData.cardImages[cardInZone.cardData.name];
                deckMaterials[2].mainTexture = tex;
                
                //destroy the owning card object
                Destroy(cardInZone.gameObject);
                cardInZone = null;
            }
        }
        
        if (drawableCard)
        {
            Vector3 diff = drawableCard.transform.position - new Vector3(transform.position.x, surfHeight+(10f*cardsInZone.Count*3e-05f)-surfOffset, transform.position.z);
            if (diff.magnitude > 0.01)
            {
                drawableCard.GetComponent<Card>().SetLocked(false);

                if (cardsInZone.Count > 0)
                {
                    cardsInZone.Pop();
                }

                model.transform.localScale = new Vector3(100, 100, 100 * cardsInZone.Count);
                transform.position = new Vector3(transform.position.x, surfHeight+(5f*cardsInZone.Count*3e-05f)+surfOffset, transform.position.z);

                
                
                if (cardsInZone.Count > 0)
                {
                    string newCardName = cardsInZone.Peek();
                    
                    SpawnDrawableCard(newCardName);
                    
                    Texture2D tex = owningDeckData.cardImages[newCardName];
                    deckMaterials[2].mainTexture = tex;
                }else
                {
                    deckMaterials[2].mainTexture = null;
                    drawableCard = null;
                }
                
                
                
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        //if theres a card in the zone
        if (other.CompareTag("Card"))
        {
            if (drawableCard)
            {
                if (other.gameObject != drawableCard)
                {
                    Card otherCard = other.GetComponent<Card>();
                    if (!otherCard.locked)
                    {
                        cardInZone = other.GetComponent<Card>();
                        cardInZone.SetLocked(true);
                    }
                }
            }
            else
            {
                Card otherCard = other.GetComponent<Card>();
                if (!otherCard.locked)
                {
                    cardInZone = other.GetComponent<Card>();
                    cardInZone.SetLocked(true);
                }
            }

            
        }
    
    }

    private void OnTriggerExit(Collider other)
    {
        //if theres a card leaving the zone
        if (other.CompareTag("Card"))
        {
            if (drawableCard)
            {
                if (other.gameObject != drawableCard)
                {
                    if (cardInZone)
                    {
                        cardInZone.SetLocked(false);
                    }

                    cardInZone = null;
                }
            }
            else
            {
                if (cardInZone)
                {
                    cardInZone.SetLocked(false);
                }
                cardInZone = null;
            }
        }
    }

    public void SpawnDrawableCard(Card cardInZone)
    {
        drawableCard = Instantiate(owningDeck.cardPrefab);
        
        drawableCard.transform.position = new Vector3(transform.position.x, surfHeight+(10f*cardsInZone.Count*3e-05f)-surfOffset, transform.position.z);
        drawableCard.transform.eulerAngles = new Vector3(-90, 0,0) + transform.eulerAngles;
        Card drawableCardObj = drawableCard.GetComponent<Card>();
        drawableCardObj.SetLocked(true);
        drawableCardObj.lockPos = model.transform.position;
        drawableCardObj.pokeInteractable.Disable();

        string cardName = cardInZone.cardData.name;

        Texture2D tex = owningDeckData.cardImages[cardName];
        CardData cardData = owningDeckData.cardData[cardName];


        //set card face to the current texture
        List<Material> materials = new List<Material>();
        drawableCard.GetComponent<Renderer>().GetMaterials(materials);
        materials[2].mainTexture = tex;

        drawableCardObj.cardData = cardData;

        var cardNetworkObject = drawableCard.GetComponent<NetworkObject>();
        cardNetworkObject.SpawnWithOwnership(owningDeck.deckID.Value);


        NetworkObjectReference cardNetworkReference = new NetworkObjectReference(drawableCard);
        ChangeCardTexClientRpc(cardNetworkReference,cardName);
    }
    
    public void SpawnDrawableCard(String cardName)
    {
        drawableCard = Instantiate(owningDeck.cardPrefab);
        
        drawableCard.transform.position = new Vector3(transform.position.x, surfHeight+(10f*cardsInZone.Count*3e-05f)-surfOffset, transform.position.z);
        drawableCard.transform.eulerAngles = new Vector3(-90, 0,0) + transform.eulerAngles;
        Card drawableCardObj = drawableCard.GetComponent<Card>();
        drawableCardObj.SetLocked(true);
        drawableCardObj.lockPos = model.transform.position;
        drawableCardObj.pokeInteractable.Disable();

        Texture2D tex = owningDeckData.cardImages[cardName];
        CardData cardData = owningDeckData.cardData[cardName];


        //set card face to the current texture
        List<Material> materials = new List<Material>();
        drawableCard.GetComponent<Renderer>().GetMaterials(materials);
        materials[2].mainTexture = tex;

        drawableCardObj.cardData = cardData;

        var cardNetworkObject = drawableCard.GetComponent<NetworkObject>();
        cardNetworkObject.SpawnWithOwnership(owningDeck.deckID.Value);


        NetworkObjectReference cardNetworkReference = new NetworkObjectReference(drawableCard);
        ChangeCardTexClientRpc(cardNetworkReference,cardName);
    }

    public void UpdateDrawableCard(Card cardInZone)
    {
        Card drawableCardObj = drawableCard.GetComponent<Card>();
        
        string cardName = cardInZone.cardData.name;

        Texture2D tex = owningDeckData.cardImages[cardName];
        CardData cardData = owningDeckData.cardData[cardName];
        
        drawableCardObj.cardData = cardData;
        
        List<Material> materials = new List<Material>();
        drawableCard.GetComponent<Renderer>().GetMaterials(materials);
        materials[2].mainTexture = tex;
        
        NetworkObjectReference cardNetworkReference = new NetworkObjectReference(drawableCard);
        ChangeCardTexClientRpc(cardNetworkReference,cardName);
    }
    
    [ClientRpc]
    public void ChangeCardTexClientRpc(NetworkObjectReference cardNetworkReference, string cardName)
    {
        NetworkObject networkObject;
        cardNetworkReference.TryGet(out networkObject);
        GameObject changedCard = networkObject.gameObject;

        Texture2D tex = owningDeckData.cardImages[cardName];
        CardData cardData = owningDeckData.cardData[cardName];

        List<Material> materials = new List<Material>();
        changedCard.GetComponent<Renderer>().GetMaterials(materials);
        materials[2].mainTexture = tex;

        changedCard.GetComponent<Card>().cardData = cardData;
    }
}
