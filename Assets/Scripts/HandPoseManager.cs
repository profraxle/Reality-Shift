using UnityEngine;

public class HandPoseManager : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created

   static public HandPoseManager Singleton;

    bool rightDrag;

    private void Awake()
    {
        Singleton = this;
    }

    public void StartRightDrag()
    {
        rightDrag = true;
    }

    public void StopRightDrag()
    {
        rightDrag = false;
    }

    public bool getRightDrag()
    {
        return rightDrag;
    }
}
