using Newtonsoft.Json.Bson;
using Oculus.Interaction;
using Oculus.Interaction.HandGrab;
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
    bool grabbed;
    bool tapped;
    float doubleTapTimer;

    GameObject hand;
    Quaternion handRotation;
    Vector3 handPosition;

    private void Awake()
    {
        locked = true;
        
        dragging = false;
        grabbed = false;

        tapped = false;
        doubleTapTimer = 0.0f;
    }

    private void Update()
    {
        //tick down the doubletap timer every frame
       if (doubleTapTimer > 0)
        {
            doubleTapTimer -= Time.deltaTime;
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

    }


    private void OnTriggerStay(Collider other)
    {
        //if able to be manipulated
        if (!locked)
        {

            
            
            //when overlapping with the surface object
            if (other.gameObject.CompareTag("Surface"))
            {
                
                pokeInteractable.Enable();
                
                
                //set rotation and position of card to be sat on tabletop
                Quaternion newRot = Quaternion.Euler(-90, transform.rotation.eulerAngles.y, transform.rotation.eulerAngles.z);

                Vector3 newPos = new Vector3(transform.position.x, other.transform.position.y, transform.position.z);

                transform.SetPositionAndRotation(newPos, newRot);
            }


            //if being dragged by player poking
            if (dragging)
            {
                //get difference in hand rotation this frame
                Quaternion handRotDiff = hand.transform.rotation * Quaternion.Inverse(handRotation);
                handRotation = hand.transform.rotation;

                //get difference in hand position this frame
                Vector3 handPosDiff = hand.transform.position - handPosition;
                handPosition = hand.transform.position;

                //increase cards y angle by the difference y angle
                Quaternion newRot = Quaternion.Euler(-90, transform.rotation.eulerAngles.y + handRotDiff.eulerAngles.y, transform.rotation.eulerAngles.z);

                //add hand position diff to x and y axes
                Vector3 newPos = new Vector3(transform.position.x + handPosDiff.x, transform.position.y, transform.position.z + handPosDiff.z);

                transform.SetPositionAndRotation(newPos, newRot);
            }
        }
        else
        {
            if (other.gameObject.CompareTag("Surface"))
            {
               
                pokeInteractable.Disable();
                
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.CompareTag("Surface"))
        {
               
            pokeInteractable.Disable();
                
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
            
        if (doubleTapTimer > 0)
        {
            if (!tapped)
            {
                transform.rotation = Quaternion.Euler(transform.rotation.eulerAngles.x, transform.rotation.eulerAngles.y +90, transform.rotation.eulerAngles.z);
                tapped = true;

            }
            else
            {
                tapped = false;
                transform.rotation = Quaternion.Euler(transform.rotation.eulerAngles.x, transform.rotation.eulerAngles.y - 90, transform.rotation.eulerAngles.z);
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
    }

    void StopGrab(HandGrabInteractor grabInteractor)
    {
        grabbed = false;
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
