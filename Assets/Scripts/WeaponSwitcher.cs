using UnityEngine;

public class WeaponSwitcher : MonoBehaviour {

    [SerializeField] int currentWeaponIndex = 0;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start() {
        SetWeaponActive();
    }

    void Update() {
        int previousWeaponIndex = currentWeaponIndex;

        ProcessScrollWheel();
        ProcessKeyInput();

        if (previousWeaponIndex != currentWeaponIndex) {
            SetWeaponActive();
        }
    }

    void SetWeaponActive() {
        int weaponIndex = 0;

        foreach (Transform weapon in transform) {

            if (weaponIndex == currentWeaponIndex) {
                weapon.gameObject.SetActive(true);
            }
            else {
                weapon.gameObject.SetActive(false);
            }
            weaponIndex++;
        }

    }

    void ProcessScrollWheel() {
        if (Input.GetAxis("Mouse ScrollWheel") < 0) {
            if (currentWeaponIndex >= transform.childCount - 1) {
                currentWeaponIndex = 0;
            }
            else {
                currentWeaponIndex++;
            }
        }

        if (Input.GetAxis("Mouse ScrollWheel") > 0) {
            if (currentWeaponIndex <= 0) {
                currentWeaponIndex = transform.childCount - 1;
            }
            else {
                currentWeaponIndex--;
            }
        }


    }
    void ProcessKeyInput() {
        if (Input.GetKeyDown(KeyCode.Alpha1)) {
            currentWeaponIndex = 0;
        }
        if (Input.GetKeyDown(KeyCode.Alpha2)) {
            currentWeaponIndex = 1;
        }
        if (Input.GetKeyDown(KeyCode.Alpha3)) {
            currentWeaponIndex = 2;
        }
        if (Input.GetKeyDown(KeyCode.Alpha4)) {
            currentWeaponIndex = 3;
        }
    }
}
