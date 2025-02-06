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

    GameObject hand;

    private void Awake()
    {
        Locked = true;
        
        dragging = false;

        
    }

    private void Start()
    {
        //grabbable = GetComponent<Grabbable>();
        hand = GameObject.FindGameObjectWithTag("Fingertip");
    }

    private void Update()
    {
       
    }

    private void OnEnable()
    {
        pokeInteractable.WhenInteractorAdded.Action += StartDrag;
        
        pokeInteractable.WhenInteractorRemoved.Action += StopDrag;


        pokeInteractable.Disable();
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void OnTriggerEnter(Collider other)
    {
        //

        
    }

    private void OnTriggerStay(Collider other)
    {
        if (!Locked)
        {

            if (other.gameObject.tag == "Surface")
            {
                pokeInteractable.Enable();    

                Quaternion newRot = Quaternion.Euler(-90, transform.rotation.eulerAngles.y, transform.rotation.eulerAngles.z);

                Vector3 newPos = new Vector3(transform.position.x, other.transform.position.y, transform.position.z);

                transform.SetPositionAndRotation(newPos, newRot);
            }



            if (dragging)
            {
                Quaternion newRot = Quaternion.Euler(-90, hand.transform.rotation.eulerAngles.y, transform.rotation.eulerAngles.z);
                Vector3 newPos = new Vector3(hand.transform.position.x, transform.position.y, hand.transform.position.z);

                transform.SetPositionAndRotation(newPos, newRot);
            }
        }
        }

    private void OnTriggerExit(Collider other)
    {
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
    }

    void StopDrag(PokeInteractor pokeInteractor)
    {
        dragging = false;
        dragger = null;
    }

}
