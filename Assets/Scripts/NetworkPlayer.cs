using UnityEngine;
using Unity.Netcode;

public class NetworkPlayer : NetworkBehaviour
{

    public Transform root;
    public Transform head;
    public Transform leftHand;
    public Transform rightHand;

    public Renderer[] meshToDisable;

    public ulong ID;

    [SerializeField]
    public Vector3[] spawns;

    [SerializeField]
    GameObject boardViewer;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        if (IsOwner)
        {
            foreach (Renderer r in meshToDisable)
            {
                r.enabled = false;
            }

            ID = NetworkManager.Singleton.LocalClientId;

            VRRigReferences.Singleton.root.position = spawns[ID];

            GameObject spawnedViewer = Instantiate(boardViewer);

            if (spawns[ID].x != 0)
            {
                VRRigReferences.Singleton.root.eulerAngles = new Vector3(0, -90, 0);
                spawnedViewer.transform.eulerAngles = new Vector3(boardViewer.transform.eulerAngles.x, 90, 315);
            }

            if (NetworkManager.Singleton.IsServer)
            {
                DeckManager.Singleton.decks[ID].SpawnFirstCard();

            }
            else
            {
                DeckManager.Singleton.decks[ID].SpawnFirstCardServerRpc();
            }

            
            spawnedViewer.transform.position = spawns[ID] + (VRRigReferences.Singleton.root.right*0.5f)+ (VRRigReferences.Singleton.root.forward * 0.2f);

            //STINKIEST OF ALL HACKS LOOK AWAY
            if (ID == 0)
            {
                spawnedViewer.GetComponent<BoardViewer>().boardView = 1;
            }
            else{
                spawnedViewer.GetComponent<BoardViewer>().boardView = 0;
            }
            spawnedViewer.GetComponent<BoardViewer>().changeTex();
        }

        
    }
    

        // Update is called once per frame
        void Update()
        {
            if (IsOwner)
            {
                root.position = VRRigReferences.Singleton.root.position;
                root.rotation = VRRigReferences.Singleton.root.rotation;

                head.position = VRRigReferences.Singleton.head.position;
                head.rotation = VRRigReferences.Singleton.head.rotation;

                leftHand.position = VRRigReferences.Singleton.leftHand.position;
                leftHand.rotation = VRRigReferences.Singleton.leftHand.rotation;

                rightHand.position = VRRigReferences.Singleton.rightHand.position;
                rightHand.rotation = VRRigReferences.Singleton.rightHand.rotation;
            }
        }
}

