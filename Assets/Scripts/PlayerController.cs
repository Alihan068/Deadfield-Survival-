using System.Collections;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour {

    StatsManager playerStatsManager;
    Rigidbody2D rb2d;
    Animator animator;

    Coroutine coroutine;

    [SerializeField] float startDashTime = 1f;
    [SerializeField] float dashSpeed = 10f;
    WeaponSwitcher weaponSwitcher;
    float currentDashTime;

    public bool canMove = true;
    bool canDash = true;
    bool playerCollision = true;

    public Weapon weapon;

    Vector3 moveInput;
    bool isAlive = true;

    void Start() {
        weaponSwitcher = GetComponentInChildren<WeaponSwitcher>();
        weapon = GetComponentInChildren<Weapon>();
        playerStatsManager = GetComponent<StatsManager>();
        rb2d = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
    }
    void OnMove(InputValue value) {
        if (!isAlive) return;
        moveInput = value.Get<Vector2>();
    }
    void OnDash(InputValue value) {
        Debug.Log("ShiftInput");
        if (canDash && isAlive) {
            StartCoroutine(Dash());
        }
    }

    void OnAttack(InputValue value) {

        if (weapon == null) { Debug.Log("No weapon Found!"); return; }

        if (weapon.weaponType == WeaponType.Melee) {
            weapon.AttackWithWeapon();
            //MELEE WEAPON BEHAVIOR
        }
        else if (weapon.weaponType == WeaponType.Ranged) {
            //RANGED WEAPON BEHAVIOR
            
        }
        else {
            //MIXED WEAPONBEHAVIOR
        }


    }

    // Update is called once per frame
    void Update() {
        AimForMouse();
        Walk();
    }

    void Walk() {
        //if (!canMove) return;       

        Vector2 vector = rb2d.linearVelocity;
        vector.x = moveInput.x * playerStatsManager.moveSpeed;
        vector.y = moveInput.y * playerStatsManager.moveSpeed;
        rb2d.linearVelocity = vector;

        if (animator) {
            bool hasHorizontal = Mathf.Abs(vector.x) > Mathf.Epsilon;
            //animator.SetBool("isWalking", hasHorizontal);
        }
    }
    void FlipSprite() {

        bool playerHasHorizntalSpeed = Mathf.Abs(rb2d.linearVelocity.x) > Mathf.Epsilon;
        if (playerHasHorizntalSpeed) {
            transform.localScale =
                new Vector2(Mathf.Sign(rb2d.linearVelocity.x) * Mathf.Abs(transform.localScale.x), transform.localScale.y);
        }
    }

    void AimForMouse () {
        var camera = Camera.main;
        Vector3 mouseDir = Mouse.current.position.ReadValue();
        mouseDir.z = Mathf.Abs(camera.transform.position.z - weaponSwitcher.transform.position.z);
        Vector3 mouseWorldPos = camera.ScreenToWorldPoint(mouseDir);
        Vector2 direction = mouseWorldPos - weaponSwitcher.transform.position;
        if (direction.sqrMagnitude > Mathf.Epsilon) {
            weaponSwitcher.transform.up = direction;
        }

    }
    IEnumerator Dash() {
        Debug.Log("DashCoroutine");
        canDash = false;
        playerCollision = false;
        currentDashTime = startDashTime; // Reset the dash timer.

        while (currentDashTime > 0f) {
            currentDashTime -= Time.deltaTime; // Lower the dash timer each frame.

            rb2d.linearVelocity = moveInput * dashSpeed; // Dash in the direction that was held down.
            // No need to multiply by Time.DeltaTime here, physics are already consistent across different FPS.

            yield return null; // Returns out of the coroutine this frame so we don't hit an infinite loop.
        }

        rb2d.linearVelocity = new Vector2(0f, 0f); // Stop dashing.

        yield return new WaitForSeconds(playerStatsManager.dashCooldown);
        canDash = true;

    }

    private void OnDrawGizmos() {
        Gizmos.color = Color.red;
        if (playerStatsManager != null) {
            Gizmos.DrawWireSphere(transform.position, playerStatsManager.baseRange);
        }
    }
}
