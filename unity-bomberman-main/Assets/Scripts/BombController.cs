using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Tilemaps;
using Mirror;

public class BombController : NetworkBehaviour
{
    [Header("Bomb")]
    public KeyCode inputKey = KeyCode.Space;
    public GameObject bombPrefab;
    public float bombFuseTime = 3f;
    public float bombCooldownTime = 3f;
    private float bombCooldownRemaining = 0f;

    [Header("Explosion")]
    public Explosion explosionPrefab;
    public LayerMask explosionLayerMask;
    public float explosionDuration = 1f;
    public int explosionRadius = 1;

    [Header("Destructible")]
    private Tilemap destructibleTiles;
    public Destructible destructiblePrefab;
    public int wallDestructionPoints = 100; // Очки за разрушение стены

    [Header("UI")]
    private Image cooldownImage;

    private void Start()
    {
        destructibleTiles = FindObjectOfType<Tilemap>();
        if (isLocalPlayer)
        {
            cooldownImage = GameObject.FindWithTag("CooldownImage").GetComponent<Image>();
        }
    }

    private void Update()
    {
        if (!isLocalPlayer) return;

        if (bombCooldownRemaining > 0)
        {
            bombCooldownRemaining -= Time.deltaTime;
            cooldownImage.fillAmount = 1 - (bombCooldownRemaining / bombCooldownTime);
        }

        if (bombCooldownRemaining <= 0 && Input.GetKeyDown(inputKey))
        {
            CmdPlaceBomb();
        }
    }

    [Command]
    private void CmdPlaceBomb()
    {
        PlaceBombServer();
    }

    /// <summary>
    /// Метод для размещения бомбы на сервере, который может вызываться ботами.
    /// </summary>
    public void ServerPlaceBomb()
    {
        if (!isServer) return;
        PlaceBombServer();
    }

    private void PlaceBombServer()
    {
        Vector3Int cellPosition = destructibleTiles.WorldToCell(transform.position);
        Vector3 bombPosition = destructibleTiles.GetCellCenterWorld(cellPosition);

        GameObject bomb = Instantiate(bombPrefab, bombPosition, Quaternion.identity);
        if (bomb == null)
        {
            Debug.LogError("Bomb prefab instantiation failed.");
            return;
        }

        NetworkServer.Spawn(bomb);
        bombCooldownRemaining = bombCooldownTime;
        StartCoroutine(BombTimer(bomb, bombPosition));
    }

    private IEnumerator BombTimer(GameObject bomb, Vector2 position)
    {
        yield return new WaitForSeconds(bombFuseTime);
        RpcExplode(position);
        Destroy(bomb);
    }

    [ClientRpc]
    private void RpcExplode(Vector2 position)
    {
        Explode(position);
    }

    private void Explode(Vector2 position)
    {
        ExplodeInDirection(position, Vector2.up);
        ExplodeInDirection(position, Vector2.down);
        ExplodeInDirection(position, Vector2.left);
        ExplodeInDirection(position, Vector2.right);
    }

    private void ExplodeInDirection(Vector2 position, Vector2 direction)
    {
        for (int i = 1; i <= explosionRadius; i++)
        {
            Vector2 currentPosition = position + direction * i;

            if (Physics2D.OverlapBox(currentPosition, Vector2.one / 2f, 0f, explosionLayerMask))
            {
                
                ClearDestructible(currentPosition);
                Explosion explosion = Instantiate(explosionPrefab, currentPosition, Quaternion.identity);
                explosion.DestroyAfter(explosionDuration);

                //break;
            }

            
        }
        
    }

    private void ClearDestructible(Vector2 position)
    {
        CmdClearDestructible(position);
    }

    [Command]
    private void CmdClearDestructible(Vector2 position)
    {
        Vector3Int cell = destructibleTiles.WorldToCell(position);
        TileBase tile = destructibleTiles.GetTile(cell);

        if (tile != null)
        {
            RpcClearTile(cell);
            RpcInstantiateDestructible(position);
        }
    }

    [ClientRpc]
    private void RpcClearTile(Vector3Int cell)
    {
        destructibleTiles.SetTile(cell, null);
    }

    [ClientRpc]
    private void RpcInstantiateDestructible(Vector2 position)
    {
        Instantiate(destructiblePrefab, position, Quaternion.identity);
        MovementController player = GetComponentInParent<MovementController>();
        if (player != null)
        {
            player.CmdAddScore(wallDestructionPoints);
        }
    }

    public bool CanPlaceBomb()
    {
        return bombCooldownRemaining <= 0f;
    }

    public void AddBomb()
    {
        bombCooldownTime -= 0.1f;
        bombFuseTime -= 0.1f;
    }
}









