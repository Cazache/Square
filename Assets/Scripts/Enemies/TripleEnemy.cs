using UnityEngine;

public class TripleEnemy : EnemyFly
{
    public Transform bulletSpawnPoint2;
    public Transform bulletSpawnPoint3;

    
    public override void Shoot(Transform bulletSpawn)
    {
        base.Shoot(bulletSpawnPoint);
        base.Shoot(bulletSpawnPoint2);
        base.Shoot(bulletSpawnPoint3);
    }
}
