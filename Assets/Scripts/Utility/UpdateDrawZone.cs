using Oculus.Interaction.Input;
using UnityEngine;

public class UpdateDrawZone : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created

    private bool isInDrawZone;

    public bool currInZone;
    public GameObject currCard;

    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "RightHand")
        {
            isInDrawZone = true;

        }

    }

    void OnTriggerExit(Collider other)
    {
        if (other.gameObject.tag == "RightHand")
        {
            isInDrawZone = false;

        }
    }

    public bool GetIsInDrawZone()
    {
        return isInDrawZone;
    }
}
