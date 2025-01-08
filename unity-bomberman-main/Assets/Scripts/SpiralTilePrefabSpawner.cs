using System.Collections;
using UnityEngine;
using UnityEngine.Tilemaps;

public class SpiralTilePrefabSpawner : MonoBehaviour
{
    public Tilemap tilemap; // Ссылка на Tilemap
    public GameObject prefabToSpawn; // Префаб, который будет спавниться
    public float spawnHeightOffset = 2f; // Высота над ячейкой, с которой начнется движение
    public float moveSpeed = 2f; // Скорость движения префаба к ячейке
    public float spawnInterval = 0.5f; // Интервал между спавном префабов

    private void Start()
    {
        
    }

    public void SpawnStart()
    {
        StartCoroutine(SpawnPrefabsSpirally());
    }

    private IEnumerator SpawnPrefabsSpirally()
    {
        BoundsInt bounds = tilemap.cellBounds;

        int left = 0;
        int right = bounds.size.x - 1;
        int top = 0;
        int bottom = bounds.size.y - 1;

        int x = 0, y = 0;
        Vector2Int[] directions = { Vector2Int.right, Vector2Int.down, Vector2Int.left, Vector2Int.up };
        int dirIndex = 0;

        while (left <= right && top <= bottom)
        {
            Vector3Int cellPosition = new Vector3Int(bounds.xMin + x, bounds.yMax - y, 0);
            TileBase tile = tilemap.GetTile(cellPosition);

            if (tile != null)
            {
                // Спавним префаб и начинаем его движение к ячейке
                Vector3 worldPosition = tilemap.GetCellCenterWorld(cellPosition);
                Vector3 spawnPosition = worldPosition + Vector3.up * spawnHeightOffset;

                GameObject spawnedPrefab = Instantiate(prefabToSpawn, spawnPosition, Quaternion.identity);
                StartCoroutine(MovePrefabToPosition(spawnedPrefab.transform, worldPosition));

                yield return new WaitForSeconds(spawnInterval); // Ждем перед спавном следующего префаба
            }

            // Проверяем следующую позицию
            Vector2Int direction = directions[dirIndex];
            x += direction.x;
            y += direction.y;

            if (dirIndex == 0 && x > right)
            {
                dirIndex = 1; top++; x--; y++;
            }
            else if (dirIndex == 1 && y > bottom)
            {
                dirIndex = 2; right--; x--; y--;
            }
            else if (dirIndex == 2 && x < left)
            {
                dirIndex = 3; bottom--; x++; y--;
            }
            else if (dirIndex == 3 && y < top)
            {
                dirIndex = 0; left++; x++; y++;
            }
        }
    }

    private IEnumerator MovePrefabToPosition(Transform prefabTransform, Vector3 targetPosition)
    {
        while (Vector3.Distance(prefabTransform.position, targetPosition) > 0.01f)
        {
            prefabTransform.position = Vector3.MoveTowards(prefabTransform.position, targetPosition, moveSpeed * Time.deltaTime);
            yield return null; // Ждем следующий кадр
        }

        prefabTransform.position = targetPosition; // Устанавливаем точную позицию в конце
    }
}

