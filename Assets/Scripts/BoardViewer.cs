using System;
using System.Collections.Generic;
using Oculus.Interaction;
using Oculus.Interaction.HandGrab;
using UnityEngine;

public class BoardViewer : MonoBehaviour
{
    public RenderTexture[] boards;
    public int boardView;

    [SerializeField] HandGrabInteractable handGrabInteractable;

    [SerializeField] PokeInteractable pokeInteractable;

    private Vector3 localTouchPos;

    [SerializeField] private GameObject firstPoint;
    [SerializeField] private GameObject secondPoint;

    private Vector3 size;
    
    [SerializeField]
    private GameObject cardPreview;
    
    [SerializeField]
    private GameObject debugMesh;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created

    private void OnEnable()
    {
        handGrabInteractable.WhenSelectingInteractorAdded.Action += OnGrab;
        handGrabInteractable.WhenSelectingInteractorAdded.Action += OnRelease;
        
        pokeInteractable.WhenSelectingInteractorAdded.Action += OnPoke;
        
    }

    void Start()
    {
        size = secondPoint.transform.position - firstPoint.transform.position;
        size = new Vector3(Mathf.Abs(size.x), Mathf.Abs(size.y), Mathf.Abs(size.z));
        size = size / 2;
        
        Debug.Log(size);
        
        List<Material> materials2 = new List<Material>();
        cardPreview.GetComponent<Renderer>().GetMaterials(materials2);
        materials2[0].mainTexture = null;
    }

    public void changeTex()
    {
        Renderer renderer = GetComponent<Renderer>();
        Material mat = renderer.material;
        mat.SetTexture("_BaseMap", boards[boardView]);
    }

    void OnGrab(HandGrabInteractor interactor)
    {
        pokeInteractable.enabled = false;
    }

    void OnRelease(HandGrabInteractor interactor)
    {
        pokeInteractable.enabled = true;
    }

    void OnPoke(PokeInteractor interactor)
    {
        localTouchPos = Vector3.ProjectOnPlane(interactor.TouchPoint,interactor.TouchNormal) - Vector3.ProjectOnPlane(transform.position,interactor.TouchNormal);

        //localTouchPos = Vector3.ProjectOnPlane(localTouchPos, Vector3.up);

        //localTouchPos = transform.InverseTransformPoint(interactor.TouchPoint);
        
        int camNum = boardView + 1;
        string cameraTag = "boardCam" + camNum.ToString();
        
        GameObject camera = GameObject.FindGameObjectWithTag(cameraTag);
        
        
        Quaternion rotation1 = Quaternion.FromToRotation(interactor.TouchNormal, -camera.transform.forward);
        Quaternion rotation2 = Quaternion.FromToRotation(transform.right, camera.transform.right);
        
        Quaternion rotation3 = Quaternion.Euler(new Vector3(0,0,0));
       // if (boardView == 1)
      //  {
            rotation3 = Quaternion.Euler(new Vector3(0,180,0));
      //  }

        
        Vector3 rotatedPosition = rotation1 * localTouchPos;
        rotatedPosition = rotation2 * rotatedPosition;
        rotatedPosition = rotation3 * rotatedPosition;
        
        rotatedPosition  = new Vector3(rotatedPosition .x/(0.54f/2f), rotatedPosition .y, rotatedPosition .z/(0.3f/2f)); 
        
        rotatedPosition  = new Vector3(rotatedPosition .x * (0.93f/2f), 0, rotatedPosition .z * (0.52f/2f));

        //Instantiate(debugMesh, camera.transform.position+rotatedPosition, Quaternion.identity);
    
        RaycastHit[] hits = Physics.RaycastAll(camera.transform.position+rotatedPosition,Vector3.down, Mathf.Infinity,LayerMask.GetMask("GameBoard"),QueryTriggerInteraction.Collide);

        foreach (RaycastHit hit in hits)
        {
            if (hit.collider != null)
            {
                if (hit.collider.gameObject.tag == "Card")
                {
                    GameObject card = hit.collider.gameObject;

                    if (card.GetComponent<Card>().faceUp)
                    {

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
