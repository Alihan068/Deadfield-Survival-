using UnityEngine;

public class TimeController : MonoBehaviour {
    public void FreezeObject(GameObject obj) {
        CustomTime ct = obj.GetComponent<CustomTime>();
        if (ct != null)
            ct.timeScale = 0f;
    }

    public void UnfreezeObject(GameObject obj) {
        CustomTime ct = obj.GetComponent<CustomTime>();
        if (ct != null)
            ct.timeScale = 1f;
    }

    public void FreezeAllEnemies() {
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        foreach (GameObject enemy in enemies) {
            FreezeObject(enemy);
        }
    }
}