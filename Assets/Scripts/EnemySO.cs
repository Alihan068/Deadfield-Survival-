using UnityEngine;

[CreateAssetMenu(fileName = "New Enemy", menuName = "Game/Enemy")]
public class EnemySO : ScriptableObject
{
    [Header("Identity")]
    public string enemyName = "Enemy";

    [Header("Behavior")]
    public EnemyType enemyType = EnemyType.MeleeChaser;

    [Header("Base Stats")]
    public float baseMaxHealth = 100f;
    public float baseSpeed = 1f;
    public float baseRange = 1f;
    public float baseDamage = 1f;
    public float attackSpeed = 1f;

    [Header("Knockback")]
    public float knockBack = 5f;
    public float baseAppliedKnockback = 10f;
    public float knockbackStagger = 0.15f;
    public bool canDamageOnTouch = false;

    [Header("Animation")]
    public RuntimeAnimatorController bodyAnimatorController;
    public RuntimeAnimatorController weaponAnimatorController;

    [Header("Audio")]
    public AudioClip[] baseSounds;
}
