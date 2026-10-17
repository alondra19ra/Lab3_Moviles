using Unity.Netcode;
using UnityEngine;

public class GameManager : NetworkBehaviour
{
    private static GameManager instance;

    [SerializeField] private Transform playerPrefab;


    
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    void Start()
    {
        
    }
    public override void OnNetworkSpawn()
    {
        print(NetworkManager.Singleton.LocalClientId);
        InstancePlayerRpc(NetworkManager.Singleton.LocalClientId);
    }
    [Rpc(SendTo.Server)]
    public void InstancePlayerRpc(ulong ownerID)
    {
        Transform player = Instantiate(playerPrefab);
        // player.GetComponent<NetworkObject>().Spawn(true);
        player.GetComponent<NetworkObject>().SpawnWithOwnership(ownerID, true);
    }

    void Update()
    {
        
    }
    public static GameManager Instance => instance;
}
