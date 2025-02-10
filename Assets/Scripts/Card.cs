using Newtonsoft.Json.Bson;
using Oculus.Interaction;
using UnityEngine;

public class Card : MonoBehaviour
{

    Texture2D cardImage;
    public CardData cardData;

    private bool Locked = true;

    public Vector3 lockPos;

    public bool onStack = false;

    [SerializeField]
    PokeInteractable pokeInteractable;

    [SerializeField]
    GrabInteractable grabInteractable;

    PokeInteractor dragger;

    bool dragging;
    bool tapped;
    float doubleTapTimer;

    GameObject hand;
    Quaternion handRotation;
    Vector3 handPosition;

    private void Awake()
    {
        Locked = true;
        
        dragging = false;

        tapped = false;
        doubleTapTimer = 0.0f;
    }

    private void Start()
    {

        //TODO: Replace this with getting the fingertip of current interacting hand
       // hand = GameObject.FindGameObjectWithTag("Fingertip");
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

        pokeInteractable.Disable();
    }

    private void OnTriggerEnter(Collider other)
    {
        //enable poke component when overlapping the table
        if (other.gameObject.tag == "Surface")
        {
            pokeInteractable.Enable();
        }
    }


    private void OnTriggerStay(Collider other)
    {
        //if able to be manipulated
        if (!Locked)
        {

            //when overlapping with the surface object
            if (other.gameObject.tag == "Surface")
            {
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
        }

    private void OnTriggerExit(Collider other)
    {
        //disable poke component when no longer on table
        if ( other.gameObject.tag == "Surface")
        {
            pokeInteractable.Disable();
        }
    }

    public void SetLocked(bool nLocked)
    {
        Locked = nLocked;
    }

     void StartDrag(PokeInteractor pokeInteractor)
    {
        
        dragging = true;
        dragger = pokeInteractor;

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
        dragger = null;
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

}
