using System;
using System.Drawing;
using Unity.Burst;
using Unity.VisualScripting;
using UnityEngine;
using static UnityEngine.ParticleSystem;

public class PlayerStatsManager : MonoBehaviour {

    public int playerIndex = 0;

    [Header("Movement")]
    public float dashCooldown = 1f;
    public float jumpCooldown;

    [Header("Base Stats")]
    public float strength = 5f;
    public float intelligence = 2f;
    public float moveSpeed = 5f;
    public float vitality = 5f;
    public float armor = 1f;
    public float playerSize;
    public float haste;

    [Header("General Offensive")]
    public float armorPen;
    public float dodgeChance;
    public float explosionSize;
    public float explosionDamage;

    [Header("General Defensive")]
    public float damageReduction;
    public float stunResistance;
    public float debufResistance;

    [Header("Melee Attributes")]
    public float meleeDamage;
    public float meleeSpeed;
    public float parryCooldown;
    public float weaponSize;

    [Header("Ranged Attributes")]
    public float weaponRange;
    public float rangedSpeed;
    public float rangedDamage;
    public float projectileAmount;
    public float weaponBurst;
    public float projectileBounce;
    public float projectileSize;
    public float spread;

    [Header("DOT Attributes")]
    public float burnDamage;
    public float poisonDamage;
    public float diseaseDamage;

}
