using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DeckChoicePanel : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created

    [SerializeField]
    GameObject text;
    [SerializeField]
    GameObject image;

    public DeckData deckData;

    void Start()
    {
        
    }

    public void UpdatePanel()
    {
        text.GetComponent<TextMeshProUGUI>().text = deckData.deckName;
    }

    public void SetImage(Sprite sprite)
    {
      image.GetComponent<Image>().sprite = sprite;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
