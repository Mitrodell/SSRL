using UnityEngine;

public sealed class MeleeEnemy : Enemy
{
    protected override void TickAI(Vector3 toPlayerFlat, Vector3 separation)
    {
        Vector3 seek = toPlayerFlat.sqrMagnitude > 0.01f ? toPlayerFlat.normalized : Vector3.zero;
        Vector3 dir = seek + separation;

        if (dir.sqrMagnitude > 0.001f)
            desiredVelocity = dir.normalized * MoveSpeed;
    }
}
