using System.Collections;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour {

    StatsManager statsManager;
    CustomTime customTime;
    Rigidbody2D rb2d;
    Animator animator;
    PlayerAttack playerAttack;
    Coroutine coroutine;

    [SerializeField] SpriteRenderer playerSprite;

    [SerializeField] float startDashTime = 1f;
    [SerializeField] float dashSpeed = 10f;

    WeaponSwitcher weaponSwitcher;
    float currentDashTime;

    public bool canAttack = true;
    bool canDash = true;
    bool playerCollision = true;

    public Weapon weapon;

    Vector3 moveInput;
    bool isAlive = true;

    void Start() {
        statsManager = GetComponent<StatsManager>();
        customTime = GetComponent<CustomTime>();
        playerAttack = GetComponent<PlayerAttack>();
        weaponSwitcher = GetComponentInChildren<WeaponSwitcher>();
        weapon = GetComponentInChildren<Weapon>();    
        rb2d = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
    }
    void OnMove(InputValue value) {
        moveInput = value.Get<Vector2>();
    }
    void OnDash(InputValue value) {
        Debug.Log("ShiftInput");
        if (canDash && isAlive && statsManager.canMove && customTime.timeScale > 0) {
            StartCoroutine(Dash());
        }
    }
    void OnAttack(InputValue value) {
        if (weapon == null) { Debug.Log("No weapon Found!"); return; }

        bool pressed = value.Get<float>() >= 1f;
        //Debug.Log($"OnAttack called - pressed: {pressed}");

        switch (weapon.weaponType) {
            case WeaponType.Melee:
                playerAttack.AttackCoroutine(pressed);
                break;

            case WeaponType.Ranged:
                weapon.SetFiring(pressed);
                break;
        }
    }

    // Update is called once per frame
    void FixedUpdate() {
        if (!isAlive 
            || !statsManager.canMove 
            || customTime.timeScale <= 0 
            || statsManager.isKnocked) return;
        AimForMouse();
        Walk();
    }

    void Walk() {
        //if (!canMove) return;       
        //FlipSprite();
        Vector2 vector = rb2d.linearVelocity;
        vector.x = moveInput.x * statsManager.moveSpeed;
        vector.y = moveInput.y * statsManager.moveSpeed;

            rb2d.linearVelocity = vector;
        

        if (animator) {
            bool hasHorizontal = Mathf.Abs(vector.x) > Mathf.Epsilon;
            animator.SetBool("isWalking", hasHorizontal);
        }
    }

    void AimForMouse() {
        var camera = Camera.main;
        Vector3 mouseDir = Mouse.current.position.ReadValue();
        mouseDir.z = Mathf.Abs(camera.transform.position.z - weaponSwitcher.transform.position.z);
        Vector3 mouseWorldPos = camera.ScreenToWorldPoint(mouseDir);
        Vector2 direction = mouseWorldPos - weaponSwitcher.transform.position;
        if (direction.sqrMagnitude > Mathf.Epsilon) {
            weaponSwitcher.transform.up = direction;
        }

        if (playerSprite != null) {
            float dx = mouseWorldPos.x - transform.position.x;
            if (Mathf.Abs(dx) > Mathf.Epsilon) {
                bool faceLeft;
                if (dx < 0f) faceLeft = true;
                else faceLeft = false;

                playerSprite.flipX = faceLeft;
            }
        }
    }

    IEnumerator Dash() {
        Debug.Log("DashCoroutine");
        canDash = false;
        playerCollision = false;
        currentDashTime = startDashTime; // Reset the dash timer.

        while (currentDashTime > 0f) {
            currentDashTime -= Time.deltaTime; // Lower the dash timer each frame.

            if (customTime.timeScale > 0) {
                rb2d.linearVelocity = moveInput * dashSpeed;
            }

            yield return null; // Returns out of the coroutine this frame so we don't hit an infinite loop.
        }

        rb2d.linearVelocity = new Vector2(0f, 0f); // Stop dashing.

        yield return new WaitForSeconds(statsManager.dashCooldown);
        canDash = true;

    }

    private void OnDrawGizmos() {
        Gizmos.color = Color.red;
        if (statsManager != null) {
            Gizmos.DrawWireSphere(transform.position, statsManager.baseRange);
        }
    }
}