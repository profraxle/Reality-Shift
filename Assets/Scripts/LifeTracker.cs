using System.Collections;
using TMPro;
using Unity.Netcode;
using UnityEngine;

public class LifeTracker : NetworkBehaviour
{
    private NetworkVariable<int> life;
    [SerializeField]
    private TextMeshProUGUI lifeText;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        StartCoroutine(Initialise());

    }

    IEnumerator Initialise()
    {
        life = new NetworkVariable<int>(0);
        
        life.OnValueChanged += SetLifeText;
        life.Value = 40;
    }
    
    [ServerRpc(RequireOwnership = false)]
    public void IncreaseLifeServerRpc(int amount)
    {
        life.Value += amount;
    }
    [ServerRpc(RequireOwnership = false)]
    public void DecreaseLifeServerRpc(int amount)
    {
        life.Value -= amount;
    }
    [ServerRpc(RequireOwnership = false)]
    public void SetLifeServerRpc(int newValue)
    {
        life.Value = newValue;
    }

    void SetLifeText(int oldValue, int newValue)
    {
        lifeText.text = newValue.ToString();
    }
    
   
}
