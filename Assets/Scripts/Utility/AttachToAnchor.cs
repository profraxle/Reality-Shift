using System.Collections;
using Meta.XR.MultiplayerBlocks.NGO;
using Unity.Netcode;
using UnityEngine;

public class AttachToAnchor : NetworkBehaviour
{

    public void AddToAnchor()
    {
        //calls a coroutine for the anchor getting to allow for a delay
        StartCoroutine(AnchorDelay());
    }
    
    IEnumerator AnchorDelay()
    {
        yield return new WaitForSeconds(1f);

        //wait for a second and get the required anchor object from the manager, then ensure object only replicates in local space
        transform.SetParent(DeckManager.Singleton.anchors[NetworkManager.Singleton.LocalClientId].transform);
        GetComponent<ClientNetworkTransform>().InLocalSpace = true;
    }

}
