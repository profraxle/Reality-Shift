using UnityEngine;

public class VRRigReferences : MonoBehaviour
{
    //a manager to pass the positions of the hands, head and root of the VR players
    public static VRRigReferences Singleton;

    public Transform root;
    public Transform head;
    public Transform leftHand;
    public Transform rightHand;

    private void Awake()
    {       
        Singleton = this;
    }

}
