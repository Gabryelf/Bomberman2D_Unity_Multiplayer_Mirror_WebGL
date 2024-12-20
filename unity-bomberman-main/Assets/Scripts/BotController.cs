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

    private Tilemap destructibleTiles;
    private Tilemap groundTiles;
    private Rigidbody2D rb;
    private Vector3Int currentCell;
    private float timeSinceLastBomb = 0f;

    private Bounds movementBounds;
    private HashSet<Vector3Int> visitedCells = new HashSet<Vector3Int>();  // Теперь HashSet доступен

    private enum BotState { Searching, Moving, PlacingBomb, Evading, Waiting }
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

        // Устанавливаем время до первой бомбы на 1.5 секунды
        forcedBombTime = 3.5f;
       
        StartCoroutine(BotLogic());
    }


    IEnumerator BotLogic()
    {
        while (true)
        {

            timeSinceLastBomb += Time.deltaTime;
            
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
            Vector3Int nextCell = currentCell + direction;

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
            Vector3Int evadeCell = currentCell + direction * (int)safeDistance;
            if (IsCellWithinBounds(evadeCell) && IsCellEmpty(evadeCell))
            {
                currentCell = evadeCell;
                yield return StartCoroutine(MoveToCell());
                movedToSafeCell = true;
                break;
            }
        }

        //if (!movedToSafeCell)
        //{
        //    Debug.LogWarning("No safe cell found, moving randomly.");
        //    currentCell += shuffledDirections[Random.Range(0, shuffledDirections.Count)];
        //    yield return StartCoroutine(MoveToCell());
        //}

        // Добавляем случайное время ожидания после эвакуации
        waitTimeAfterBomb = Random.Range(1.5f, 3f);
        currentState = BotState.Waiting;
    }

    void SetRandomForcedBombTime()
    {
        forcedBombTime = Random.Range(3f, 6f); // Случайное время для следующей принудительной установки бомбы

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
        float maxMoveTime = 2f; // Максимальное время на передвижение

        while (Vector2.Distance(rb.position, targetPosition) > 0.05f)
        {
            rb.MovePosition(Vector2.MoveTowards(rb.position, targetPosition, moveSpeed * Time.deltaTime));
            timer += Time.deltaTime;

            // Если бот не может добраться до цели за 2 секунды, выбираем новую клетку
            if (timer >= maxMoveTime)
            {
                
                currentState = BotState.Searching;
                yield break;
            }

            yield return null;
        }

        // После достижения цели возвращаемся к поиску
        rb.MovePosition(targetPosition);
        currentState = BotState.Searching;
    }


    bool HasCollider(Vector3Int cell)
    {
        Vector3 worldPos = groundTiles.GetCellCenterWorld(cell);
        Collider2D hitCollider = Physics2D.OverlapPoint(worldPos);
        return hitCollider != null;
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
}















