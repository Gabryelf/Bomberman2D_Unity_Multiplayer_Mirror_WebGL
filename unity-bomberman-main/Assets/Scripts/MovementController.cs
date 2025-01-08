using UnityEngine;
using Mirror;
using System.Collections.Generic;
using System.Collections;

[RequireComponent(typeof(Rigidbody2D))]
public class MovementController : NetworkBehaviour
{
    private new Rigidbody2D rigidbody;
    private Vector2 direction = Vector2.zero;
    public float speed = 5f;

    [Header("Input")]
    public KeyCode inputUp = KeyCode.W;
    public KeyCode inputDown = KeyCode.S;
    public KeyCode inputLeft = KeyCode.A;
    public KeyCode inputRight = KeyCode.D;

    [Header("Skins")]
    public List<GameObject> skins; // ������ ��������-������

    [Header("Sprites")]
    public List<AnimatedSpriteRenderer> spriteRenderersUp;
    public List<AnimatedSpriteRenderer> spriteRenderersDown;
    public List<AnimatedSpriteRenderer> spriteRenderersLeft;
    public List<AnimatedSpriteRenderer> spriteRenderersRight;
    public List<AnimatedSpriteRenderer> spriteRenderersDeath;

    [SyncVar]
    public int score = 0; // ���� ������

    [Header("Fly Text Settings")]
    public GameObject flyTextPrefab; // ������ ��� ����������� ����������� �����
    private Transform scoreTextTarget;
    // ����, ���� ������ ��������� flyTextPrefab

    private AnimatedSpriteRenderer activeSpriteRenderer;
    private BonusManager bm;
    private Transform respawnPoint;


    [SyncVar]
    private int playerNumber = -1;

    private bool isMovementEnabled = false;
    private Transform initialSpawnPoint;

    private void Awake()
    {
        rigidbody = GetComponent<Rigidbody2D>();

        GameObject targetObject = GameObject.Find("ScoreTextTarget");

        if (targetObject != null)
        {
            scoreTextTarget = targetObject.transform;
        }
        else
        {
            Debug.LogError("ScoreTextTarget not found in the scene!");
        }
    }

    public override void OnStartServer()
    {
        playerNumber = GetNextPlayerNumber();
        AdjustScaleForOrientation();
    }

    private void AdjustScaleForOrientation()
    {
        float scaleFactor = DynamicReferenceResolution.ScaleFactor;
        transform.localScale = Vector3.one * scaleFactor;
    }

    public override void OnStartClient()
    {
        DisableAllSkins();

        // ��������� ��������� ����� ��������
        if (isLocalPlayer && respawnPoint == null)
        {
            respawnPoint = new GameObject($"{name}_RespawnPoint").transform;
            respawnPoint.position = transform.position;
            Debug.Log($"����� �������� �����������: {respawnPoint.position}");
        }

        int index = playerNumber % spriteRenderersDown.Count;
        activeSpriteRenderer = spriteRenderersDown[index];
        activeSpriteRenderer.enabled = true;
    }


    private void Update()
    {

        if (!isLocalPlayer || !isMovementEnabled) return;

        HandleInput();
    }

    private void FixedUpdate()
    {
        Vector2 position = rigidbody.position;
        Vector2 translation = direction * speed * Time.fixedDeltaTime;

        rigidbody.MovePosition(position + translation);
    }

    [Command]
    public void CmdAddScore(int points)
    {
        score += points;
        BonusManager.Instance.UpdateScoreUI();
        RpcDisplayFlyText(points);
        Debug.Log($"Player {playerNumber} scored {points} points. Total Score: {score}");
    }

    // ����������� flyTextPrefab �� �������
    [ClientRpc]
    private void RpcDisplayFlyText(int points)
    {
        if (flyTextPrefab != null && scoreTextTarget != null)
        {
            GameObject flyText = Instantiate(flyTextPrefab, transform.position, Quaternion.identity);
            flyText.GetComponent<TextMesh>().text = $"+{points}";

            StartCoroutine(MoveFlyTextToTarget(flyText));
        }
    }

    private IEnumerator MoveFlyTextToTarget(GameObject flyText)
    {
        float duration = 1f;
        float elapsed = 0f;
        Vector3 startPosition = flyText.transform.position;
        Vector3 targetPosition = scoreTextTarget.position;

        while (elapsed < duration)
        {
            flyText.transform.position = Vector3.Lerp(startPosition, targetPosition, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }

        Destroy(flyText);
    }


    private void HandleInput()
    {
        if (Input.GetKey(inputUp))
        {
            SetDirection(Vector2.up, spriteRenderersUp[playerNumber % spriteRenderersUp.Count]);
        }
        else if (Input.GetKey(inputDown))
        {
            SetDirection(Vector2.down, spriteRenderersDown[playerNumber % spriteRenderersDown.Count]);
        }
        else if (Input.GetKey(inputLeft))
        {
            SetDirection(Vector2.left, spriteRenderersLeft[playerNumber % spriteRenderersLeft.Count]);
        }
        else if (Input.GetKey(inputRight))
        {
            SetDirection(Vector2.right, spriteRenderersRight[playerNumber % spriteRenderersRight.Count]);
        }
        else
        {
            SetDirection(Vector2.zero, activeSpriteRenderer);
        }
    }

    private void SetDirection(Vector2 newDirection, AnimatedSpriteRenderer spriteRenderer)
    {
        if (direction == newDirection && activeSpriteRenderer == spriteRenderer)
            return;

        direction = newDirection;

        DisableAllSprites();
        spriteRenderer.enabled = true;
        activeSpriteRenderer = spriteRenderer;
        activeSpriteRenderer.idle = direction == Vector2.zero;
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
        skins[index % skins.Count].SetActive(true);
    }

    private void DisableAllSprites()
    {
        int index = playerNumber % spriteRenderersUp.Count;
        spriteRenderersUp[index].enabled = false;
        spriteRenderersDown[index].enabled = false;
        spriteRenderersLeft[index].enabled = false;
        spriteRenderersRight[index].enabled = false;
        spriteRenderersDeath[index].enabled = false;
    }

    [Command]
    public void CmdSelectCharacter(int characterIndex)
    {
        playerNumber = characterIndex % skins.Count;
        Debug.LogError($"{playerNumber}") ;

        RpcActivateSkin(playerNumber);

        GameManager.Instance.SetPlayerSkin(characterIndex);

        CharacterSelectionManager selectionManager = FindObjectOfType<CharacterSelectionManager>();
        if (selectionManager != null)
        {
            selectionManager.RpcDisableButton(characterIndex);
        }

        CheckAllPlayersSelected();
        ActivateSkin(playerNumber);
    }

    [ClientRpc]
    private void RpcActivateSkin(int index)
    {
        DisableAllSkins();
        ActivateSkin(index);
    }

    private void CheckAllPlayersSelected()
    {
        var players = FindObjectsOfType<MovementController>();
        foreach (var player in players)
        {
            if (player.playerNumber == -1)
                return;
        }

        foreach (var player in players)
        {
            player.RpcEnableMovement();
        }
    }

    [ClientRpc]
    private void RpcEnableMovement()
    {
        isMovementEnabled = true;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("Explosion"))
        {
            GameTimer gameTimer = FindObjectOfType<GameTimer>();
            gameTimer.AddOpponentScore(100);
            DeathSequence();
        }
    }

    private void DeathSequence()
    {
        // ��������� ���������� � �������
        enabled = false;
        GetComponent<BombController>().enabled = false;
        DisableAllSprites();

        // �������� �������� ������
        spriteRenderersDeath[playerNumber % spriteRenderersDeath.Count].enabled = true;

        // ��������� ������� ����� 1.25 �������
        Invoke(nameof(OnDeathSequenceEnded), 1.25f);
    }


    private void OnDeathSequenceEnded()
    {
        // ������������ ������ ������
        gameObject.SetActive(false);

        // �������� �������
        RpcRespawnPlayer();
    }


    private int GetNextPlayerNumber()
    {
        return playerNumber == -1 ? Random.Range(0, skins.Count) : playerNumber;
    }

    public void EnablePlayerMovement()
    {
        isMovementEnabled = true;
    }

    public void SetInitialSpawnPoint(Transform spawnPoint)
    {
        initialSpawnPoint = spawnPoint;
    }

    [ClientRpc]
    private void RpcRespawnPlayer()
    {
        if (!isLocalPlayer)
            return;

        if (respawnPoint == null)
        {
            Debug.LogError("����� �������� �� �����������! ���������, ��� ��� ������ � OnStartClient.");
            return;
        }

        // ���������� ������ � ����� ��������
        Debug.Log($"������� ������ � �����: {respawnPoint.position}");
        transform.position = respawnPoint.position;

        // �������� ������ ������
        gameObject.SetActive(true);

        // �������� ����������
        enabled = true;
        EnablePlayerMovement();

        // �������� BombController
        BombController bombController = GetComponent<BombController>();
        if (bombController != null)
        {
            bombController.enabled = true;
        }

        // ���������� �������
        DisableAllSprites();
        activeSpriteRenderer.enabled = true;
    }

    [Server]
    private void RespawnPlayerOnServer()
    {
        transform.position = respawnPoint.position;
        RpcRespawnPlayer();
    }


    private bool IsBot()
    {
        return playerNumber == -1; // ��� ����� ������ �������� ��� ����������� �����
    }

    
}



