using UnityEngine;
using Unity.Netcode;
using Unity.Netcode.Components;

public class SimplePlayer : NetworkBehaviour
{
    public float speed;
    void Start()
    {
    }
    void Update()
    {
        if(!IsOwner) return;

        if (Input.GetAxisRaw("Horizontal") == 0 && Input.GetAxisRaw("Vertical") == 0) return;

        float x = Input.GetAxisRaw("Horizontal") * speed * Time.deltaTime;
        float y = Input.GetAxisRaw("Vertical") * speed * Time.deltaTime;
        ValideteMoventRpc(x, y);
    }
    [Rpc(SendTo.Server)]
    public void ValideteMoventRpc(float x,float y)
    {
        transform.position += new Vector3(x, 0, y);
    }
}
