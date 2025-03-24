using System;
using System.Collections.Generic;
using Oculus.Movement.AnimationRigging;
using UnityEngine;
using Unity.Netcode;
using Unity.VisualScripting;

public class NetworkPlayer : NetworkBehaviour
{

    public Transform root;
    public Transform head;
    public Transform leftHand;
    public Transform rightHand;

    //public Renderer[] meshToDisable;
    public SkinnedMeshRenderer[] skinnedMeshToDisable;
    
    public ulong ID;

    [SerializeField]
    public Vector3[] spawns;

    [SerializeField]
    GameObject boardViewer;

    [SerializeField]
    private GameObject cardHandPrefab;
    
    [SerializeField]
    private float rigOffset;

    private void FindSurface()
    {
        
        GameObject[] surfaces = GameObject.FindGameObjectsWithTag("Surface");
        GameObject closest = surfaces[0];
        for (int i = 1; i < surfaces.Length; i++)
        {
            if (Vector3.Distance(surfaces[i].transform.position, VRRigReferences.Singleton.root.position) < Vector3.Distance(closest.transform.position, VRRigReferences.Singleton.root.position))
            { 
                closest = surfaces[i];
            }
                    
        }

        DeckManager.Singleton.surface = closest;
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        if (IsOwner)
        {
            /*foreach (Renderer r in meshToDisable)
            {
                r.enabled = false;
            }

            foreach (SkinnedMeshRenderer s in skinnedMeshToDisable)
            {
                s.enabled = false;
            }*/

            ID = NetworkManager.Singleton.LocalClientId;

            VRRigReferences.Singleton.root.position = spawns[ID] - new Vector3(0,rigOffset,0);
            transform.position= spawns[ID]- new Vector3(0,rigOffset,0);
            transform.eulerAngles = new Vector3(0, 90, 0);

            GameObject spawnedViewer = Instantiate(boardViewer);
            spawnedViewer.transform.eulerAngles = new Vector3(90, 0, 45);

            if (spawns[ID].x != 0)
            {
                VRRigReferences.Singleton.root.eulerAngles = new Vector3(0, -90, 0);
                transform.eulerAngles =new Vector3(0, -90, 0);
                spawnedViewer.transform.eulerAngles = new Vector3(90,135,0);
            }

            
            spawnedViewer.transform.position = spawns[ID] + (VRRigReferences.Singleton.root.right*0.5f)+ (VRRigReferences.Singleton.root.forward * 0.2f);
            
            GameObject cardHand = Instantiate(cardHandPrefab);
            cardHand.transform.position = spawns[ID] + (VRRigReferences.Singleton.root.forward * 0.1f) + (-VRRigReferences.Singleton.root.up * 0.3f);
            cardHand.transform.eulerAngles = new Vector3(0,VRRigReferences.Singleton.root.eulerAngles.y-90,0);

            LocalPlayerManager.Singleton.localPlayerHand = cardHand.transform.Find("CardHand").gameObject;

            //STINKIEST OF ALL HACKS LOOK AWAY
            if (ID == 0)
            {
                spawnedViewer.GetComponent<BoardViewer>().boardView = 1;
            }
            else{
                spawnedViewer.GetComponent<BoardViewer>().boardView = 0;
            }
            spawnedViewer.GetComponent<BoardViewer>().changeTex();

            GameObject[] synthetics = GameObject.FindGameObjectsWithTag("Synthetic");

           // SkeletonProcessAggregator processor = GetComponent<SkeletonProcessAggregator>();
            foreach (GameObject synthetic in synthetics)
            {
              // processor.AddProcessor(synthetic.GetComponentInChildren<SkeletonHandAdjustment>());
            }
            
            
            FindSurface();
        }
        
        
        
    }
    

        // Update is called once per frame
        void Update()
        {
            if (IsOwner)
            {
                /*
                root.position = VRRigReferences.Singleton.root.position;
                root.rotation = VRRigReferences.Singleton.root.rotation;

                head.position = VRRigReferences.Singleton.head.position;
                head.rotation = VRRigReferences.Singleton.head.rotation;

                leftHand.position = VRRigReferences.Singleton.leftHand.position;
                leftHand.rotation = VRRigReferences.Singleton.leftHand.rotation;

                rightHand.position = VRRigReferences.Singleton.rightHand.position;
                rightHand.rotation = VRRigReferences.Singleton.rightHand.rotation;
                */
            }
        }
}

