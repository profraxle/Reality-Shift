using UnityEngine;

public class BoardViewer : MonoBehaviour
{
    public RenderTexture[] boards;
    public int boardView;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
       
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void changeTex()
    {
        Renderer renderer = GetComponent<Renderer>();
        Material mat = renderer.material;
        mat.SetTexture("_BaseMap", boards[boardView]);
    }
}
