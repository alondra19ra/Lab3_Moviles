using UnityEngine;
using Unity.Netcode;

public class PlayerColor : NetworkBehaviour
{
    // Rosado y morado.
    private static readonly Color rosado = new Color(1f, 0.41f, 0.71f);
    private static readonly Color morado = new Color(0.58f, 0.2f, 0.8f);

    // Color del jugador (lo decide el servidor, lo ven todos).
    private NetworkVariable<Color> playerColor =
        new NetworkVariable<Color>(
            Color.white,
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Server
        );

    public override void OnNetworkSpawn()
    {
        playerColor.OnValueChanged += OnColorChanged;

        // El servidor elige el color según el ID del dueño:
        // ID par (0, 2, 4...) = rosado | ID impar (1, 3, 5...) = morado
        if (IsServer)
        {
            playerColor.Value =
                (OwnerClientId % 2 == 0) ? rosado : morado;
        }

        // Aplicamos el color actual (también para quien entra tarde).
        ApplyColor();
    }

    public override void OnNetworkDespawn()
    {
        playerColor.OnValueChanged -= OnColorChanged;
    }

    private void OnColorChanged(Color previous, Color current)
    {
        ApplyColor();
    }

    private void ApplyColor()
    {
        Renderer renderer = GetComponentInChildren<Renderer>();

        if (renderer == null)
            return;

        Material mat = renderer.material;

        // URP usa _BaseColor; Built-in usa _Color.
        if (mat.HasProperty("_BaseColor"))
            mat.SetColor("_BaseColor", playerColor.Value);

        if (mat.HasProperty("_Color"))
            mat.SetColor("_Color", playerColor.Value);
    }
}