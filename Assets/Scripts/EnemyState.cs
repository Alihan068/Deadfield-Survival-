using UnityEngine;

public enum EnemyStateId
{
    Idle,
    Combat,
    Dead
}

public interface IEnemyState
{
    void OnEnter(EnemyController enemy);
    void OnUpdate(EnemyController enemy);
    void OnExit(EnemyController enemy);
}

public class EnemyIdleState : IEnemyState
{
    public void OnEnter(EnemyController enemy)
    {
        if (enemy == null) return;
        enemy.SetVelocity(Vector2.zero);
        enemy.SetWalkingAnimation(false);
    }

    public void OnUpdate(EnemyController enemy)
    {
        if (enemy == null) return;
        if (enemy.HasValidPlayerTarget())
        {
            float distance = enemy.DistanceToPlayer;
            if (distance <= enemy.DetectionRange)
            {
                enemy.ChangeState(new EnemyCombatState(), EnemyStateId.Combat);
            }
        }
    }

    public void OnExit(EnemyController enemy) { }
}

public class EnemyCombatState : IEnemyState
{
    public void OnEnter(EnemyController enemy) { }

    public void OnUpdate(EnemyController enemy)
    {
        if (enemy == null) return;

        if (!enemy.HasValidPlayerTarget())
        {
            enemy.SetVelocity(Vector2.zero);
            enemy.ChangeState(new EnemyIdleState(), EnemyStateId.Idle);
            return;
        }

        if (enemy.IsDead)
        {
            enemy.ChangeState(new EnemyDeadState(), EnemyStateId.Dead);
            return;
        }

        enemy.UpdateCombatMovementAndAttack();
    }

    public void OnExit(EnemyController enemy) { }
}

public class EnemyDeadState : IEnemyState
{
    public void OnEnter(EnemyController enemy)
    {
        if (enemy == null) return;
        enemy.SetVelocity(Vector2.zero);
        enemy.SetWalkingAnimation(false);
    }

    public void OnUpdate(EnemyController enemy) { }

    public void OnExit(EnemyController enemy) { }
}
