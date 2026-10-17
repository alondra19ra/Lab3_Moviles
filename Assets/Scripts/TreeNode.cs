using UnityEngine;
using Unity.Netcode;
using Unity.Collections;
using TMPro;

public class TreeNode : NetworkBehaviour
{
    // =========================================================
    // COLORES
    // =========================================================

    // Blanco = libre (color inicial).
    private static readonly Color libre = Color.white;

    // Celeste = tiene dueño.
    private static readonly Color celeste = new Color(0.53f, 0.81f, 0.92f);


    // =========================================================
    // ESTADO DEL ÁRBOL
    // =========================================================

    public NetworkVariable<Color> treeColor =
        new NetworkVariable<Color>(
            Color.white,
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Server
        );

    private NetworkVariable<bool> isClaimed =
        new NetworkVariable<bool>(false);

    // Cantidad de cortes que le quedan al árbol.
    public NetworkVariable<int> logs =
        new NetworkVariable<int>(
            3,
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Server
        );

    public NetworkVariable<FixedString32Bytes> ownerName =
        new NetworkVariable<FixedString32Bytes>(
            new FixedString32Bytes("Sin dueño"),
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Server
        );


    // =========================================================
    // TEXTO DEL ÁRBOL
    // =========================================================

    [SerializeField]
    private TextMeshProUGUI treeInfoText;


    // =========================================================
    // CHOQUE CON UN JUGADOR (RECLAMAR)
    // =========================================================
    // Solo el servidor procesa el choque.
    // Cada vez que un jugador choca, el árbol pasa a ser suyo.

    private void OnCollisionEnter(Collision collision)
    {
        TryClaim(collision.gameObject);
    }

    private void OnTriggerEnter(Collider other)
    {
        TryClaim(other.gameObject);
    }

    private void TryClaim(GameObject other)
    {
        if (!IsServer || !IsSpawned)
            return;

        // Buscamos el NetworkObject del jugador que chocó.
        NetworkObject playerObject =
            other.GetComponentInParent<NetworkObject>();

        // Los jugadores se crean con SpawnWithOwnership, así que
        // IsPlayerObject sería false. Los reconocemos por PlayerColor.
        if (playerObject == null ||
            playerObject.GetComponent<PlayerColor>() == null)
            return;

        ulong who = playerObject.OwnerClientId;

        // Si ya es suyo, no hacemos nada.
        if (isClaimed.Value && OwnerClientId == who)
            return;

        isClaimed.Value = true;

        NetworkObject.ChangeOwnership(who);

        treeColor.Value = celeste;

        ownerName.Value =
            new FixedString32Bytes("Player " + who);

        Debug.Log("Árbol reclamado por " + who);
    }


    // =========================================================
    // CLIC EN EL ÁRBOL (CORTAR)
    // =========================================================

    private void OnMouseDown()
    {
        if (!IsSpawned)
            return;

        // Solo el dueño puede cortarlo.
        if (isClaimed.Value && IsOwner)
        {
            ChopTreeRpc();
        }
        else
        {
            Debug.Log("No es tuyo");
        }
    }

    [Rpc(SendTo.Server)]
    private void ChopTreeRpc()
    {
        logs.Value--;

        Debug.Log("Corte. Quedan: " + logs.Value);

        if (logs.Value <= 0)
        {
            NetworkObject.RemoveOwnership();

            isClaimed.Value = false;

            // Vuelve a blanco.
            treeColor.Value = libre;

            ownerName.Value =
                new FixedString32Bytes("Sin dueño");

            logs.Value = 3;

            Debug.Log("Árbol libre");
        }
    }


    // =========================================================
    // CALLBACKS DE CAMBIO
    // =========================================================

    private void OnColorChanged(Color previous, Color current)
    {
        ChangeColor();
    }

    private void OnLogsChanged(int previous, int current)
    {
        UpdateTreeInfo();
    }

    private void OnOwnerNameChanged(
        FixedString32Bytes previous,
        FixedString32Bytes current)
    {
        UpdateTreeInfo();
    }


    // =========================================================
    // RED
    // =========================================================

    public override void OnNetworkSpawn()
    {
        treeColor.OnValueChanged += OnColorChanged;
        logs.OnValueChanged += OnLogsChanged;
        ownerName.OnValueChanged += OnOwnerNameChanged;

        // Estado actual (importante para quien entra tarde).
        ChangeColor();
        UpdateTreeInfo();
    }

    public override void OnNetworkDespawn()
    {
        treeColor.OnValueChanged -= OnColorChanged;
        logs.OnValueChanged -= OnLogsChanged;
        ownerName.OnValueChanged -= OnOwnerNameChanged;
    }


    // =========================================================
    // VISUAL
    // =========================================================

    private void ChangeColor()
    {
        Renderer renderer = GetComponent<Renderer>();

        if (renderer == null)
            return;

        Material mat = renderer.material;

        // URP usa _BaseColor; Built-in usa _Color.
        if (mat.HasProperty("_BaseColor"))
            mat.SetColor("_BaseColor", treeColor.Value);

        if (mat.HasProperty("_Color"))
            mat.SetColor("_Color", treeColor.Value);
    }

    private void UpdateTreeInfo()
    {
        if (treeInfoText == null)
            return;

        // En lugar de "Vida" ahora dice "Cortes".
        treeInfoText.text =
            ownerName.Value.ToString() +
            "\nCortes: " +
            logs.Value;
    }
}