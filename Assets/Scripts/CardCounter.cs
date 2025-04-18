using System;
using TMPro;
using Unity.Netcode;
using UnityEngine;

public class CardCounter : NetworkBehaviour
{
    public NetworkVariable<int> counter = new NetworkVariable<int>(0);

    [SerializeField]
    private TextMeshProUGUI counterText;

    public void Start()
    {
        //bind the changing of the counter text to when the value of the counter is edited
        counter.OnValueChanged += SetCounterText;
        
        //initialise  the text of the counter
        SetCounterText(counter.Value,counter.Value);
    }
    
    
    //functions to increment and decrement counter value amount
    [ServerRpc(RequireOwnership = false)]
    public void IncrementCounterServerRpc()
    {
        counter.Value++;
    }

    [ServerRpc(RequireOwnership = false)]
    public void DecrementCounterServerRpc()
    {
        counter.Value--;
    }
 
    //sets the visual text of the counter to the new value
    void SetCounterText(int oldValue, int newValue)
    {
        counterText.text = newValue.ToString();
    }
    
    //destroy this counter, callable from a button on the counter
    public void DestroyCounter()
    {
        Destroy(gameObject);
    }
}
