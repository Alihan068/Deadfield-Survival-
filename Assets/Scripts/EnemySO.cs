using UnityEngine;

[CreateAssetMenu(fileName = "New Enemy Type", menuName = "Create EnemyType")]
public class EnemySO : ScriptableObject
{
  public EnemyType enemyType;

    public float extraHp = 0f;
    public float extraDamage = 0f;
    public float extraRange = 5f;
    public float extraAttackSpeed = 0f;
    public float extraBounce = 0f;
    public float extraArmor = 0f;

    public float GetExtraHP() {
        return extraHp;
    }



}
