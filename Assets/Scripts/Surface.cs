using System;
using System.Collections.Generic;
using UnityEngine;

public class Surface : MonoBehaviour
{
    
    List<Card> cardsOnSurface = new List<Card>();
    float surfOffsetAmount = 0.0001f;

    public void LateUpdate()
    {
        //iterate through cards on surface, align them to table if not
        if (cardsOnSurface.Count != 0)
        {
            for (int i = 0; i < cardsOnSurface.Count; i++)
            {
                if (cardsOnSurface[i])
                {
                    if (!cardsOnSurface[i].grabbed)
                    {
                        bool needsUpdate = false;

                        GameObject cardObj = cardsOnSurface[i].gameObject;

                        float yTarget = transform.position.y + (i * surfOffsetAmount);

                        Vector3 posOnTable = cardObj.transform.position;
                        Quaternion rotOnTable = cardObj.transform.rotation;
                        if (cardObj.transform.position.y != yTarget)
                        {
                            posOnTable = new Vector3(cardObj.transform.position.x, yTarget,
                                cardObj.transform.position.z);
                            needsUpdate = true;
                        }

                        if (cardObj.transform.eulerAngles.x != 0 && cardObj.transform.eulerAngles.z != 0)
                        {
                            rotOnTable = Quaternion.Euler(-90, 0, 0);
                            needsUpdate = true;
                        }

                        if (needsUpdate)
                        {
                            cardObj.transform.SetPositionAndRotation(posOnTable, rotOnTable);
                        }
                    }
                }
            }
        }
    }


    public void AddCardToSurface(Card card)
    {
        cardsOnSurface.Add(card);
    }

    public void RemoveCardFromSurface(Card card)
    {
       cardsOnSurface.Remove(card);
    }


}
