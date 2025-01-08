using System.Collections;
using UnityEngine;
using UnityEngine.Tilemaps;

public class SpiralTileActivator : MonoBehaviour
{
    public Tilemap tilemap; // Ссылка на ваш Tilemap
    public float activationInterval = 1f; // Интервал между активацией тайлов

    private void Start()
    {
        if (tilemap == null)
        {
            Debug.LogError("Tilemap не установлен!");
            return;
        }

        DeactivateAllTiles();
        StartCoroutine(ActivateTilesSpirally());
    }

    private void DeactivateAllTiles()
    {
        BoundsInt bounds = tilemap.cellBounds;

        for (int x = 0; x < bounds.size.x; x++)
        {
            for (int y = 0; y < bounds.size.y; y++)
            {
                Vector3Int cellPosition = new Vector3Int(bounds.xMin + x, bounds.yMin + y, 0);
                TileBase tile = tilemap.GetTile(cellPosition);

                if (tile != null)
                {
                    tilemap.SetTileFlags(cellPosition, TileFlags.None); // Снимаем флаги
                    tilemap.SetColor(cellPosition, new Color(1f, 1f, 1f, 0f)); // Прозрачный
                }
            }
        }
    }

    private IEnumerator ActivateTilesSpirally()
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
                tilemap.SetTileFlags(cellPosition, TileFlags.None); // Снимаем флаги
                tilemap.SetColor(cellPosition, new Color(1f, 1f, 1f, 1f)); // Полностью видимый
                Debug.Log($"Tile activated at {cellPosition}");
            }

            yield return new WaitForSeconds(activationInterval);

            // Переход к следующей ячейке
            Vector2Int direction = directions[dirIndex];
            x += direction.x;
            y += direction.y;

            // Проверка выхода за границы и смена направления
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
}




