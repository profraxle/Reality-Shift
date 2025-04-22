using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;


//base class for any pile of cards, such as the Deck or Zones (Graveyard, Exile, Face down Exile)
public class CardPile : NetworkBehaviour
{
    #region Header
    // Start is called once before the first execution of Update after the MonoBehaviour is created

    public GameObject cardPrefab;

    //reference to card object being added at top of pile
    public Card cardToAdd;

    //stack of all card names currently in the pile of cards
    public Stack<string> cardsInPile;
    
    //the model of the deck to scale with stack size
    [SerializeField] protected GameObject model;

    //object representing the card that can be drawn from the deck
    public GameObject drawableCard;

    //data descriptor of the deck
    public DeckData deckData;
    
    //the name of the top card on deck
    public string topCardName;

    //list of materials to edit model 
    protected List<Material> modelMaterials;

    //the Y position of the surface
    protected float surfHeight;
    
    //the offset of the cards from the surface
    public float surfOffset;
    
    //the height of card objects
    public float cardHeight;

    //network synced variable to hold the ID of the deck's player
    public NetworkVariable<ulong> playerID;

    //the parameters of the client RPC
    protected ClientRpcParams clientRpcParams;

    //bool to track if deck is faceup or facedown
    public bool faceUp;
    //stores what rotation to spawn card at depending on being faceup or facedown
    private float cardRot;

    //network synced variable storing the amount of cards in the pile
    private NetworkVariable<int> pileHeight = new NetworkVariable<int>(0);

    //bool for callback in card spawning over network
    public bool cardSpawnedValid;

    //the menu to be spawned for searching cards
    [SerializeField] private GameObject searchableMenu;

    //the item selected from the searching menu
    [SerializeField] private GameObject searchCardItem;

    //a sorted list of the cards remaining in the pile
    public List<string> searchableList;

    //a cached card stored when the 
    private GameObject cachedDrawable;

    //bool to check if cards should be added to the 
    private bool addingTop;

    private bool searching;
    
    #endregion 
    
    protected virtual void Start()
    {
        //set the card rotation if its faceup or facedown
        cardRot = faceUp ? -90f : 90f;
        
        //load card prefab from game resources folder
        cardPrefab = Resources.Load<GameObject>("Card");

        //set the drawable card to null
        drawableCard = null;

        //create stack to store cards in pile
        cardsInPile = new Stack<string>();

        //get materials of model and store them
        modelMaterials = new List<Material>();
        model.GetComponent<Renderer>().GetMaterials(modelMaterials);

        //set the surface height to the height of the surface object
        surfHeight = GameObject.FindGameObjectWithTag("Surface").transform.position.y;
        cardHeight = 3e-05f;
        surfOffset = cardHeight / 2f;

        //set the position of this to the surface + the offset
        transform.position = new Vector3(transform.position.x, surfHeight + surfOffset, transform.position.z);

        //define the client RPC parameters
        clientRpcParams = new ClientRpcParams
        {
            Send = new ClientRpcSendParams
            {
                TargetClientIds = new ulong[] { playerID.Value }
            }
        };

        //bind function to when update pile height is updated
        pileHeight.OnValueChanged += UpdatePileHeight;

        //set the scale to 100 times the model because the model's too small 
        model.transform.localScale = new Vector3(100, 100, 0);

        //bool to check if the spawned card is valid
        cardSpawnedValid = false;

        //bool to set where new added cards go, on top or bottom
        addingTop = true;

        searching = false;
        
        //wait a delay and give reference of this to surface
        StartCoroutine(AddToSurface());
    }

    //give reference of this to the surface
    IEnumerator AddToSurface()
    {
        yield return new WaitForSeconds(0.5f);
        DeckManager.Singleton.surface.GetComponent<Surface>().AddToPiles(gameObject);
    }

    // Update is called once per frame
    protected virtual void Update()
    {
        //if this pile is being controlled by the local player
        if (playerID.Value == NetworkManager.Singleton.LocalClientId)
        {
            
            //if the surface exists, set the surface height to the surface's height
            if (DeckManager.Singleton.surface)
            {
                surfHeight = DeckManager.Singleton.surface.transform.position.y;
            }

            //if this is being processed on the server, update the pile height
            if (NetworkManager.Singleton.IsServer)
            {
                if (pileHeight.Value != cardsInPile.Count)
                {
                    UpdatePileHeight();
                }
            }

            //if a card's being added
            if (cardToAdd && !cachedDrawable)
            {

                
                
                //add to the pile when released
                if (cardToAdd.IsNotGrabbed())
                {
                    if (cardToAdd.cardData.name == "Token")
                    {
                        DestroyCardObjectServerRpc(cardToAdd.gameObject);
                        cardToAdd = null;
                    }
                    else
                    {


                        //if there's not an objects for a drawable card, spawn with with correct data
                        if (!drawableCard)
                        {
                            cardSpawnedValid = false;

                            //handle on server or client
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
                            //if adding to the top, update the card data of the drawable card
                            if (addingTop)
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
                        }

                        //add the card name to the cards in Zone
                        if (addingTop)
                        {
                            //handle on server or client request to server
                            if (NetworkManager.Singleton.IsServer)
                            {
                                cardsInPile.Push(cardToAdd.cardData.name);
                            }
                            else
                            {
                                cardsInPile.Push(cardToAdd.cardData.name);
                                PushCardToPileServerRpc(cardToAdd.cardData.name);
                            }
                        }
                        else
                        {
                            //add card to bottom on server or remote call to server
                            if (NetworkManager.Singleton.IsServer)
                            {
                                AddCardOnBottom(cardToAdd.cardData.name);
                            }
                            else
                            {
                                AddCardOnBottomServerRpc(cardToAdd.cardData.name);
                            }
                        }


                        if (faceUp)
                        {
                            //get texture of adding card and set pile top texture to it
                            if (deckData.cardImages.ContainsKey(cardToAdd.cardData.name))
                            {
                                Texture2D tex = deckData.cardImages[cardToAdd.cardData.name];
                                modelMaterials[2].mainTexture = tex;

                                ChangePileTextureServerRpc(cardToAdd.cardData.name);
                            }
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
                            //send reference of card to be destroyed to server
                            NetworkObjectReference cardNetworkObjectReference =
                                new NetworkObjectReference(cardToAdd.gameObject);
                            DestroyCardObjectServerRpc(cardNetworkObjectReference);
                            cardToAdd = null;

                        }
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
                if (diff.magnitude > 0.01f && !cachedDrawable && cardSpawnedValid)
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
                        //spawn the next card if there's one in the pile
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
                            SpawnNextCardInPileServerRpc();
                        }


                    }
                    else
                    {
                        //set the deck texture to null if face up
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
                //if the compared card is a new card, lock it so it doesnt align to surface
                if (other.gameObject != drawableCard)
                {
                    Card otherCard = other.GetComponent<Card>();
                    if (!otherCard.locked)
                    {
                        cardToAdd = otherCard;
                        cardToAdd.SetLocked(true);
                    }
                }
            }
            else
            {
                Card otherCard = other.GetComponent<Card>();
                if (!otherCard.locked)
                {
                    cardToAdd = otherCard;
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
                //unlock card if its leaving zone
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

        NewCardClientRpc(cardNetworkReference, clientRpcParams);
    }

    [ServerRpc(RequireOwnership = false)]
    public void UpdateDrawableCardServerRpc(string cardName)
    {
        UpdateDrawableCard(cardName);
    }

    [ServerRpc(RequireOwnership = false)]
    public void UpdateDrawableCardWithPeekServerRpc()
    {
        UpdateDrawableCard(cardsInPile.Peek());
    }




    //instatiate new drawable card and set tex and data from deckData
    public void SpawnDrawableCard(String cardName)
    {
        //initialise the card object at the top of the deck
        Vector3 initPos = new Vector3(transform.position.x,
            surfHeight + (10f * cardsInPile.Count * cardHeight) - surfOffset, transform.position.z);
        Quaternion initQuat = Quaternion.Euler(new Vector3(cardRot, 0, 0) + transform.eulerAngles);

        drawableCard = Instantiate(cardPrefab, initPos, initQuat);


        //intialise the card component's values 
        Card drawableCardObj = drawableCard.GetComponent<Card>();
        drawableCardObj.SetLocked(true);
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
        drawableCardObj.ParentToAnchor(playerID.Value);

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
        
        //drawableCard.GetComponent<Card>().ParentToAnchor(playerID.Value);
        
        Vector3 initPos = new Vector3(transform.position.x,
            surfHeight + (10f * cardsInPile.Count * cardHeight) - surfOffset, transform.position.z);
        Quaternion initQuat = Quaternion.Euler(new Vector3(cardRot, 0, 0) + transform.eulerAngles);
        drawableCard.transform.SetPositionAndRotation(initPos,initQuat);
        
        cardSpawnedValid = true;
    }

    public void UpdateDrawableCard(Card cardInZone)
    {

        string cardName = cardInZone.cardData.name;

        UpdateDrawableCard(cardName);
    }

    //update the drawable card data with the new card
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
    
    //update the model's height
    private void UpdatePileHeight(int previousValue, int newValue)
    {
        model.transform.localScale = new Vector3(100, 100, 100 * newValue);
        transform.position = new Vector3(transform.position.x,
            surfHeight + (5f * cardsInPile.Count * cardHeight) + surfOffset, transform.position.z);
    }


    //change the texture of the card using the card name in the dictionary
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


    //return the card pile height from server
    [ServerRpc(RequireOwnership = false)]
    protected void GetPileHeightServerRpc()
    {
        pileHeight.Value = cardsInPile.Count;
    }

    //callback for changing card texture from server
    [ClientRpc]
    protected void ChangePileTextureClientRpc(string cardAtTop)
    {
        Texture2D tex = deckData.cardImages[cardAtTop];
        modelMaterials[2].mainTexture = tex;
    }

    //change the texture of the model on the server and callback for the client
    [ServerRpc(RequireOwnership = false)]
    protected void ChangePileTextureServerRpc(string cardAtTop)
    {
        Texture2D tex = deckData.cardImages[cardAtTop];
        modelMaterials[2].mainTexture = tex;

        ChangePileTextureClientRpc(cardAtTop);
    }

    //check the top of the deck and pop it on the server
    [ServerRpc(RequireOwnership = false)]
    protected void CheckAndPopCurrentPileServerRpc()
    {
        if (cardsInPile.Count > 0)
        {
            cardsInPile.Pop();
        }

        UpdatePileHeight();

    }


    //deference and destroy the card object on the server
    [ServerRpc(RequireOwnership = false)]
    protected void DestroyCardObjectServerRpc(NetworkObjectReference cardNetworkReference)
    {
        NetworkObject networkObject;
        cardNetworkReference.TryGet(out networkObject);
        networkObject.Despawn();
        Destroy(networkObject.gameObject);
    }

    //push card to pile on server
    [ServerRpc(RequireOwnership = false)]
    protected void PushCardToPileServerRpc(string cardName)
    {
        cardsInPile.Push(cardName);
        UpdatePileHeight();
    }

    //add card to the bottom of the deck by deconstructing stack and reconstucting it
    protected void AddCardOnBottom(string cardName)
    {
        
        string[] tempCardsInPile = cardsInPile.ToArray();
        
        cardsInPile.Clear();
        cardsInPile.Push(cardName);

        for (int i = tempCardsInPile.Length - 1; i >= 0; i--)
        {
            cardsInPile.Push(tempCardsInPile[i]);
        }
        
    }

    //call AddCardOnBottom from the server
    [ServerRpc(RequireOwnership = false)]
    protected void AddCardOnBottomServerRpc(string cardName)
    {
        AddCardOnBottom(cardName);
        UpdatePileHeight();
    }

    //update the height of the pile
    protected void UpdatePileHeight()
    {
        pileHeight.Value = cardsInPile.Count;
    }


    //start coroutine of quickdrawing cards
    public void QuickDrawCards(int amount)
    {
        if (!cachedDrawable)
        {
            StartCoroutine(QuickDrawCardsCoroutine(amount));
        }
    }

    //coroutine to spawn cards in the player's hand up to a specified amount
    IEnumerator QuickDrawCardsCoroutine(int amount)
    {
        //cache the card on the top of the deck
        GameObject oldDrawCard = drawableCard;

        //loop until the 
        for (int i = 0; i < amount; i++)
        {
            //dereference the drawable card and spawn a new one
            drawableCard = null;
            if (NetworkManager.Singleton.IsServer)
            {
                cardSpawnedValid = false;
                SpawnNextCardInPile();
            }
            else
            {
                cardSpawnedValid = false;
                SpawnNextCardInPileServerRpc();
            }

            //wait until the card spawned (if called from client)
            yield return new WaitUntil(GetCardSpawnedValid);

            //set the position of the new card to the card hand
            drawableCard.GetComponent<Card>().SetLocked(false);
            drawableCard.transform.position = LocalPlayerManager.Singleton.localPlayerHand.transform.position;
            CheckAndPopCurrentPileServerRpc();
        }

        //set the drawable card to the cached card
        drawableCard = oldDrawCard;
        
        //update the drawable card to the top of the deck
        if (NetworkManager.Singleton.IsServer)
        {
            UpdateDrawableCard(cardsInPile.Peek());
        }
        else
        {
            UpdateDrawableCardWithPeekServerRpc();
        }
    }
    
    bool GetCardSpawnedValid()
    {
        return cardSpawnedValid;
    }

    public void SearchCards()
    {
        if (!searching)
        {
            searching = true;
            SearchCardsAsync();
        }
    }

    //create the search menu and add all cards from the card pile as buttons into it in alphabetical order
    async void SearchCardsAsync()
    {
        //lock cards from being drawed while menu is open
        drawableCard.GetComponent<Card>().SetDrawLocked(true);
        
        //cache the drawable card for later
        cachedDrawable = drawableCard;
        
        //turn the stack pile into a sorted array
        string[] searchable = cardsInPile.ToArray();
        searchableList = new List<string>(searchable);
        
        List<string> sortedList = new List<string>(searchable);
        
        sortedList.Sort();
        

        //create menu object, and then create a button for each card to be spawned
        GameObject menuObject =Instantiate(searchableMenu,LocalPlayerManager.Singleton.localPlayerHand.gameObject.transform.position+(Vector3.up*0.2f),Quaternion.Euler(new Vector3(0,VRRigReferences.Singleton.root.eulerAngles.y,0)));
        SearchableMenu menu = menuObject.GetComponent<SearchableMenu>();
        menu.owningPile = this;
        for (int i = 0; i < sortedList.Count; i++)
        {
            GameObject newItem = Instantiate(searchCardItem);
            
            CardSearchCard cardSearchCard = newItem.GetComponent<CardSearchCard>();

            cardSearchCard.owningPile = this;
            cardSearchCard.cardData = deckData.cardData[sortedList[i]];
            cardSearchCard.cardTexture = deckData.cardImages[sortedList[i]];
            cardSearchCard.SetCardImage();
            
            menu.AddToMenu(newItem);
        }
        //update the menu item's positions
        menu.SetMenuItemsPositions();
        await Task.Yield();
    }

    //finalise searching for a card
    public void FinishSearching()
    {
        searching = false;
        //set the drawable card if there's one cached, else delete the drawable card and set everything to null
        
        List<FixedString128Bytes> currentDeckSendable = new List<FixedString128Bytes>(searchableList.Count);

        foreach (var cardName in searchableList)
        {
            currentDeckSendable.Add(new FixedString128Bytes(cardName));
        }
        
        FinishSearchingServerRpc(currentDeckSendable.ToArray());
    }
    

    //call the finish searching on the server
    [ServerRpc(RequireOwnership = false)]
    public void FinishSearchingServerRpc(FixedString128Bytes[]nCurrentDeck)
    {
        
        FinishSearchingClientRpc(nCurrentDeck);
        cardsInPile.Clear();
        cardsInPile = new Stack<String>();
        for (int i = nCurrentDeck.Count() - 1; i >= 0; i--)
        {
            cardsInPile.Push(nCurrentDeck[i].ToString());
        }
    }
    
    //callback to ensure deck data is synced
    [ClientRpc]
    public void FinishSearchingClientRpc( FixedString128Bytes[] nCurrentDeck)
    {
        //empty out the pile and reconstruct it
        cardsInPile.Clear();
        bool updateDrawable = true;
        for (int i = nCurrentDeck.Count() - 1; i >= 0; i--)
        { 
            cardsInPile.Push(nCurrentDeck[i].ToString());
            
            //if top card still exists dont update once over
            if (nCurrentDeck[i].ToString() == cachedDrawable.GetComponent<Card>().cardData.name)
            {
                updateDrawable = false;
            }
        }
        
        
        if (nCurrentDeck.Count() > 0)
        {
            drawableCard = cachedDrawable;
            if (updateDrawable)
            {
                UpdateDrawableCardWithPeekServerRpc();
            }


            drawableCard.GetComponent<Card>().SetDrawLocked(false);
        }
        else
        {
            Destroy(drawableCard);
            drawableCard = null;
        }
        cachedDrawable = null;
        
    }

    //update the position of the drawable card NOT USED ANYMORE
    public void UpdateDrawablePosition()
    {
        if (drawableCard != null)
        {
            surfHeight = DeckManager.Singleton.surface.transform.position.y;
            drawableCard.transform.SetPositionAndRotation(
                new Vector3(transform.position.x, surfHeight + (10f * cardsInPile.Count * cardHeight) - surfOffset,
                    transform.position.z), Quaternion.Euler(new Vector3(cardRot, 0, 0) + transform.eulerAngles));
        }
    }

    //spawn the searched card
    public void DrawSearchedCard(string searchedCardName)
    {
        StartCoroutine(DrawSearchedCardCoroutine(searchedCardName));
    }
    
   
    //spawn the searched card
    IEnumerator  DrawSearchedCardCoroutine(string searchedCardName)
    {
        if (NetworkManager.Singleton.IsServer)
        {
            cardSpawnedValid = false;
            SpawnDrawableCard(searchedCardName);
        }
        else
        {
            cardSpawnedValid = false;
            SpawnDrawableCardServerRpc(searchedCardName);
        }
        
        
        yield return new WaitUntil(GetCardSpawnedValid);
        
        drawableCard.GetComponent<Card>().SetLocked(false);
        drawableCard.transform.position = LocalPlayerManager.Singleton.localPlayerHand.transform.position;
    }

    public void ToggleAddingTop()
    {
        addingTop = !addingTop;
    }

    //parent this to the anchor object so it moves properly across the network
    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        StartCoroutine(HierarchyDelay());
    }

    IEnumerator HierarchyDelay()
    {
        yield return new WaitForSeconds(1f);
        ChangeHierarchyServerRpc();
    }
    
    
    [ServerRpc(RequireOwnership = false)]
    void ChangeHierarchyServerRpc()
    {
        transform.SetParent(DeckManager.Singleton.anchors[playerID.Value].transform,true);
    }
}
