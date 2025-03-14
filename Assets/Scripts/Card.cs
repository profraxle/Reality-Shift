using System;
using Newtonsoft.Json.Bson;
using Oculus.Interaction;
using Oculus.Interaction.HandGrab;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Serialization;

public class Card : MonoBehaviour
{

    Texture2D cardImage;
    public CardData cardData;

    [FormerlySerializedAs("Locked")] public bool locked = true;

    public Vector3 lockPos;

    public bool onStack = false;

    [SerializeField]
    public PokeInteractable pokeInteractable;
    [SerializeField]
    HandGrabInteractable handGrabInteractable;

    bool dragging;
    public bool grabbed;
    bool tapped;
    public bool inHand;
    float doubleTapTimer;
    float dragTimer;

    GameObject hand;
    Quaternion handRotation;
    Vector3 handPosition;
    
    public Surface surface;

    private void Awake()
    {
        locked = true;
        
        dragging = false;
        grabbed = false;

        tapped = false;
        doubleTapTimer = 0.0f;

        inHand = false;
    }

    private void Update()
    {
        //tick down the doubletap timer every frame
       if (doubleTapTimer > 0)
        {
            doubleTapTimer -= Time.deltaTime;
        }
       

        if (dragging)
        {
            if (dragTimer >= 0.1f)
            {
                //get difference in hand rotation this frame
                Quaternion handRotDiff = hand.transform.rotation * Quaternion.Inverse(handRotation);
                handRotation = hand.transform.rotation;

                //get difference in hand position this frame
                Vector3 handPosDiff = hand.transform.position - handPosition;

                handPosition = hand.transform.position;

                Quaternion newRot = Quaternion.Euler(-90, transform.rotation.eulerAngles.y ,
                    transform.rotation.eulerAngles.z+ handRotDiff.eulerAngles.y);

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
        
        pokeInteractable.Disable();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!locked)
        {
            if (other.CompareTag("Surface") && !inHand)
            {
                surface = other.GetComponent<Surface>();
                surface.AddCardToSurface(this);
                pokeInteractable.Enable();
            }

            if (other.CompareTag("CardHand"))
            {
                other.GetComponent<CardHand>().AddToCardsInHand(gameObject);
                inHand = true;
                if (IsNotGrabbed())
                {
                    other.GetComponent<CardHand>().FinalizeMove(gameObject);
                }
                pokeInteractable.Disable();
            }
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (!locked&& !inHand)
        {
            if (other.CompareTag("Surface")  && !surface)
            {
                surface = other.GetComponent<Surface>();
                surface.AddCardToSurface(this);
                pokeInteractable.Enable();
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
                pokeInteractable.Disable();
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
        
        dragging = true;
        GetFingerTip(pokeInteractor);
            
        handRotation = hand.transform.rotation;
        handPosition = hand.transform.position;
        
        dragTimer = 0f;
            
        if (doubleTapTimer > 0)
        {
            if (!tapped)
            {
                transform.rotation = Quaternion.Euler(transform.rotation.eulerAngles.x, transform.rotation.eulerAngles.y , transform.rotation.eulerAngles.z+90);
                tapped = true;

            }
            else
            {
                tapped = false;
                transform.rotation = Quaternion.Euler(transform.rotation.eulerAngles.x, transform.rotation.eulerAngles.y , transform.rotation.eulerAngles.z-90);
            }
            doubleTapTimer = 0.0f;
        }
        else
        {
            doubleTapTimer = 0.5f;
        }
    }

    void StopDrag(PokeInteractor pokeInteractor)
    {
        dragging = false;
    }

    void GetFingerTip(PokeInteractor pokeInteractor)
    {
        Transform [] children = pokeInteractor.GetComponentsInChildren<Transform>();

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
        grabbed =true;
        if (inHand)
        {
            LocalPlayerManager.Singleton.localPlayerHand.GetComponent<CardHand>().BeginMove(gameObject);
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

}
