using System;
using System.Collections.Generic;
using Oculus.Interaction;
using Oculus.Interaction.HandGrab;
using TMPro;
using UnityEngine;

public class BoardViewer : MonoBehaviour
{
    //list of rendertextures for the board viewin g
    public RenderTexture[] boards;
    
    //integer storing the target board to view
    public int boardView;

    //reference to the component handling hand grabs
    [SerializeField] HandGrabInteractable handGrabInteractable;

    //reference to the component handling pokes
    [SerializeField] PokeInteractable pokeInteractable;

    //vector storing the local position of the touch
    private Vector3 localTouchPos;

    //opposing empty game objects that are placed at the corners of the plane to determine the plane's size
    [SerializeField] private GameObject firstPoint;
    [SerializeField] private GameObject secondPoint;

    //vector 3 storing the size of this object
    private Vector3 size;
    
    //the plane to display the selected card to the player
    [SerializeField]
    private GameObject cardPreview;
    
    //a mesh used for debugging where the tapped location is on the board
    [SerializeField]
    private GameObject debugMesh;

    //the text to be drawn if there's a counter
    [SerializeField] private TextMeshPro counterText;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created

    private void OnEnable()
    {
        //bind functions to starting and ending grab and poke interactions
        handGrabInteractable.WhenSelectingInteractorAdded.Action += OnGrab;
        handGrabInteractable.WhenSelectingInteractorAdded.Action += OnRelease;
        
        pokeInteractable.WhenSelectingInteractorAdded.Action += OnPoke;
        
    }

    void Start()
    {
        //determine the size of the plane on the viewer by finding the difference in the position of the corners
        size = secondPoint.transform.position - firstPoint.transform.position;
        size = new Vector3(Mathf.Abs(size.x), Mathf.Abs(size.y), Mathf.Abs(size.z));
        size = size / 2;
        
        //set the material of the card preview to a null texture
        List<Material> materials = new List<Material>();
        cardPreview.GetComponent<Renderer>().GetMaterials(materials);
        materials[0].mainTexture = null;
    }

    public void changeTex()
    {
        //change the texture of the boardviewer to the correct render texture
        Renderer renderer = GetComponent<Renderer>();
        Material mat = renderer.material;
        mat.SetTexture("_BaseMap", boards[boardView]);
    }

    void OnGrab(HandGrabInteractor interactor)
    {
        //disable the poke interactor on grab to avoid interference
        pokeInteractable.enabled = false;
    }

    void OnRelease(HandGrabInteractor interactor)
    {
        //enable the poke interactor on grab release
        pokeInteractable.enabled = true;
    }

    void OnPoke(PokeInteractor interactor)
    {
        //get the local position of where the pokeInteractor has touched the plane
        localTouchPos = Vector3.ProjectOnPlane(interactor.TouchPoint,interactor.TouchNormal) - Vector3.ProjectOnPlane(transform.position,interactor.TouchNormal);
        
        //get the camera number
        int camNum = boardView + 1;
        string cameraTag = "boardCam" + camNum.ToString();
        
        //get the camera that this viewer is looking at
        GameObject camera = GameObject.FindGameObjectWithTag(cameraTag);
        
        
        //rotate the point to be from the local point to facing up
        Quaternion rotation1 = Quaternion.FromToRotation(interactor.TouchNormal, -camera.transform.forward);
        Quaternion rotation2 = Quaternion.FromToRotation(transform.right, camera.transform.right);
        
        Quaternion rotation3 = Quaternion.Euler(new Vector3(0,0,0));

        rotation3 = Quaternion.Euler(new Vector3(0,180,0));


        //apply rotations to the position
        Vector3 rotatedPosition = rotation1 * localTouchPos;
        rotatedPosition = rotation2 * rotatedPosition;
        rotatedPosition = rotation3 * rotatedPosition;
        
        //cast the rotated position from the bounds of the board viewer to the bounds of the table
        rotatedPosition  = new Vector3(rotatedPosition .x/(0.54f/2f), rotatedPosition .y, rotatedPosition .z/(0.3f/2f)); 
        
        rotatedPosition  = new Vector3(rotatedPosition .x * (0.93f/2f), 0, rotatedPosition .z * (0.52f/2f));
        
    
        //do a raycast from the selected point downwards, to hit a card
        RaycastHit[] hits = Physics.RaycastAll(camera.transform.position+rotatedPosition,Vector3.down, Mathf.Infinity,LayerMask.GetMask("GameBoard"),QueryTriggerInteraction.Collide);

        //for each object hit
        foreach (RaycastHit hit in hits)
        {
            if (hit.collider != null)
            {
                //if the collider is a card
                if (hit.collider.gameObject.tag == "Card")
                {
                    
                    //get the card counter value and texture and set the smaller plane's texture to the card texture
                    GameObject card = hit.collider.gameObject;

                    if (card.GetComponent<Card>().faceUp)
                    {

                        if (card.GetComponent<Card>().counters.Count > 0)
                        {
                            counterText.text = card.GetComponent<Card>().counters[0].counter.Value.ToString();
                        }
                        else
                        {
                            counterText.text = "";
                        }
                        
                        List<Material> materials1 = new List<Material>();
                        card.GetComponent<Renderer>().GetMaterials(materials1);

                        Texture tex = materials1[2].mainTexture;

                        List<Material> materials2 = new List<Material>();
                        cardPreview.GetComponent<Renderer>().GetMaterials(materials2);
                        materials2[0].mainTexture = tex;
                        break;
                    }
                }
            }
        }


    }



}
