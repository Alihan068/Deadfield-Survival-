using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{

    PlayerStatsManager playerStatsManager;
    Rigidbody2D rb2d;
    Animator animator;

    [SerializeField] float moveSpeed = 5.0f;
    [SerializeField] float dashPower = 10f;

    Vector3 moveInput;
    bool isAlive = true;
    void Start()
    {
        playerStatsManager = GetComponent<PlayerStatsManager>();
        rb2d = GetComponent<Rigidbody2D>();
    }
    void OnMove(InputValue value) {
        if (!isAlive) return;
        moveInput = value.Get<Vector2>();
        Debug.Log(moveInput);
    }
    void OnDash(InputValue value) {
        Dash();
    }

    // Update is called once per frame
    void Update()
    {
        Walk();
    }

    void Walk() {
        //if (!canMove) return;       

        Vector2 vector = rb2d.linearVelocity;
        vector.x = moveInput.x * moveSpeed;
        vector.y = moveInput.y * moveSpeed;
        rb2d.linearVelocity = vector;

        if (animator) {
            bool hasHorizontal = Mathf.Abs(vector.x) > Mathf.Epsilon;
            animator.SetBool("isWalking", hasHorizontal);
        }
    }
    void Dash() {
        rb2d.AddForce(moveInput * dashPower, ForceMode2D.Impulse);
    }
}
