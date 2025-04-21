using TMPro;
using UnityEngine;
using UnityEngine.UI;


//panel to be spawned for the user to select to choose deck
public class DeckChoicePanel : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created

    //obejcts for the text and image to be displayed
    [SerializeField]
    GameObject text;
    [SerializeField]
    GameObject image;

	//the parent DeckSelector object
    public DeckSelector selector;

    //the data of the deck this object represents
    public DeckData deckData;
    
    //update the text on the panel with the deck name
    public void UpdatePanel()
    {
        text.GetComponent<TextMeshProUGUI>().text = deckData.deckName;
    }
    
    //set image to a passed in sprite
    public void SetImage(Sprite sprite)
    {
      image.GetComponent<Image>().sprite = sprite;
    }

    //set this to the selected deck and display the contents of the deck to the user
    public void Selected()
    {
        selector.SetSelectedDeck(deckData);
        selector.UpdateDeckText();
    }
}
