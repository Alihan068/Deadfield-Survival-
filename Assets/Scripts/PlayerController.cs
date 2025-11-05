using System.Collections;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour {

    StatsManager statsManager;
    Rigidbody2D rb2d;
    Animator animator;
    PlayerAttack playerAttack;
    CustomTime customTime;
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
        playerAttack = GetComponent<PlayerAttack>();
        weaponSwitcher = GetComponentInChildren<WeaponSwitcher>();
        weapon = GetComponentInChildren<Weapon>();
        statsManager = GetComponent<StatsManager>();
        rb2d = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        customTime = GetComponent<CustomTime>();

        // Add CustomTime if not present
        if (customTime == null) {
            customTime = gameObject.AddComponent<CustomTime>();
        }
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

    void Update() {
        if (!isAlive || !statsManager.canMove || customTime.timeScale <= 0) return;
        AimForMouse();
        Walk();
    }

    void Walk() {
        Vector2 vector = rb2d.linearVelocity;

        // Use customTime.DeltaTime for time-scaled movement
        vector.x = moveInput.x * statsManager.moveSpeed;
        vector.y = moveInput.y * statsManager.moveSpeed;

        // Only apply velocity if not frozen
        if (customTime.timeScale > 0) {
            rb2d.linearVelocity = vector;
        }

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
        currentDashTime = startDashTime;

        while (currentDashTime > 0f) {
            // Use customTime.DeltaTime for time-scaled dash
            currentDashTime -= customTime.DeltaTime;

            if (customTime.timeScale > 0) {
                rb2d.linearVelocity = moveInput * dashSpeed;
            }

            yield return null;
        }

        rb2d.linearVelocity = new Vector2(0f, 0f);

        // Use real time for cooldown so it's not affected by freezes
        yield return new WaitForSecondsRealtime(statsManager.dashCooldown);
        canDash = true;
    }

    private void OnDrawGizmos() {
        Gizmos.color = Color.red;
        if (statsManager != null) {
            Gizmos.DrawWireSphere(transform.position, statsManager.baseRange);
        }
    }
}
