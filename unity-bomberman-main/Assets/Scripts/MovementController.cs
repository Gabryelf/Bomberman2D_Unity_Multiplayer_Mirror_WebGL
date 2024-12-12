using UnityEngine;
using Mirror;
using System.Collections.Generic;

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
    public List<GameObject> skins; // Список объектов-скинов

    [Header("Sprites")]
    public List<AnimatedSpriteRenderer> spriteRenderersUp;
    public List<AnimatedSpriteRenderer> spriteRenderersDown;
    public List<AnimatedSpriteRenderer> spriteRenderersLeft;
    public List<AnimatedSpriteRenderer> spriteRenderersRight;
    public List<AnimatedSpriteRenderer> spriteRenderersDeath;

    private AnimatedSpriteRenderer activeSpriteRenderer;

    [SyncVar]
    private int playerNumber = -1;

    private bool isMovementEnabled = false;

    private void Awake()
    {
        rigidbody = GetComponent<Rigidbody2D>();
    }

    public override void OnStartServer()
    {
        playerNumber = GetNextPlayerNumber();
    }

    public override void OnStartClient()
    {
        DisableAllSkins();
        ActivateSkin(playerNumber);

        CharacterSelectionManager selectionManager = FindObjectOfType<CharacterSelectionManager>();
        
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
        RpcActivateSkin(playerNumber);

        CharacterSelectionManager selectionManager = FindObjectOfType<CharacterSelectionManager>();
        if (selectionManager != null)
        {
            selectionManager.RpcDisableButton(characterIndex);
        }

        CheckAllPlayersSelected();
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
            DeathSequence();
        }
    }

    private void DeathSequence()
    {
        enabled = false;
        GetComponent<BombController>().enabled = false;
        DisableAllSprites();

        spriteRenderersDeath[playerNumber % spriteRenderersDeath.Count].enabled = true;
        Invoke(nameof(OnDeathSequenceEnded), 1.25f);
    }

    private void OnDeathSequenceEnded()
    {
        gameObject.SetActive(false);
    }

    private int GetNextPlayerNumber()
    {
        return playerNumber == -1 ? Random.Range(0, skins.Count) : playerNumber;
    }

    public void EnablePlayerMovement()
    {
        isMovementEnabled = true;
    }
}



