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

    [SerializeField] protected UpdateDrawZone drawZoneUpdater;
    [SerializeField] protected GameObject model;

    public GameObject drawableCard;

    public DeckData deckData;

    protected List<Material> modelMaterials;

    protected float surfHeight;
    public float surfOffset;
    public float cardHeight;

    public NetworkVariable<ulong> playerID;

    protected ClientRpcParams clientRpcParams;

    public bool faceUp;
    private float cardRot;

    private NetworkVariable<int> pileHeight = new NetworkVariable<int>(0);

    public bool cardSpawnedValid;

    protected virtual void Start()
    {
        if (faceUp)
        {
            cardRot = -90f;
        }
        else
        {
            cardRot = 90f;
        }

        //load card prefab from game resources folder
        cardPrefab = Resources.Load<GameObject>("Card");

        //set the drawable card to null
        drawableCard = null;

        //create stack to store cards in pile
        cardsInPile = new Stack<string>();

        modelMaterials = new List<Material>();
        model.GetComponent<Renderer>().GetMaterials(modelMaterials);

        surfHeight = GameObject.FindGameObjectWithTag("Surface").transform.position.y;
        cardHeight = 3e-05f;
        surfOffset = cardHeight / 2f;

        transform.position = new Vector3(transform.position.x, surfHeight + surfOffset, transform.position.z);

        clientRpcParams = new ClientRpcParams
        {
            Send = new ClientRpcSendParams
            {
                TargetClientIds = new ulong[] { playerID.Value }
            }
        };
        
        pileHeight.OnValueChanged += UpdatePileHeight;

        model.transform.localScale = new Vector3(100, 100, 0);

        cardSpawnedValid = false;
    }

    // Update is called once per frame
    protected virtual void Update()
    {
        if (playerID.Value == NetworkManager.Singleton.LocalClientId)
        {
            if (NetworkManager.Singleton.IsServer)
            {
                if (pileHeight.Value != cardsInPile.Count)
                {
                    UpdatePileHeight();
                }
            }

            if (cardToAdd)
            {
                if (cardToAdd.IsNotGrabbed())
                {
                    if (!drawableCard)
                    {
                        cardSpawnedValid = false;
                        if (NetworkManager.Singleton.IsServer)
                        {
                            SpawnDrawableCard(cardToAdd);
                        }
                        else
                        {

                            SpawnDrawableCardServerRpc(cardToAdd.cardData.name);
                        }
                    }
                    else
                    {
                        if (NetworkManager.Singleton.IsServer)
                        {
                            UpdateDrawableCard(cardToAdd);
                        }
                        else
                        {
                            UpdateDrawableCardServerRpc(cardToAdd.cardData.name);

                        }
                    }

                    //add the card name to the cards in Zone
                    if (NetworkManager.Singleton.IsServer)
                    {
                        cardsInPile.Push(cardToAdd.cardData.name);
                    }
                    else
                    {
                        PushCardToPileServerRpc(cardToAdd.cardData.name);
                    }


                    if (faceUp)
                    {
                        //get texture of adding card and set pile top texture to it
                        Texture2D tex = deckData.cardImages[cardToAdd.cardData.name];
                        modelMaterials[2].mainTexture = tex;
                        
                        ChangePileTextureServerRpc(cardToAdd.cardData.name);
                    }

                    //destroy the owning card object
                    if (NetworkManager.Singleton.IsServer)
                    {
                        cardToAdd.gameObject.GetComponent<NetworkObject>().Despawn();
                        Destroy(cardToAdd.gameObject);
                        cardToAdd = null;
                    }
                    else
                    {
                        NetworkObjectReference cardNetworkObjectReference =
                            new NetworkObjectReference(cardToAdd.gameObject);
                        DestroyCardObjectServerRpc(cardNetworkObjectReference);
                        cardToAdd = null;

                    }
                }
            }

            //if a card exists to be drawn
            if (drawableCard && cardSpawnedValid)
            {
                
                //get difference of cards pos from spawned pos
                Vector3 diff = drawableCard.transform.position - new Vector3(transform.position.x,
                    surfHeight + (10f * pileHeight.Value * cardHeight) - surfOffset, transform.position.z);

                //when distance is greater than 0.01
                if (diff.magnitude > 0.01f)
                {
                    cardSpawnedValid = false;
                    
                    Debug.Log(diff.magnitude);
                    //unlock card, and pop top of stack
                    drawableCard.GetComponent<Card>().SetLocked(false);
                    drawableCard = null;

                    CheckAndPopCurrentPileServerRpc();


                    //if there are still cards in the pile
                    if (pileHeight.Value > 0)
                    {
                        if (NetworkManager.Singleton.IsServer)
                        {
                            SpawnNextCardInPile();

                            if (faceUp)
                            {
                                modelMaterials[2].mainTexture = deckData.cardImages[cardsInPile.Peek()];

                                ChangePileTextureClientRpc(cardsInPile.Peek());
                            }
                        }
                        else
                        {
                            Debug.Log("Spawning Next Card In Pile!!");
                            SpawnNextCardInPileServerRpc();
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

    [ServerRpc(RequireOwnership = false)]
    public void SpawnDrawableCardServerRpc(string cardName)
    {
        SpawnDrawableCard(cardName);
        
        NetworkObjectReference cardNetworkReference = new NetworkObjectReference(drawableCard);
        
        NewCardClientRpc(cardNetworkReference,clientRpcParams);
    }

    [ServerRpc(RequireOwnership = false)]
    public void UpdateDrawableCardServerRpc(string cardName)
    {
        UpdateDrawableCard(cardName);
    }

  

    //instatiate new drawable card and set tex and data from deckData
    public void SpawnDrawableCard(String cardName)
    {
        

        Vector3 initPos = new Vector3(transform.position.x,
            surfHeight + (10f * cardsInPile.Count * cardHeight) - surfOffset, transform.position.z);
        Quaternion initQuat = Quaternion.Euler(new Vector3(cardRot, 0, 0) + transform.eulerAngles);
        
        drawableCard = Instantiate(cardPrefab, initPos, initQuat);
        
        
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
        cardNetworkObject.SpawnWithOwnership(playerID.Value);

        pileHeight.Value = cardsInPile.Count;

        cardSpawnedValid = true;
        
        NetworkObjectReference cardNetworkReference = new NetworkObjectReference(drawableCard);
        ChangeCardTexClientRpc(cardNetworkReference, cardName);
    }

    //peek top and spawn card with that name
    public void SpawnNextCardInPile()
    {
        string newCardName = cardsInPile.Peek();
        SpawnDrawableCard(newCardName);
    }

    //same as above but over ~the network~
    [ServerRpc(RequireOwnership = false)]
    public void SpawnNextCardInPileServerRpc()
    {
        if (cardsInPile.Count > 0)
        {
            SpawnNextCardInPile();
            NetworkObjectReference cardNetworkReference = new NetworkObjectReference(drawableCard);

            NewCardClientRpc(cardNetworkReference, clientRpcParams);

            if (faceUp)
            {
                modelMaterials[2].mainTexture = deckData.cardImages[cardsInPile.Peek()];
                ChangePileTextureClientRpc(cardsInPile.Peek());
            }
        }
    }
    

    [ClientRpc]
    public void NewCardClientRpc(NetworkObjectReference cardObjectReference, ClientRpcParams clientRpcParams)
    {
        cardObjectReference.TryGet(out NetworkObject networkObject);

        drawableCard = networkObject.gameObject;
        cardSpawnedValid = true;
    }

    public void UpdateDrawableCard(Card cardInZone)
    {

        string cardName = cardInZone.cardData.name;
        
        UpdateDrawableCard(cardName);
    }
    
    public void UpdateDrawableCard(string cardName)
    {
        Card drawableCardObj = drawableCard.GetComponent<Card>();

        Texture2D tex = deckData.cardImages[cardName];
        CardData cardData = deckData.cardData[cardName];

        drawableCardObj.cardData = cardData;

        List<Material> materials = new List<Material>();
        drawableCard.GetComponent<Renderer>().GetMaterials(materials);
        materials[2].mainTexture = tex;

        NetworkObjectReference cardNetworkReference = new NetworkObjectReference(drawableCard);
        ChangeCardTexClientRpc(cardNetworkReference, cardName);
    }

    private void UpdatePileHeight(int previousValue, int newValue)
    {
        model.transform.localScale = new Vector3(100, 100, 100 * newValue);
        transform.position = new Vector3(transform.position.x,
            surfHeight + (5f * cardsInPile.Count * cardHeight) + surfOffset, transform.position.z);
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
    

    [ServerRpc(RequireOwnership = false)]
    protected void GetPileHeightServerRpc()
    {
        pileHeight.Value = cardsInPile.Count;
    }

    [ClientRpc]
    protected void ChangePileTextureClientRpc(string cardAtTop)
    {
        Texture2D tex = deckData.cardImages[cardAtTop];
        modelMaterials[2].mainTexture = tex;
    }
    
    [ServerRpc(RequireOwnership = false)]
    protected void ChangePileTextureServerRpc(string cardAtTop)
    {
        Texture2D tex = deckData.cardImages[cardAtTop];
        modelMaterials[2].mainTexture = tex;
        
        ChangePileTextureClientRpc(cardAtTop);
    }

    [ServerRpc(RequireOwnership = false)]
    protected void CheckAndPopCurrentPileServerRpc()
    {
        if (cardsInPile.Count > 0)
        {
            cardsInPile.Pop();
        }
        
        UpdatePileHeight();
        
    }
    

    [ServerRpc(RequireOwnership = false)]
    protected void DestroyCardObjectServerRpc(NetworkObjectReference cardNetworkReference)
    {
        NetworkObject networkObject;
        cardNetworkReference.TryGet(out networkObject);
        networkObject.Despawn();
        Destroy(networkObject.gameObject);
    }

    [ServerRpc(RequireOwnership = false)]
    protected void PushCardToPileServerRpc(string cardName)
    {
        cardsInPile.Push(cardName);
        UpdatePileHeight();
    }

    protected void UpdatePileHeight()
    {
        pileHeight.Value = cardsInPile.Count;
    }
}
