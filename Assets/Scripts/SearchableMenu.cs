using System;
using System.Collections;
using System.Collections.Generic;
using Oculus.Interaction;
using UnityEngine;

public class SearchableMenu : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    
    public List<GameObject> menuItems;
    public GameObject scrollableContent;
    public CardPile owningPile;

    public PokeInteractable pokeInteractable;
    private PokeInteractor pokeInteractor;
    
    public void SetMenuItemsPositions()
    {
        
        StartCoroutine(SetMenuItemsPositionsCoroutine());
    }

    IEnumerator SetMenuItemsPositionsCoroutine()
    {
        int xAlong = 0;
        int yAlong = 0;
        for (int i = 0; i < menuItems.Count; i++)
        {
            menuItems[i].transform.localPosition = new Vector3(120+220*xAlong,-170 +yAlong*-307,0);

            xAlong++;
            if (xAlong > 4)
            {
                xAlong = 0;
                yAlong++;
            }
        }
        yield return null;
    }

    public void AddToMenu(GameObject menuItem)
    {
        menuItems.Add(menuItem);
        menuItem.GetComponent<RectTransform>().SetParent(scrollableContent.transform,false);
        menuItem.GetComponent<CardSearchCard>().owningMenu = this;
    }

    public void RemoveFromMenu(GameObject menuItem)
    {
        owningPile.searchableList.Remove(menuItem.GetComponent<CardSearchCard>().cardData.name);
        owningPile.DrawSearchedCard(menuItem.GetComponent<CardSearchCard>().cardData.name);
        menuItems.Remove(menuItem);
    }

    public void DestroyMenu()
    {
        owningPile.FinishSearching();
        foreach (GameObject menuItem in menuItems)
        {
            Destroy(menuItem);
        }

        foreach (PokeInteractor interactor in pokeInteractable.SelectingInteractors)
        {
            pokeInteractor = interactor;
            interactor.enabled = false;
        }

        Destroy(gameObject);
    }

    public void OnDestroy()
    {
        pokeInteractor.enabled = true;
    }
}
