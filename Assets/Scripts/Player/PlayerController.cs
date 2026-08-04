using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [Header("Settings")]
    public float moveSpeed = 5f;
    public float frameRate = 0.1f;
    public SpriteRenderer spriteRenderer;

    [Header("Sprites (Do not change)")]
    public Sprite[] front, back, side, diagDown, diagUp;

    [Header("Stair Snapping")]
    public LayerMask snapLayer;
    public float groundCheckDistance = 1f;
    public float yOffset = 0f;

    private PlayerControls controls;
    private Rigidbody rb;
    private Vector2 movementInput;

    private float timer;
    private int currentFrame;
    private Vector2 lastDirection = Vector2.left;
    private bool wasMoving = false;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;
        controls = new PlayerControls();
    }

    void Update()
    {
        movementInput = controls.Player.Move.ReadValue<Vector2>();
        bool isMoving = movementInput.magnitude > 0.1f;

        if (isMoving)
        {
            if (!wasMoving)
            {
                currentFrame = 0;
                timer = 0;
                spriteRenderer.sprite = GetDirectionArray(lastDirection)[0];
            }

            lastDirection = movementInput;
            Animate();
        }
        else
        {
            currentFrame = 0;
            spriteRenderer.sprite = GetDirectionArray(lastDirection)[0];
        }

        wasMoving = isMoving;
    }

    void Animate()
    {
        timer += Time.deltaTime;
        Sprite[] currentSet = GetDirectionArray(lastDirection);

        if (timer >= frameRate)
        {
            timer = 0;
            currentFrame = (currentFrame + 1) % currentSet.Length;
            spriteRenderer.sprite = currentSet[currentFrame];
        }

        float absoluteX = Mathf.Abs(spriteRenderer.transform.localScale.x);
        if (lastDirection.x < 0)
        {
            spriteRenderer.transform.localScale = new Vector3(
                -absoluteX,
                spriteRenderer.transform.localScale.y,
                spriteRenderer.transform.localScale.z
            );
        }
        else if (lastDirection.x > 0)
        {
            spriteRenderer.transform.localScale = new Vector3(
                absoluteX,
                spriteRenderer.transform.localScale.y,
                spriteRenderer.transform.localScale.z
            );
        }
    }


    Sprite[] GetDirectionArray(Vector2 dir)
    {
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        if (angle < 0) angle += 360;

        if (angle > 67.5f && angle < 112.5f) return back;
        if (angle > 247.5f && angle < 292.5f) return front;
        if (angle > 157.5f && angle < 202.5f) return side;
        if (angle > 337.5f || angle < 22.5f) return side;
        if (angle >= 22.5f && angle <= 67.5f) return diagUp;
        if (angle >= 112.5f && angle <= 157.5f) return diagUp;
        if (angle >= 202.5f && angle <= 247.5f) return diagDown;
        return diagDown;
    }

    void FixedUpdate()
    {
        Vector3 moveDir = new Vector3(movementInput.x, 0, movementInput.y);

        Vector3 horizontalMove = moveDir * (moveSpeed * Time.fixedDeltaTime);
        Vector3 targetPosition = rb.position + horizontalMove;
        Vector3 rayOrigin = targetPosition + (Vector3.up * 1f);

        if (Physics.Raycast(rayOrigin, Vector3.down, out RaycastHit hit, groundCheckDistance + 1f, snapLayer))
        {
            targetPosition.y = hit.point.y + yOffset;

            rb.MovePosition(targetPosition);

            rb.linearVelocity = Vector3.zero;
        }
        else
        {
            rb.linearVelocity = new Vector3(moveDir.x * moveSpeed, rb.linearVelocity.y, moveDir.z * moveSpeed);
        }
    }
    void OnEnable() => controls.Player.Enable();
    void OnDisable() {controls.Player.Disable();spriteRenderer.sprite = GetDirectionArray(lastDirection)[0]; }
}