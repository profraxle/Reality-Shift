using UnityEngine;

public class SurfaceMenu : MonoBehaviour
{
    public Surface surface;

    public void Start()
    {
        surface = DeckManager.Singleton.surface.GetComponent<Surface>();
    }
    
    public void UntapAll()
    {
        surface.UntapAllCards();
    }

    public void AlignToSurface()
    {
        surface.StartAligning();
    }

    public void SpawnToken()
    {
        LocalPlayerManager.Singleton.localPlayerDeckObj.SpawnTokenServerRpc();
    }

    public void AddCounter()
    {
        LocalPlayerManager.Singleton.addingToken = true;
    }
}
