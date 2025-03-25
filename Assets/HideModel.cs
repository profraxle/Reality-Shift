using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;

public class HideModel : MonoBehaviour
{
    [SerializeField]
    private SkinnedMeshRenderer mesh1, mesh2;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (transform.parent.gameObject.name != "RemoteCharacter")
        {
            mesh1.gameObject.layer = LayerMask.NameToLayer("HiddenMesh");
            mesh2.gameObject.layer = LayerMask.NameToLayer("HiddenMesh");
        }

        transform.parent.AddComponent<AttachToAnchor>();
        transform.parent.GetComponent<AttachToAnchor>().AddToAnchor();

    }
    
}
