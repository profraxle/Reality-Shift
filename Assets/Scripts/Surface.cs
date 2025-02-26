using UnityEngine;

public class Surface : MonoBehaviour
{
    private float cardsOnSurface;

    private void Start()
    {
        cardsOnSurface = 0;
    }
    
    public void IncreaseCardsOnSurface()
    {
        cardsOnSurface++;
    }

    public void DecreaseCardsOnSurface()
    {
        cardsOnSurface--;
    }

    public float GetCardsOnSurface()
    {
        return cardsOnSurface;
    }
}
