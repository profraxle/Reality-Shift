using Unity.Netcode;
using UnityEngine;

public class LocalSpaceAnchor : NetworkBehaviour
{

    public ulong ID;
    
    
    void Update()
    {
        if (DeckManager.Singleton)
        {
            if (DeckManager.Singleton.surface)
            {
                if (IsOwner)
                {
                    transform.position = DeckManager.Singleton.surface.transform.position;
                    transform.rotation = DeckManager.Singleton.surface.transform.rotation;
                }
                else
                {
                    transform.position = DeckManager.Singleton.surface.transform.position+DeckManager.Singleton.surface.transform.right*0.525f;
                    transform.rotation = Quaternion.Euler(new Vector3(
                        DeckManager.Singleton.surface.transform.rotation.eulerAngles.x,
                        DeckManager.Singleton.surface.transform.rotation.eulerAngles.y+180,
                        DeckManager.Singleton.surface.transform.rotation.eulerAngles.z));
                }
            }
        }
    }
}
