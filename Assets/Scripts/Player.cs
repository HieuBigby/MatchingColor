using DG.Tweening;
using UnityEngine;
using UnityEngine.UIElements;

public class Player : MonoBehaviour
{
    public int ColorId;
    public float speed;

    private Vector3 originalPosition;
    private bool isDragging = false;
    private bool isMoving = false;
    private Rigidbody2D rb;
    Tween moveTween;

    public bool IsDragging => isDragging;

    public Vector3 Position
    {
        get => transform.position;
        set
        {
            transform.position = value;
            OriginalPosition = value;
        }
    }
    public Vector3 OriginalPosition
    {
        get => originalPosition;
        set
        {
            originalPosition = value;
        }
    }

    private SpriteRenderer spriteRenderer;
    public Vector3 Size
    {
        get
        {
            if(spriteRenderer == null)
            {
                spriteRenderer = GetComponent<SpriteRenderer>();
            }
            return spriteRenderer.bounds.size;
        }
    }

    public int SpriteOrder
    {
        get => spriteRenderer.sortingOrder;
        set
        {
            if(spriteRenderer)
            {
                spriteRenderer.sortingOrder = value;
            }
        }
    }

    public void SetColor(int id)
    {
        ColorId = id;
        spriteRenderer.color = GameplayManager.Instance.Colors[ColorId];
    }

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        SetColor(ColorId);
        rb = GetComponent<Rigidbody2D>();
        SpriteOrder = 0;

        // Store original position
        originalPosition = transform.position;

        // Set drag to zero for infinite movement
        if (rb != null)
        {
            rb.drag = 0f;
            rb.angularDrag = 0f;
        }
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0) && !isMoving)
        {
            Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector2 mousePos2D = new Vector2(mousePos.x, mousePos.y);
            RaycastHit2D hit = Physics2D.Raycast(mousePos2D, Vector2.zero);

            if (hit.collider && hit.collider.gameObject == gameObject)
            {
                SpriteOrder = 10;
                moveTween?.Complete();

                // Start dragging
                isDragging = true;
            }
        }

        if (Input.GetMouseButton(0) && isDragging)
        {
            // Update position during drag
            Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            transform.position = new Vector3(mousePos.x, mousePos.y, transform.position.z);

            // Check for overlapping blocks and swap positions
            Player overlappedBlock = GameplayManager.Instance.GetOverlappedBlock(this);
            if (overlappedBlock != null)
            {
                // Swap the blocks in the list
                GameplayManager.Instance.SwapBlocks(this, overlappedBlock);
            }
        }

        if (Input.GetMouseButtonUp(0) && isDragging)
        {
            // Tween back to original position
            SpriteOrder = 0;
            moveTween = transform.DOMove(originalPosition, 0.2f).SetEase(Ease.OutSine);
            if (rb != null)
            {
                rb.velocity = Vector2.zero;
            }

            // Reset dragging
            isDragging = false;
        }
    }

    public void StopMoving()
    {
        if (rb != null)
        {
            rb.velocity = Vector2.zero;
        }
        moveTween?.Kill(); // Stop any ongoing tweens
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Obstacle"))
        {
            if(isMoving)
            {
                return;
            }

            Vector3 dragDirection = new Vector2(0f, 1f);

            // Apply movement to the block's Rigidbody2D if it has one
            if (rb != null)
            {
                rb.velocity = new Vector2(dragDirection.x, dragDirection.y) * speed;
            }

            // Reset dragging
            isDragging = false;
            GameplayManager.Instance.UnregisterBlock(this);
            isMoving = true;
        }

        if(isMoving)
        {
            if(collision.gameObject.CompareTag("Wall"))
            {
                StopMoving();
                GameplayManager.Instance.GameEnded();
            }
        }
    }
}
