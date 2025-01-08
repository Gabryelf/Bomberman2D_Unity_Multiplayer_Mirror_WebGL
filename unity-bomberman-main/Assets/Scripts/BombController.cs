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
        bomb.transform.localScale *= DynamicReferenceResolution.ScaleFactor;
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
        // Взрыв в центральной ячейке
        CreateExplosion(position);

        // Взрыв в каждом направлении
        ExplodeInDirection(position, Vector2.up, explosionRadius);
        ExplodeInDirection(position, Vector2.down, explosionRadius);
        ExplodeInDirection(position, Vector2.left, explosionRadius);
        ExplodeInDirection(position, Vector2.right, explosionRadius);
    }

    private void ExplodeInDirection(Vector2 position, Vector2 direction, int length)
    {
        if (length <= 0) return;

        // Учитываем масштаб при расчёте шага
        Vector2 fixedStep = new Vector2(1f, 1f); // Настройте под сетку
        if (DynamicReferenceResolution.ScaleFactor == 0.5f)
        {
            fixedStep = new Vector2(0.5f, 0.5f); // Масштаб шага для другой сетки
        }

        Vector2 nextPosition = position + direction * fixedStep;

        // Проверяем столкновение с коллайдером
        Collider2D hitCollider = Physics2D.OverlapBox(nextPosition, fixedStep, 0f, explosionLayerMask);

        if (hitCollider)
        {
            // Проверяем, если объект имеет тег "Ground" и компонент TilemapCollider2D
            if (hitCollider.CompareTag("Ground") && hitCollider.GetComponent<TilemapCollider2D>() != null)
            {
                return; // Остановить распространение в этом направлении
            }

            // Очищаем разрушаемый объект
            ClearDestructible(nextPosition);
        }

        // Создаём взрыв
        CreateExplosion(nextPosition);

        // Рекурсивный вызов
        ExplodeInDirection(nextPosition, direction, length - 1);
    }



    private void CreateExplosion(Vector2 position)
    {
        Explosion explosion = Instantiate(explosionPrefab, position, Quaternion.identity);

        // Масштабируем взрыв с учетом разрешения
        explosion.transform.localScale = Vector3.one * DynamicReferenceResolution.ScaleFactor;
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
        
        //destructiblePrefab.transform.localScale *= DynamicReferenceResolution.ScaleFactor;
        MovementController player = GetComponentInParent<MovementController>();
        if (player != null && isLocalPlayer)
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









