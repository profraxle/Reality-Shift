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
        counter.OnValueChanged += SetCounterText;
        SetCounterText(counter.Value,counter.Value);
    }

    public void IncrementCounter()
    {
        counter.Value++;
    }

    public void DecrementCounter()
    {
        counter.Value--;
    }
 
    void SetCounterText(int oldValue, int newValue)
    {
        counterText.text = newValue.ToString();
    }

    public void DestroyCounter()
    {
        Destroy(gameObject);
    }
}
