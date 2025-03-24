using Unity.Netcode;
using UnityEngine;

public class SpawnNetworkPlayer : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created

    [SerializeField]
    GameObject playerPrefab;

    public void SpawnPlayers()
    {
        if (NetworkManager.Singleton.IsServer)
        {
            for (int i = 0; i < NetworkManager.Singleton.ConnectedClients.Count - 1; i++)
            {
                SpawnPlayerServerRpc((ulong)i);
            }
        }   
    }

    [ServerRpc(RequireOwnership = false)]
    void SpawnPlayerServerRpc(ulong playerId)
    {
        GameObject prefab = Instantiate(playerPrefab);
        
        prefab.GetComponent<NetworkObject>().SpawnWithOwnership(playerId,false);
    }

}
