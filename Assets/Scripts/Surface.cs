using System.Collections.Generic;
using UnityEngine;

public class Surface : MonoBehaviour
{
    
    List<GameObject> cardsOnSurface = new List<GameObject>();
    float surfOffsetAmount = 0.0001f;
    
    public void AddCardToSurface(GameObject card)
    {
        cardsOnSurface.Add(card);
        SetCardOffsets();
    }

    public void RemoveCardFromSurface(GameObject card)
    {
       cardsOnSurface.Remove(card);
       card.GetComponent<Card>().surfOffset = -1f;
       SetCardOffsets();
    }

    public void SetCardOffsets()
    {
        for (int i = 0; i < cardsOnSurface.Count; i++)
        {
            cardsOnSurface[i].GetComponent<Card>().surfOffset = surfOffsetAmount * i;
        }
    }
}
