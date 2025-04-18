using System;
using System.Collections.Generic;
using Newtonsoft.Json.Bson;
using Oculus.Interaction;
using Oculus.Interaction.HandGrab;
using Oculus.Interaction.Surfaces;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Serialization;

public class Card : NetworkBehaviour
{

    Texture2D cardImage;
    public CardData cardData;

    [FormerlySerializedAs("Locked")] public bool locked = true;

    public Vector3 lockPos;

    public bool onStack = false;

    [SerializeField] public PokeInteractable pokeInteractable;
    [SerializeField] HandGrabInteractable handGrabInteractable;

    [SerializeField] private PlaneSurface planeSurface;
    
    bool dragging;
    public bool grabbed;
    bool tapped;
    public bool inHand;
    float doubleTapTimer;
    float dragTimer;
    private bool drawLocked;

    public bool faceUp;

    GameObject hand;
    Quaternion handRotation;
    Vector3 handPosition;

    public Surface surface;

    public float flipRot;

    public List<CardCounter> counters = new List<CardCounter>();
    [SerializeField] private GameObject counterPrefab;

    private void Awake()
    {
        locked = true;

        dragging = false;
        grabbed = false;

        tapped = false;
        doubleTapTimer = 0.0f;

        inHand = false;

        pokeInteractable.enabled = false;
        
        faceUp = false;
        
        flipRot = -90.0f;
        
    }

    private void Update()
    {
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

        float angle = Vector3.Angle(transform.forward, Vector3.up);

        if (angle != 0f && angle != 180f)
        {
            
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


        if (dragging)
        {
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
        if (!locked)
        {
            if (other.CompareTag("Surface") && !inHand)
            {
                surface = other.GetComponent<Surface>();
                surface.AddCardToSurface(this);
                pokeInteractable.enabled = true;
            }

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

    void StartDrag(PokeInteractor pokeInteractor)
    {

        if (LocalPlayerManager.Singleton.addingToken)
        {
            AddCounterToCardServerRpc();
            LocalPlayerManager.Singleton.addingToken = false;
        }
        
        dragging = true;
        GetFingerTip(pokeInteractor);

        handRotation = hand.transform.rotation;
        handPosition = hand.transform.position;

        dragTimer = 0f;

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
            doubleTapTimer = 0.5f;
        }
    }

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
    
    [ClientRpc]
    public void AddCounterToCardClientRpc(NetworkObjectReference reference)
    {
        reference.TryGet(out NetworkObject networkObject);

        counters.Add(networkObject.gameObject.GetComponent<CardCounter>());
        networkObject.gameObject.transform.localRotation = Quaternion.Euler(180,0,-90);
        networkObject.gameObject.transform.localPosition = new Vector3(0,0.0f,0.00001f);
    }

    void StopDrag(PokeInteractor pokeInteractor)
    {
        dragging = false;
    }

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

    void StartGrab(HandGrabInteractor grabInteractor)
    {
        grabbed = true;
        if (inHand)
        {
            LocalPlayerManager.Singleton.localPlayerHand.GetComponent<CardHand>().BeginMove(gameObject);
        }

        if (surface)
        {
            surface.RemoveCardFromSurface(this);
            pokeInteractable.enabled = false;
        }
    }

    void StopGrab(HandGrabInteractor grabInteractor)
    {
        grabbed = false;

        if (inHand)
        {
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

    private void OnDestroy()
    {
        Destroy(pokeInteractable.gameObject);
    }

    public void ParentToAnchor(ulong owner)
    {
        transform.SetParent(DeckManager.Singleton.anchors[owner].transform);
        GetComponent<NetworkTransformClient>().InLocalSpace = true;
    }
}
