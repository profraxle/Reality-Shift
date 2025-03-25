using System.Collections;
using TMPro;
using Unity.Netcode;
using UnityEngine;

public class LifeTracker : NetworkBehaviour
{
    private NetworkVariable<int> life  = new NetworkVariable<int>(20);
    [SerializeField]
    private TextMeshProUGUI lifeText;
    [SerializeField]
    private TextMeshProUGUI lifeText2;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
        life.OnValueChanged += SetLifeText;
        SetLifeText(life.Value,life.Value);
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
        lifeText2.text = newValue.ToString();
    }
    
   
}
