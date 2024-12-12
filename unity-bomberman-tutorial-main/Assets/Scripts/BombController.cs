using System.Collections;
using UnityEngine;
using Mirror;
using UnityEngine.UI;
using UnityEngine.Tilemaps;

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

    [Header("UI")]
    private Image cooldownImage;

    private void Start()
    {
        destructibleTiles = FindObjectOfType<Tilemap>();
        cooldownImage = FindObjectOfType<Image>();
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
        Vector2 position = transform.position;
        position.x = Mathf.Round(position.x);
        position.y = Mathf.Round(position.y);

        GameObject bomb = Instantiate(bombPrefab, position, Quaternion.identity);
        NetworkServer.Spawn(bomb);

        bombCooldownRemaining = bombCooldownTime;

        StartCoroutine(BombTimer(bomb, position));
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
        position += direction;

        if (Physics2D.OverlapBox(position, Vector2.one / 2f, 0f, explosionLayerMask))
        {
            ClearDestructible(position);
            return;
        }

        Explosion explosion = Instantiate(explosionPrefab, position, Quaternion.identity);
        explosion.DestroyAfter(explosionDuration);
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
    }

    public void AddBomb()
    {
        // Убираем добавление новых бомб, так как они теперь имеют перезарядку
    }
}




