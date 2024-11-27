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

            if (spawns[ID].x != 0)
            {
                VRRigReferences.Singleton.root.eulerAngles = new Vector3(0, -90, 0);
            }

            if (NetworkManager.Singleton.IsServer)
            {
                DeckManager.Singleton.decks[ID].SpawnFirstCard();

            }
            else
            {
                DeckManager.Singleton.decks[ID].SpawnFirstCardServerRpc();
            }

            

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

