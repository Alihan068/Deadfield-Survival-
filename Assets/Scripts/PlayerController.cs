using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour {

    PlayerStatsManager playerStatsManager;
    Rigidbody2D rb2d;
    Animator animator;
    Coroutine coroutine;

    [SerializeField] float startDashTime = 1f;
    [SerializeField] float dashSpeed = 10f;
    [SerializeField] GameObject weaponManager;
    float currentDashTime;

    bool canDash = true;
    bool playerCollision = true;

    public Weapon weapon;

    Vector3 moveInput;
    bool isAlive = true;

    void Start() {
        weapon = GetComponentInChildren<Weapon>();
        playerStatsManager = GetComponent<PlayerStatsManager>();
        rb2d = GetComponent<Rigidbody2D>();
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
            animator.SetBool("isWalking", hasHorizontal);
        }
    }

    void AimForMouse () {
        Vector2 mouseDir = Input.mousePosition.normalized;
        Vector2 dirToMouse = transform.rotation * mouseDir;
        //weaponManager.transform.position = weaponManager.transform.rotation * dirToMouse;
        weaponManager.transform.up = Input.mousePosition - transform.position;
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
            Gizmos.DrawWireSphere(transform.position, playerStatsManager.playerBaseRange);
        }
    }
}
