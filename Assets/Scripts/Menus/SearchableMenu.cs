using System;
using System.Collections;
using System.Collections.Generic;
using Oculus.Interaction;
using UnityEngine;

public class SearchableMenu : MonoBehaviour
{

    
    //list of buttons selectable by the player
    public List<GameObject> menuItems;
    
    //the container for the objects to be scrolled through
    public GameObject scrollableContent;
    
    //the pile that spawned this menu
    public CardPile owningPile;

    //the component that handles poke interactions
    public PokeInteractable pokeInteractable;
    
    //the player's hand which pokes the menu
    private PokeInteractor pokeInteractor;
    
    public void SetMenuItemsPositions()
    {
        StartCoroutine(SetMenuItemsPositionsCoroutine());
    }

    IEnumerator SetMenuItemsPositionsCoroutine()
    {
        //update all the positions of the menu items to be in a grid
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

    //add the option to the scrollable content
    public void AddToMenu(GameObject menuItem)
    {
        menuItems.Add(menuItem);
        menuItem.GetComponent<RectTransform>().SetParent(scrollableContent.transform,false);
        menuItem.GetComponent<CardSearchCard>().owningMenu = this;
    }

    //remove the option form the scrollable content
    public void RemoveFromMenu(GameObject menuItem)
    {
        owningPile.searchableList.Remove(menuItem.GetComponent<CardSearchCard>().cardData.name);
        owningPile.DrawSearchedCard(menuItem.GetComponent<CardSearchCard>().cardData.name);
        menuItems.Remove(menuItem);
    }

    //clean up this object and all menu objects
    public void DestroyMenu()
    {
        owningPile.FinishSearching();
        foreach (GameObject menuItem in menuItems)
        {
            Destroy(menuItem);
        }
        

        Destroy(gameObject);
    }
    
}
