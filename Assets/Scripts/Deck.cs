using Oculus.Interaction.HandGrab;
using Oculus.Interaction.Input;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using System.Linq;
using System;
using System.Collections;
using Unity.Collections;
using Oculus.Interaction;
using Vector3 = UnityEngine.Vector3;


public class Deck : NetworkBehaviour
{
    int cardsInDeck = 100;

    [SerializeField]
    private GameObject model;

    [SerializeField]
    public GameObject cardPrefab;

    [SerializeField]
    private GameObject drawZone;

    UpdateDrawZone drawZoneUpdater;

    public GameObject currentCard;

    [SerializeField]
    IHand hand;

    [SerializeField]
    IHandGrabInteractor interactor;

    public NetworkVariable<ulong> deckID;

    private bool newCardNeeded = false;

    public DeckData deckData;
    private Stack<string> currentDeck; 

    ClientRpcParams clientRpcParams;

    public SelectorUnityEventWrapper selector;

    [SerializeField]
    private GameObject cardZonePrefab;

    private float surfHeight;
    public float surfOffset;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        surfHeight = GameObject.FindGameObjectWithTag("Surface").transform.position.y;
        if (deckID.Value == NetworkManager.LocalClientId)
        {

            selector = FindFirstObjectByType<SelectorUnityEventWrapper>();
            if (selector != null)
            {
                selector.WhenSelected.AddListener(DrawFromDeck);
            }

            currentDeck = new Stack<string>();

            deckData = LocalPlayerManager.Singleton.GetLocalPlayerDeck();

            foreach (string card in deckData.cardsInDeck)
            {
                currentDeck.Push(card);
            }
            shuffle();

            drawZoneUpdater = drawZone.GetComponent<UpdateDrawZone>();

            cardsInDeck--;
            
            //creates a first card to be grabbable on top of the deck

            List<FixedString128Bytes> currentDeckSendable = new List<FixedString128Bytes>(currentDeck.Count);

            foreach (var cardName in currentDeck)
            {
                currentDeckSendable.Add(new FixedString128Bytes(cardName));
            }

            if (NetworkManager.Singleton.IsClient)
            {
                //send strings of current deck order
                ClientConnectedServerRpc(deckData.deckName, currentDeckSendable.ToArray());
                SpawnFirstCardServerRpc();
            }
            else
            {
                ClientConnectedClientRpc(deckData.deckName, currentDeckSendable.ToArray());
                SpawnFirstCard();
            }

            surfOffset = 0.001f;
            
            model.transform.localScale = new Vector3(100, 100, 100 * currentDeck.Count);
            model.transform.position = new Vector3(transform.position.x, surfHeight+(5f*currentDeck.Count*3e-05f)+surfOffset, transform.position.z);
        }

        StartCoroutine(SpawnZones());
    }

    IEnumerator SpawnZones()
    {
        //wait so position of deck will be correct when called
        yield return new WaitForSeconds(0.5f);

        for (int i = 0; i < 2; i++)
        {
            GameObject newZone =Instantiate(cardZonePrefab, transform.position + (0.8f*transform.forward) + (0.15f*transform.right*i), transform.rotation);
            newZone.GetComponent<CardZone>().owningDeck = this;
        }
    }

    void shuffle()
    {
       
        //fisher-yates shuffle algo

        string[] deckShuffle = currentDeck.ToArray();

        for (int i = deckShuffle.Length - 1; i > 0; i--)
        {
            int j = UnityEngine.Random.Range(0, i + 1);
            (deckShuffle[i], deckShuffle[j]) = (deckShuffle[j], deckShuffle[i]);
        }

        currentDeck = new Stack<string>(deckShuffle);
            
        
    }

    // Update is called once per frame
    void Update()
    {
        if (currentCard)
        {
            Vector3 diff = currentCard.transform.position - new Vector3(transform.position.x, surfHeight+(10f*currentDeck.Count*3e-05f)-surfOffset, transform.position.z);;
            if (diff.magnitude > 0.01)
            {
                currentCard.GetComponent<Card>().SetLocked(false);
                newCardNeeded = true;
            }
        }
        
    }


    /// <summary>
    /// Method is called from Poses Manager when DrawCard Pose is detected. Checks if the hand is overlapping the draw zone and creates a new card on top when a new card is taken
    /// </summary>
    public void DrawFromDeck()
    {
        //checks if the hand is within the draw zone
        if (drawZoneUpdater.GetIsInDrawZone())
        { 
            //checks if currentCard no longer in the spawn position
            if (newCardNeeded)
            { 
                newCardNeeded = false;
                if (NetworkManager.Singleton.IsServer)
                {
                    SpawnNewCard();
                    //spawn a new card to be grabbed and change scale of deck to reflect cards left to be drawn
                    model.transform.localScale = new Vector3(100, 100, 100 * currentDeck.Count);
                    model.transform.position = new Vector3(transform.position.x, surfHeight+(5f*currentDeck.Count*3e-05f)+surfOffset, transform.position.z);
                }
                else 
                { 
                    SpawnNewCardServerRpc(); 
                } 
            }
        }
    }

    public void SpawnFirstCard()
    {
        currentCard = Instantiate(cardPrefab);
        
        currentCard.transform.position = new Vector3(transform.position.x, surfHeight+(10f*currentDeck.Count*3e-05f)-surfOffset, transform.position.z);
        currentCard.transform.eulerAngles = new Vector3(90, 0,0) + transform.eulerAngles;
        Card currentCardObj = currentCard.GetComponent<Card>();
        currentCardObj.SetLocked(true);
        currentCardObj.lockPos = transform.position;

        string cardName = currentDeck.Pop();

        Texture2D tex = deckData.cardImages[cardName];
        CardData cardData = deckData.cardData[cardName];


        //set card face to the current texture
        List<Material> materials = new List<Material>();
        currentCard.GetComponent<Renderer>().GetMaterials(materials);
        materials[2].mainTexture = tex;

        currentCardObj.cardData = cardData;

        var cardNetworkObject = currentCard.GetComponent<NetworkObject>();
        cardNetworkObject.SpawnWithOwnership(deckID.Value);


        NetworkObjectReference cardNetworkReference = new NetworkObjectReference(currentCard);
        ChangeCardTexClientRpc(cardNetworkReference,cardName);

        


    }

    [ServerRpc(RequireOwnership = false)]
    public void SpawnFirstCardServerRpc()
    {

        ClientRpcParams clientRpcParams = new ClientRpcParams
        {
            Send = new ClientRpcSendParams
            {
                TargetClientIds = new ulong[] { deckID.Value }
            }
        };

        SpawnFirstCard();
        this.NetworkObject.ChangeOwnership(deckID.Value);

        NetworkObjectReference cardNetworkReference = new NetworkObjectReference(currentCard);

        newCardClientRpc(cardNetworkReference, clientRpcParams);

    }

    public void SpawnNewCard()
    {
        currentCard = Instantiate(cardPrefab);
        currentCard.transform.position = new Vector3(transform.position.x, surfHeight+(10f*currentDeck.Count*3e-05f)-surfOffset, transform.position.z);
        currentCard.transform.eulerAngles = new Vector3(90, 0, 0) + transform.eulerAngles;
        Card currentCardObj = currentCard.GetComponent<Card>();
        currentCardObj.SetLocked(true);
        currentCardObj.lockPos = transform.position;

        string cardName = currentDeck.Pop();
        Texture2D tex = deckData.cardImages[cardName];
        CardData cardData = deckData.cardData[cardName];

        //set card face to the current texture
        List<Material> materials = new List<Material>();
        currentCard.GetComponent<Renderer>().GetMaterials(materials);
        materials[2].mainTexture = tex;

        currentCardObj.cardData = cardData;

        var cardNetworkObject = currentCard.GetComponent<NetworkObject>();
        cardNetworkObject.SpawnWithOwnership(deckID.Value);


        NetworkObjectReference cardNetworkReference = new NetworkObjectReference(currentCard);
        ChangeCardTexClientRpc(cardNetworkReference, cardName);

    }

    [ServerRpc(RequireOwnership = false)]
    public void SpawnNewCardServerRpc()
    {
        SpawnNewCard();
        //spawn a new card to be grabbed and change scale of deck to reflect cards left to be drawn

        model.transform.localScale = new Vector3(100, 100, 100 * currentDeck.Count);
        model.transform.position = new Vector3(transform.position.x, surfHeight+(10f*currentDeck.Count*3e-05f)-surfOffset, transform.position.z);
        newCardNeeded = false;

        NetworkObjectReference cardNetworkReference= new NetworkObjectReference(currentCard);

        newCardClientRpc(cardNetworkReference, clientRpcParams);
    }

    [ServerRpc]
    public void ClientConnectedServerRpc(string deckName, FixedString128Bytes[]nCurrentDeck )
    {
        ClientConnectedClientRpc(deckName,nCurrentDeck);
        deckData = LocalPlayerManager.Singleton.allDeckData[deckName];
        
        for (int i = nCurrentDeck.Count() -1; i >= 0; i--)
        {
            currentDeck.Push(nCurrentDeck[i].ToString());
        }


    }

    [ClientRpc]
    public void ClientConnectedClientRpc(string deckName, FixedString128Bytes[] nCurrentDeck)
    {
        if (deckID.Value != NetworkManager.LocalClientId)
        {
            currentDeck = new Stack<String>();
            deckData = LocalPlayerManager.Singleton.allDeckData[deckName];
            for (int i = nCurrentDeck.Count() - 1; i >= 0; i--)
            {
                currentDeck.Push(nCurrentDeck[i].ToString());
            }
        }
    }


    [ClientRpc]
    
    public void newCardClientRpc(NetworkObjectReference cardNetworkReference, ClientRpcParams clientRpcParams = default)
    {
        
        NetworkObject networkObject;
        cardNetworkReference.TryGet(out networkObject);

        currentCard = networkObject.gameObject;

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


    

}
