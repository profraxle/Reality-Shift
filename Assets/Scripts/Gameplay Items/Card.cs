using System;
using System.Collections.Generic;
using Oculus.Interaction;
using Oculus.Interaction.HandGrab;
using Oculus.Interaction.Surfaces;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Serialization;

public class Card : NetworkBehaviour
{

    //texture of card face
    Texture2D cardImage;
    
    //card's data
    public CardData cardData;

    //a bool to lock the card
    [FormerlySerializedAs("Locked")] public bool locked = true;
    
    //the poke interactable and hand grab interactables
    [SerializeField] public PokeInteractable pokeInteractable;
    [SerializeField] HandGrabInteractable handGrabInteractable;

    //the plane surface of this card
    [SerializeField] private PlaneSurface planeSurface;
    
    //if being dragged
    bool dragging;
    
    //if being grabbed
    public bool grabbed;
    
    //if tapped (rotated 90 degrees)
    bool tapped;
    
    //if in the card hand
    public bool inHand;
    
    //timer to tick down before tapping
    float doubleTapTimer;
    
    //timer to tick down before dragging
    float dragTimer;
    
    //if not able to be moved from deck
    private bool drawLocked;

    //if face up or down
    public bool faceUp;

    //reference to player hand, rotation and position
    GameObject hand;
    Quaternion handRotation;
    Vector3 handPosition;

    //reference to gameplay surface
    public Surface surface;

    //rotation when flipped
    public float flipRot;

    //list of counters placed on this card
    public List<CardCounter> counters = new List<CardCounter>();
    [SerializeField] private GameObject counterPrefab;

    private void Awake()
    {
        //set this to be locked
        locked = true;

        //intialise variables
        dragging = false;
        grabbed = false;
        tapped = false;
        doubleTapTimer = 0.0f;

        inHand = false;

        pokeInteractable.enabled = false;
        
        faceUp = false;
        
        //set flip rotation to -90.f
        flipRot = -90.0f;
        
    }

    private void Update()
    {
        //for each counters attached to the card, remove reference if no longer valid
        foreach (CardCounter counter in counters)
        {
            if (!counter)
            {
                counters.Remove(counter);
            }
        }
        
        //tick down the doubletap timer every frame
        if (doubleTapTimer > 0)
        {
            doubleTapTimer -= Time.deltaTime;
        }

        
        //get the angle between the front of the card and the up vector
        float angle = Vector3.Angle(transform.forward, Vector3.up);

        //if not directly face up or down
        if (angle != 0f && angle != 180f)
        {
            
            //set the angle to be face up for face down with correct new rotation
            if (Vector3.Angle(transform.forward, Vector3.up) > 90)
            {
                faceUp = false;
                flipRot = 90f;
                planeSurface.Facing = PlaneSurface.NormalFacing.Backward;
            }
            else
            {
                faceUp = true;
                flipRot = -90f;
                planeSurface.Facing = PlaneSurface.NormalFacing.Forward;
            }
        }

        
        //if being dragged
        if (dragging)
        {
            //if being dragged for more than alloted time
            if (dragTimer >= 0.1f)
            {
                //get difference in hand rotation this frame
                Quaternion handRotDiff = hand.transform.rotation * Quaternion.Inverse(handRotation);
                handRotation = hand.transform.rotation;

                if (!faceUp)
                {
                    handRotDiff.eulerAngles = new Vector3(0f, handRotDiff.eulerAngles.y * -1f, 0f);
                }

                //get difference in hand position this frame
                Vector3 handPosDiff = hand.transform.position - handPosition;

                handPosition = hand.transform.position;
                
                //set rotations and position to new ones 
                Quaternion newRot = Quaternion.Euler(flipRot, transform.rotation.eulerAngles.y,
                    transform.rotation.eulerAngles.z +  handRotDiff.eulerAngles.y );

                Vector3 newPos = new Vector3(transform.position.x + handPosDiff.x, transform.position.y,
                    transform.position.z + handPosDiff.z);

                transform.SetPositionAndRotation(newPos, newRot);
               
            }
            else
            {
                dragTimer += Time.deltaTime;
            }
        }
    }

    private void OnEnable()
    {
        //bind functions to when this card is poked then disable component until card is on table
        pokeInteractable.WhenSelectingInteractorAdded.Action += StartDrag;

        pokeInteractable.WhenSelectingInteractorRemoved.Action += StopDrag;

        handGrabInteractable.WhenSelectingInteractorAdded.Action += StartGrab;

        handGrabInteractable.WhenSelectingInteractorRemoved.Action += StopGrab;

        pokeInteractable.enabled = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        //if this isnt locked and entering the surface add to the surface and enable the poke component
        if (!locked)
        {
            if (other.CompareTag("Surface") && !inHand)
            {
                surface = other.GetComponent<Surface>();
                surface.AddCardToSurface(this);
                pokeInteractable.enabled = true;
            }

            //if entering cardhand collider, add to the card hand and disable poke interactable components
            if (other.CompareTag("CardHand"))
            {
                other.GetComponent<CardHand>().AddToCardsInHand(gameObject);
                inHand = true;
                if (IsNotGrabbed())
                {
                    other.GetComponent<CardHand>().FinalizeMove(gameObject);
                }

                if (surface != null)
                {
                    surface.RemoveCardFromSurface(this);
                    surface = null;
                }

                pokeInteractable.enabled = false;
            }
        }
    }

    //if staying in the collider, check if its the surface and add to surface if not already
    private void OnTriggerStay(Collider other)
    {
        if (!locked && !inHand)
        {
            if (other.CompareTag("Surface") && !pokeInteractable.enabled)
            {
                surface = other.GetComponent<Surface>();
                surface.AddCardToSurface(this);
                pokeInteractable.enabled = true;
            }
        }
    }


    //when leaving collider either remove it from the surface or cardhand depending on which one
    private void OnTriggerExit(Collider other)
    {
        if (!locked)
        {
            if (other.CompareTag("Surface") && surface)
            {
                surface.RemoveCardFromSurface(this);
                pokeInteractable.enabled = false;
                surface = null;
            }

            if (other.CompareTag("CardHand"))
            {
                other.GetComponent<CardHand>().RemoveFromCardsInHand(gameObject);
                inHand = false;
            }
        }
    }


    public void SetLocked(bool nLocked)
    {
        locked = nLocked;
    }

    //start a drag on beiong poked
    void StartDrag(PokeInteractor pokeInteractor)
    {

        //if a counter is being added, add a counter on poke
        if (LocalPlayerManager.Singleton.addingToken)
        {
            AddCounterToCardServerRpc();
            LocalPlayerManager.Singleton.addingToken = false;
        }
        
        //set dragging variable to true
        dragging = true;
        
        //get fingertip from hand
        GetFingerTip(pokeInteractor);

        //get hand position and rotation
        handRotation = hand.transform.rotation;
        handPosition = hand.transform.position;

        //initialise drag timer
        dragTimer = 0f;

        //if the doubletap timer is greater than 0, rotate it 90 degrees, set tapped to true, else undo that
        if (doubleTapTimer > 0)
        {
            if (!tapped)
            {
                transform.rotation = Quaternion.Euler(transform.rotation.eulerAngles.x,
                    transform.rotation.eulerAngles.y, transform.rotation.eulerAngles.z + 90);
                tapped = true;

            }
            else
            {
                tapped = false;
                transform.rotation = Quaternion.Euler(transform.rotation.eulerAngles.x,
                    transform.rotation.eulerAngles.y, transform.rotation.eulerAngles.z - 90);
            }

            doubleTapTimer = 0.0f;
        }
        else
        {
            //start doubletap timer
            doubleTapTimer = 0.5f;
        }
    }

    //add a counter into the list and create the counter object to be a child of this card on the server
    [ServerRpc (RequireOwnership = false)]
    public void AddCounterToCardServerRpc()
    {
        if (counters.Count == 0)
        {
            GameObject newCounter = Instantiate(counterPrefab, transform);
            counters.Add(newCounter.GetComponent<CardCounter>());
            
            newCounter.GetComponent<NetworkObject>().SpawnWithOwnership(GetComponent<NetworkObject>().OwnerClientId);
            
            newCounter.transform.SetParent(this.transform,true);
            
            NetworkObjectReference reference = new NetworkObjectReference(newCounter);
            
            AddCounterToCardClientRpc(reference);
        }
    }
    
    //add the counter callback on the client
    [ClientRpc]
    public void AddCounterToCardClientRpc(NetworkObjectReference reference)
    {
        reference.TryGet(out NetworkObject networkObject);

        counters.Add(networkObject.gameObject.GetComponent<CardCounter>());
        networkObject.gameObject.transform.localRotation = Quaternion.Euler(180,0,-90);
        networkObject.gameObject.transform.localPosition = new Vector3(0,0.0f,0.00001f);
    }

    //set dragging to false on stop dragging
    void StopDrag(PokeInteractor pokeInteractor)
    {
        dragging = false;
    }

    //get the position of the object tagged fingertip
    void GetFingerTip(PokeInteractor pokeInteractor)
    {
        Transform[] children = pokeInteractor.GetComponentsInChildren<Transform>();

        foreach (Transform child in children)
        {
            if (child.CompareTag("Fingertip"))
            {
                hand = child.gameObject;
            }
        }

    }

    //function for when the object is grabbed
    void StartGrab(HandGrabInteractor grabInteractor)
    {
        //set grabbed bool to true
        grabbed = true;
        if (inHand)
        {
            //begin moving this card if its already in the hand
            LocalPlayerManager.Singleton.localPlayerHand.GetComponent<CardHand>().BeginMove(gameObject);
        }

        //if its on the surface, remover it and disable the pokeInteractable
        if (surface)
        {
            surface.RemoveCardFromSurface(this);
            pokeInteractable.enabled = false;
        }
    }

    //function for when the card is stopped grab
    void StopGrab(HandGrabInteractor grabInteractor)
    {
        grabbed = false;

        if (inHand)
        {
            //finalise move in card hand
            LocalPlayerManager.Singleton.localPlayerHand.GetComponent<CardHand>().FinalizeMove(gameObject);
        }
        
    }


    //checks if the card is not being grabbed for purposes of adding back to deck or other zone
    public bool IsNotGrabbed()
    {
        if (!dragging && !grabbed)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    //set this to be draw locked, so it cant be grabbed from the deck
    public void SetDrawLocked(bool nLocked)
    {
        drawLocked = nLocked;

        if (drawLocked)
        {
            handGrabInteractable.enabled = false;
        }
        else
        {
            handGrabInteractable.enabled = true;
        }
    }

    //destroy poke interactable on this destroyed
    private void OnDestroy()
    {
        Destroy(pokeInteractable.gameObject);
    }

    
    //parent this to the anchor for transform parity
    public void ParentToAnchor(ulong owner)
    {
        transform.SetParent(DeckManager.Singleton.anchors[owner].transform);
        GetComponent<NetworkTransformClient>().InLocalSpace = true;
    }
    
}
