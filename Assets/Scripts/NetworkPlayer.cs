using System;
using System.Collections.Generic;
using Oculus.Movement.AnimationRigging;
using UnityEngine;
using Unity.Netcode;
using Unity.VisualScripting;

//class to handle player functionality across network
public class NetworkPlayer : NetworkBehaviour
{
    //id of the player
    public ulong ID;

    //list of spawn locations
    [SerializeField]
    public Vector3[] spawns;

    //the board viewer prefab
    [SerializeField]
    GameObject boardViewer;

    //the card hand prefab
    [SerializeField]
    private GameObject cardHandPrefab;
    
    //offset to the player model rig
    [SerializeField]
    private float rigOffset;

    //the prefab of the anchor
    [SerializeField] private GameObject anchorPrefab;

    //parameters for client RPCs
    ClientRpcParams clientRpcParams;
    private void FindSurface()
    {
        //find the closest surface object to the player and store it in the deck manager 
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

    //when created on the network
    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        
        //if this is the owning player
        if (IsOwner)
        {
            
            clientRpcParams = new ClientRpcParams
            {
                Send = new ClientRpcSendParams
                {
                    TargetClientIds = new ulong[] { NetworkManager.Singleton.LocalClientId }
                }
            };
            
            SpawnAnchorServerRpc(NetworkManager.Singleton.LocalClientId);

            ID = NetworkManager.Singleton.LocalClientId;

            //set the position to the spawnpoint for this ID
            VRRigReferences.Singleton.root.position = spawns[ID] - new Vector3(0,rigOffset,0);
            transform.position= spawns[ID]- new Vector3(0,rigOffset,0);
            transform.eulerAngles = new Vector3(0, 90, 0);

            //spawn the viewer for this player
            GameObject spawnedViewer = Instantiate(boardViewer);
            spawnedViewer.transform.eulerAngles = new Vector3(90, 0, 45);

            //rotate the player and items if not the first player
            if (spawns[ID].x != 0)
            {
                VRRigReferences.Singleton.root.eulerAngles = new Vector3(0, -90, 0);
                transform.eulerAngles =new Vector3(0, -90, 0);
                spawnedViewer.transform.eulerAngles = new Vector3(90,135,0);
            }

            //offset the board viewer
            spawnedViewer.transform.position = spawns[ID] + (VRRigReferences.Singleton.root.right*0.5f)+ (VRRigReferences.Singleton.root.forward * 0.2f);
            
            //create the card hand
            GameObject cardHand = Instantiate(cardHandPrefab);
            cardHand.transform.position = spawns[ID] + (VRRigReferences.Singleton.root.forward * 0.1f) + (-VRRigReferences.Singleton.root.up * 0.3f);
            cardHand.transform.eulerAngles = new Vector3(0,VRRigReferences.Singleton.root.eulerAngles.y-90,0);
        
            //store the cardhand within the cardHand prefab
            LocalPlayerManager.Singleton.localPlayerHand = cardHand.transform.Find("CardHand").gameObject;

            //really hacky way to set the board viewer's target but hey, it works
            if (ID == 0)
            {
                spawnedViewer.GetComponent<BoardViewer>().boardView = 1;
            }
            else{
                spawnedViewer.GetComponent<BoardViewer>().boardView = 0;
            }
            spawnedViewer.GetComponent<BoardViewer>().changeTex();

            
            //code for altering skeleton position based on poke interactions
            GameObject[] synthetics = GameObject.FindGameObjectsWithTag("Synthetic");

            // SkeletonProcessAggregator processor = GetComponent<SkeletonProcessAggregator>();
            foreach (GameObject synthetic in synthetics)
            {
              // processor.AddProcessor(synthetic.GetComponentInChildren<SkeletonHandAdjustment>());
            }
            
            
            FindSurface();
        }
        
        
        
    }

    //spawn the anchor for transform asymmetry on the server, and callback for client
    [ServerRpc(RequireOwnership = false)]
    void SpawnAnchorServerRpc(ulong owner)
    {
        GameObject newAnchor = Instantiate(anchorPrefab);
            
        newAnchor.GetComponent<LocalSpaceAnchor>().ID = owner;
            
        newAnchor.GetComponent<NetworkObject>().SpawnWithOwnership(owner);
            
        NetworkObjectReference reference = new NetworkObjectReference(newAnchor);
            
        DeckManager.Singleton.anchors[owner] = newAnchor;
            
        SpawnAnchorClientRpc(owner,reference);
    }

    [ClientRpc] 
    void SpawnAnchorClientRpc(ulong owner, NetworkObjectReference reference) 
    {
        reference.TryGet(out NetworkObject networkObject);
        DeckManager.Singleton.anchors[owner] = networkObject.gameObject;
    }
}

