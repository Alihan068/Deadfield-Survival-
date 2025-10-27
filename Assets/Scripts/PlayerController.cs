using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    Rigidbody2D rb2d;
    Animator animator;

    [SerializeField] float moveSpeed = 5.0f; 

    Vector3 moveInput;
    bool isAlive = true;
    void Start()
    {

        rb2d = GetComponent<Rigidbody2D>();
    }
    void OnMove(InputValue value) {
        if (!isAlive) return;
        moveInput = value.Get<Vector2>();
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
        vector.y = moveInput.z * moveSpeed;
        rb2d.linearVelocity = vector;

        if (animator) {
            bool hasHorizontal = Mathf.Abs(vector.x) > Mathf.Epsilon;
            animator.SetBool("isWalking", hasHorizontal);
        }


    }
}
