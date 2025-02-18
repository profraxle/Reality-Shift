using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;


//base class for any pile of cards, such as the Deck or Zones (Graveyard, Exile, Face down Exile)
public class CardPile : NetworkBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    
    public GameObject cardPrefab;
    
    //reference to card object being added at top of pile
    public Card cardToAdd;
    
    protected Stack<string> cardsInPile;
    
    [SerializeField]
    protected UpdateDrawZone drawZoneUpdater;
    [SerializeField]
    protected GameObject model;

    protected GameObject drawableCard;
    
    public DeckData deckData;
    
    protected List<Material> modelMaterials;
    
    protected float surfHeight;
    public float surfOffset;
    public float cardHeight;

    protected ulong playerID;

    protected ClientRpcParams clientRpcParams;

    public bool faceUp;
    private float cardRot;
    
    protected virtual void Start()
    {
        model.GetComponent<NetworkObject>().Spawn();
        if (faceUp)
        {
            cardRot = -90f;
        }
        else
        {
            cardRot = 90f;
        }
        
        cardPrefab = Resources.Load<GameObject>("Card");
        drawableCard = null;
        cardsInPile = new Stack<string>();
        
        model.transform.localScale = new Vector3(100, 100, 0);
        
        modelMaterials = new List<Material>();
        model.GetComponent<Renderer>().GetMaterials(modelMaterials);
        
        surfHeight = GameObject.FindGameObjectWithTag("Surface").transform.position.y;
        cardHeight = 3e-05f;
        surfOffset = cardHeight / 2f;
        
        transform.position = new Vector3(transform.position.x, surfHeight+surfOffset, transform.position.z);
        
        clientRpcParams = new ClientRpcParams
        {
            Send = new ClientRpcSendParams
            {
                TargetClientIds = new ulong[] { playerID }
            }
        };
    }

    // Update is called once per frame
    protected virtual void Update()
    {
        if (cardToAdd)
        {
            if (cardToAdd.IsNotGrabbed())
            {
                if (!drawableCard)
                {
                    
                    SpawnDrawableCard(cardToAdd);
                }
                else
                {
                    UpdateDrawableCard(cardToAdd);
                }
                
                //add the card name to the cards in Zone
                cardsInPile.Push(cardToAdd.cardData.name);

                //set size of the pile to correct location and move to be on tabletop
                UpdatePileHeight();

                if (faceUp)
                {
                    //get texture of adding card and set pile top texture to it
                    Texture2D tex = deckData.cardImages[cardToAdd.cardData.name];
                    modelMaterials[2].mainTexture = tex;
                }

                //destroy the owning card object
                Destroy(cardToAdd.gameObject);
                cardToAdd = null;
            }
        }

        //if a card exists to be drawn
        if (drawableCard)
        {
            
            //get difference of cards pos from spawned pos
            Vector3 diff = drawableCard.transform.position - new Vector3(transform.position.x, 
                surfHeight + (10f * cardsInPile.Count * cardHeight) - surfOffset, transform.position.z);
            
            //when distance is greater than 0.01
            if (diff.magnitude > 0.01)
            {
                
                //unlock card, and pop top of stack
                drawableCard.GetComponent<Card>().SetLocked(false);
                if (cardsInPile.Count > 0)
                {
                    cardsInPile.Pop();
                }

                //change size of the card model 
                UpdatePileHeight();

                //if there are still cards in the pile
                if (cardsInPile.Count > 0)
                {
                    SpawnNextCardInPile();

                    if (faceUp)
                    {
                        modelMaterials[2].mainTexture = deckData.cardImages[cardsInPile.Peek()];
                    }
                }
                else
                {
                    if (faceUp)
                    {
                        modelMaterials[2].mainTexture = null;
                        drawableCard = null;
                    }
                }
            }
        }

    }
    
        private void OnTriggerEnter(Collider other)
    {
        //if there's a card in the zone
        if (other.CompareTag("Card"))
        {
            if (drawableCard)
            {
                if (other.gameObject != drawableCard)
                {
                    Card otherCard = other.GetComponent<Card>();
                    if (!otherCard.locked)
                    {
                        cardToAdd = other.GetComponent<Card>();
                        cardToAdd.SetLocked(true);
                    }
                }
            }
            else
            {
                Card otherCard = other.GetComponent<Card>();
                if (!otherCard.locked)
                {
                    cardToAdd = other.GetComponent<Card>();
                    cardToAdd.SetLocked(true);
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
                    if (cardToAdd)
                    {
                        cardToAdd.SetLocked(false);
                    }

                    cardToAdd = null;
                }
            }
            else
            {
                if (cardToAdd)
                {
                    cardToAdd.SetLocked(false);
                }
                cardToAdd = null;
            }
        }
    }

    //call SpawnDrawableCard but get the string from card object passed in
    public void SpawnDrawableCard(Card cardInZone)
    {
        string cardName = cardInZone.cardData.name;
        SpawnDrawableCard(cardName);   
    }
    
    //instatiate new drawable card and set tex and data from deckData
    public void SpawnDrawableCard(String cardName)
    {
        drawableCard = Instantiate(cardPrefab);
        
        drawableCard.transform.position = new Vector3(transform.position.x, surfHeight+(10f*cardsInPile.Count*cardHeight)-surfOffset, transform.position.z);
        drawableCard.transform.eulerAngles = new Vector3(cardRot, 0,0) + transform.eulerAngles;
        Card drawableCardObj = drawableCard.GetComponent<Card>();
        drawableCardObj.SetLocked(true);
        drawableCardObj.lockPos = model.transform.position;
        drawableCardObj.pokeInteractable.Disable();

        Texture2D tex = deckData.cardImages[cardName];
        CardData cardData = deckData.cardData[cardName];


        //set card face to the current texture
       
        List<Material> materials = new List<Material>();
        drawableCard.GetComponent<Renderer>().GetMaterials(materials);
        materials[2].mainTexture = tex;
        

        drawableCardObj.cardData = cardData;

        var cardNetworkObject = drawableCard.GetComponent<NetworkObject>();
        cardNetworkObject.SpawnWithOwnership(playerID);


        NetworkObjectReference cardNetworkReference = new NetworkObjectReference(drawableCard);
        ChangeCardTexClientRpc(cardNetworkReference,cardName);
    }

    //peek top and spawn card with that name
    public void SpawnNextCardInPile()
    {
        string newCardName = cardsInPile.Peek();
        SpawnDrawableCard(newCardName);
    }

    //same as above but over ~the network~
    [ServerRpc]
    public void SpawnNextCardInPileServerRpc()
    {
        SpawnNextCardInPile();
        NetworkObjectReference cardNetworkReference = new NetworkObjectReference(drawableCard);
        
        NewCardClientRpc(cardNetworkReference,clientRpcParams);
    }

    [ClientRpc]
    public void NewCardClientRpc(NetworkObjectReference cardObjectReference, ClientRpcParams clientRpcParams)
    {
        cardObjectReference.TryGet(out NetworkObject networkObject);
        
        drawableCard = networkObject.gameObject;
    }

    public void UpdateDrawableCard(Card cardInZone)
    {
        Card drawableCardObj = drawableCard.GetComponent<Card>();
        
        string cardName = cardInZone.cardData.name;

        Texture2D tex = deckData.cardImages[cardName];
        CardData cardData = deckData.cardData[cardName];
        
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

        Texture2D tex = deckData.cardImages[cardName];
        CardData cardData = deckData.cardData[cardName];

        List<Material> materials = new List<Material>();
        changedCard.GetComponent<Renderer>().GetMaterials(materials);
        materials[2].mainTexture = tex;

        changedCard.GetComponent<Card>().cardData = cardData;
    }

    public void UpdatePileHeight()
    {
        model.transform.localScale = new Vector3(150, 150, 150 * cardsInPile.Count);
        transform.position = new Vector3(transform.position.x,
            surfHeight + (5f * cardsInPile.Count * cardHeight) + surfOffset, transform.position.z);
    }
    
}
