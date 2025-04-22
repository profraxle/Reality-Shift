using UnityEngine;

public class SurfaceMenu : MonoBehaviour
{
    public Surface surface;

    //different buttons that call functions within the surface or the local player deck
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
        LocalPlayerManager.Singleton.localPlayerDeckObj.SpawnToken();
    }

    public void AddCounter()
    {
        LocalPlayerManager.Singleton.addingToken = true;
    }
}
