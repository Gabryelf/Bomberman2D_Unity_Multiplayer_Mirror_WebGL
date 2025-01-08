using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class BotController : MonoBehaviour
{
    public float moveSpeed = 3f;
    public BombController bombController;
    public float safeDistance = 2f; // Расстояние для отхода после установки бомбы
    public float waitTimeAfterBomb = 2f; // Время ожидания после эвакуации
    public float respawnTime = 1f; // Время до респауна после смерти
    public Transform respawnPoint; // Точка респауна

    private Tilemap destructibleTiles;
    private Tilemap groundTiles;
    private Rigidbody2D rb;
    private Vector3Int currentCell;
    private float timeSinceLastBomb = 0f;

    [Header("Skins")]
    public List<GameObject> skins; // Список объектов-скинов

    public List<AnimatedSpriteRenderer> spriteRenderersUp;
    public List<AnimatedSpriteRenderer> spriteRenderersDown;
    public List<AnimatedSpriteRenderer> spriteRenderersLeft;
    public List<AnimatedSpriteRenderer> spriteRenderersRight;
    public List<AnimatedSpriteRenderer> spriteRenderersDeath;

    private AnimatedSpriteRenderer activeSpriteRenderer;
    public int playerSkinIndex = 0;
    public int botSkinIndex;

    private Transform startPoint;

    private Bounds movementBounds;
    private HashSet<Vector3Int> visitedCells = new HashSet<Vector3Int>();

    private enum BotState { Searching, Moving, PlacingBomb, Evading, Waiting, Dead }
    private BotState currentState = BotState.Searching;

    private readonly Vector3Int[] directions = {
        Vector3Int.up,
        Vector3Int.right,
        Vector3Int.down,
        Vector3Int.left
    };

    private float forcedBombTime;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        bombController = GetComponent<BombController>();
        destructibleTiles = GameObject.FindWithTag("DestructibleTiles").GetComponent<Tilemap>();
        groundTiles = GameObject.FindWithTag("Ground").GetComponent<Tilemap>();
        currentCell = groundTiles.WorldToCell(transform.position);

        GameObject zoneObject = GameObject.Find("zone");
        if (zoneObject != null)
        {
            Collider2D collider = zoneObject.GetComponent<Collider2D>();
            if (collider != null)
            {
                movementBounds = collider.bounds;
            }
        }

        if (startPoint == null)
        {
            startPoint = new GameObject($"{name}_StartPoint").transform;
            startPoint.position = transform.position;
        }

        if (respawnPoint == null)
        {
            respawnPoint = startPoint;
        }

        forcedBombTime = 3.5f;


        botSkinIndex = GameManager.Instance.GetPlayerSkin(); // Используется правильный номер для скина

        ApplySkin(botSkinIndex); // Применение скина для бота
        SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            spriteRenderer.enabled = true;
        }
        else
        {
            Debug.LogError("SpriteRenderer is missing on BotPrefab!");
        }

        if (spriteRenderersDown != null && spriteRenderersDown.Count > 0)
        {
            int validIndex = Mathf.Abs(playerSkinIndex % spriteRenderersUp.Count);
            activeSpriteRenderer = spriteRenderersUp[validIndex];
            activeSpriteRenderer.enabled = true;

        }
        else
        {
            Debug.LogError("spriteRenderersDown is not set or is empty.");
        }

        Debug.Log($"Skin Index: {botSkinIndex}");
        Debug.Log($"Active Sprite Renderer: {activeSpriteRenderer}");
        Debug.Log($"Bot Position: {transform.position}");

        StartCoroutine(BotLogic());
    }




    IEnumerator BotLogic()
    {
        while (true)
        {
            timeSinceLastBomb += Time.deltaTime;

            if (currentState == BotState.Dead)
            {
                yield return null; // Ожидаем респауна
                continue;
            }

            if (timeSinceLastBomb >= forcedBombTime && currentState != BotState.PlacingBomb)
            {
                currentState = BotState.PlacingBomb;
            }

            switch (currentState)
            {
                case BotState.Searching:
                    yield return StartCoroutine(SearchForTarget());
                    break;

                case BotState.Moving:
                    yield return StartCoroutine(MoveToCell());
                    break;

                case BotState.PlacingBomb:
                    PlaceBomb();
                    yield return new WaitForSeconds(1f);
                    currentState = BotState.Evading;
                    break;

                case BotState.Evading:
                    yield return StartCoroutine(EvadeBomb());
                    break;

                case BotState.Waiting:
                    yield return StartCoroutine(WaitAfterBomb());
                    break;
            }

            yield return null;
        }
    }

    void OnTriggerEnter2D(Collider2D collision)
    {

        if (collision.gameObject.layer == LayerMask.NameToLayer("Explosion"))
        {
            MovementController player = FindObjectOfType<MovementController>();
            player.CmdAddScore(200);
            StartCoroutine(HandleDeath());
        }
    }


    IEnumerator SearchForTarget()
    {
        currentCell = groundTiles.WorldToCell(transform.position);
        visitedCells.Add(currentCell);

        // Перемешиваем направления для случайного выбора
        List<Vector3Int> shuffledDirections = new List<Vector3Int>(directions);
        ShuffleDirections(shuffledDirections);

        bool foundTarget = false;

        foreach (var direction in shuffledDirections)
        {
            // Случайное количество шагов в допустимых пределах
            int randomSteps = Random.Range(1, 4); // От 1 до 3 клеток
            Vector3Int nextCell = currentCell + direction * randomSteps;

            if (IsCellWithinBounds(nextCell) && IsCellEmpty(nextCell) && !visitedCells.Contains(nextCell) && !HasCollider(nextCell))
            {
                currentCell = nextCell;
                foundTarget = true;
                currentState = BotState.Moving;
                yield break;
            }
        }

        // Если не нашли подходящую клетку, ставим бомбу
        if (!foundTarget)
        {
            currentState = BotState.PlacingBomb;
        }
    }

    // Метод для перемешивания направлений
    void ShuffleDirections(List<Vector3Int> dirs)
    {
        for (int i = 0; i < dirs.Count; i++)
        {
            Vector3Int temp = dirs[i];
            int randomIndex = Random.Range(i, dirs.Count);
            dirs[i] = dirs[randomIndex];
            dirs[randomIndex] = temp;
        }
    }

    IEnumerator EvadeBomb()
    {
        bool movedToSafeCell = false;

        List<Vector3Int> shuffledDirections = new List<Vector3Int>(directions);
        ShuffleDirections(shuffledDirections);

        foreach (var direction in shuffledDirections)
        {
            // Увеличенное расстояние эвакуации с рандомизацией
            int randomSafeDistance = Random.Range(2, 5); // От 2 до 4 клеток
            Vector3Int evadeCell = currentCell + direction * randomSafeDistance;

            if (IsCellWithinBounds(evadeCell) && IsCellEmpty(evadeCell))
            {
                currentCell = evadeCell;
                yield return StartCoroutine(MoveToCell());
                movedToSafeCell = true;
                break;
            }
        }

        // Если не нашли безопасную клетку, двигаемся случайно
        if (!movedToSafeCell)
        {
            Debug.LogWarning("No safe cell found, moving randomly.");
            currentCell += shuffledDirections[Random.Range(0, shuffledDirections.Count)];
            yield return StartCoroutine(MoveToCell());
        }

        // Добавляем случайное время ожидания после эвакуации
        waitTimeAfterBomb = Random.Range(1.5f, 3f);
        currentState = BotState.Waiting;
    }


    void SetRandomForcedBombTime()
    {
        forcedBombTime = Random.Range(4f, 8f); // Увеличенный диапазон
    }


    IEnumerator WaitAfterBomb()
    {
        float randomWaitTime = Random.Range(1.5f, 3f);

        yield return new WaitForSeconds(randomWaitTime);

        timeSinceLastBomb = 0f;
        SetRandomForcedBombTime();
        currentState = BotState.Searching;

        // Очистка истории посещённых клеток после ожидания
        visitedCells.Clear();
    }



    IEnumerator MoveToCell()
    {
        Vector2 targetPosition = groundTiles.GetCellCenterWorld(currentCell);
        float timer = 0f;
        float maxMoveTime = 2f;

        // Определение направления движения
        Vector2 direction = (targetPosition - rb.position).normalized;

        // Устанавливаем направление
        SetBotDirection(direction);

        while (Vector2.Distance(rb.position, targetPosition) > 0.05f)
        {
            rb.MovePosition(Vector2.MoveTowards(rb.position, targetPosition, moveSpeed * Time.deltaTime));
            timer += Time.deltaTime;

            if (timer >= maxMoveTime)
            {
                currentState = BotState.Searching;
                yield break;
            }

            yield return null;
        }

        // Введение случайной паузы после перемещения
        yield return new WaitForSeconds(Random.Range(0.1f, 0.5f));

        SetBotDirection(Vector2.zero); // Если бот остановился, сбрасываем направление
        currentState = BotState.Searching;
    }



    bool HasCollider(Vector3Int cell)
    {
        Vector3 worldPos = groundTiles.GetCellCenterWorld(cell);
        Collider2D hitCollider = Physics2D.OverlapPoint(worldPos);
        return hitCollider != null;
    }

    IEnumerator HandleDeath()
    {
        currentState = BotState.Dead;

        // Отключаем рендеринг и коллайдеры
        //GetComponent<SpriteRenderer>().enabled = false;
        //GetComponent<Collider2D>().enabled = false;

        yield return new WaitForSeconds(respawnTime);

        Respawn();
    }

    void Respawn()
    {
        transform.position = respawnPoint.position;
        currentCell = groundTiles.WorldToCell(respawnPoint.position);

        // Enable the active skin instead of accessing the SpriteRenderer directly
        if (botSkinIndex >= 0 && botSkinIndex < skins.Count)
        {
            ApplySkin(botSkinIndex);
        }
        else
        {
            Debug.LogWarning("Skin index out of range during respawn.");
        }

        GetComponent<Collider2D>().enabled = true;
        timeSinceLastBomb = 0f;
        visitedCells.Clear();
        currentState = BotState.Searching;
    }


    public void SetRespawnPoint(Transform point)
    {
        respawnPoint = point;
        startPoint = point; // Запоминаем стартовую точку
    }

    void SpawnBot(GameObject botPrefab, Transform spawnPoint)
    {
        GameObject bot = Instantiate(botPrefab, spawnPoint.position, Quaternion.identity);
        BotController botController = bot.GetComponent<BotController>();
        botController.SetRespawnPoint(spawnPoint);
    }


    void PlaceBomb()
    {

        bombController.ServerPlaceBomb();
        timeSinceLastBomb = 0f;
        SetRandomForcedBombTime();
    }

    bool IsCellEmpty(Vector3Int cell)
    {
        return groundTiles.HasTile(cell) && destructibleTiles.GetTile(cell) == null;
    }

    bool IsCellWithinBounds(Vector3Int cell)
    {
        Vector3 cellWorldPos = groundTiles.GetCellCenterWorld(cell);
        return movementBounds.Contains(cellWorldPos);
    }

    private void ApplySkin(int skinIndex)
    {
        DisableAllSkins();
        ActivateSkin(skinIndex);
    }

    private void DisableAllSkins()
    {
        foreach (var skin in skins)
        {
            skin.SetActive(false);
        }
    }

    private void ActivateSkin(int index)
    {
        skins[index].SetActive(true);
    }

    private void DisableAllBotSprites()
    {
        foreach (var sprite in spriteRenderersUp)
        {
            sprite.enabled = false;
        }
        foreach (var sprite in spriteRenderersDown)
        {
            sprite.enabled = false;
        }
        foreach (var sprite in spriteRenderersLeft)
        {
            sprite.enabled = false;
        }
        foreach (var sprite in spriteRenderersRight)
        {
            sprite.enabled = false;
        }
        foreach (var sprite in spriteRenderersDeath)
        {
            sprite.enabled = false;
        }
    }

    private void SetBotDirection(Vector2 newDirection)
    {
        ApplySkin(botSkinIndex);
        if (spriteRenderersUp == null || spriteRenderersDown == null ||
            spriteRenderersLeft == null || spriteRenderersRight == null)
        {
            Debug.LogError("Один или несколько списков рендеров не настроены.");
            return;
        }

        if (playerSkinIndex < 0)
        {
            Debug.LogError("playerSkinIndex имеет отрицательное значение.");
            return;
        }

        int validIndex = playerSkinIndex % spriteRenderersUp.Count;

        // Определение направления движения
        if (newDirection == Vector2.up && spriteRenderersUp.Count > 0)
        {
            activeSpriteRenderer = spriteRenderersUp[validIndex];
        }
        else if (newDirection == Vector2.down && spriteRenderersDown.Count > 0)
        {
            activeSpriteRenderer = spriteRenderersDown[validIndex];
        }
        else if (newDirection == Vector2.left && spriteRenderersLeft.Count > 0)
        {
            activeSpriteRenderer = spriteRenderersLeft[validIndex];
        }
        else if (newDirection == Vector2.right && spriteRenderersRight.Count > 0)
        {
            activeSpriteRenderer = spriteRenderersRight[validIndex];
        }
        else
        {
            Debug.LogError($"Невозможно определить направление бота. newDirection: {newDirection}, playerSkinIndex: {playerSkinIndex}");
            return;
        }

        // Отключаем все спрайты и активируем нужный
        DisableAllBotSprites();
        activeSpriteRenderer.enabled = true;
    }



}














