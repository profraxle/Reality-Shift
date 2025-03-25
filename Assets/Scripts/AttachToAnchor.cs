using System.Collections;
using Meta.XR.MultiplayerBlocks.NGO;
using Unity.Netcode;
using UnityEngine;

public class AttachToAnchor : NetworkBehaviour
{

    public void AddToAnchor()
    {
        
        StartCoroutine(AnchorDelay());
    }

    IEnumerator AnchorDelay()
    {
        yield return new WaitForSeconds(1f);

        //if (IsServer)
       // {
            transform.SetParent(DeckManager.Singleton.anchors[NetworkManager.Singleton.LocalClientId].transform);
            GetComponent<ClientNetworkTransform>().InLocalSpace = true;
       // }

    }

}
