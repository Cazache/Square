using System.Collections;
using UnityEngine;

public class EnemyFly : EnemyController
{
    private Rigidbody2D rb;
    public float rotationSpeed = 5;
    bool lookplayer = true;
    public float recoveryTime;
    public int NumberofShoots;
    public float cdShoots;


    public override void Start()
    {
        base.Start();
        rb = GetComponent<Rigidbody2D>();
    }
    public override void Update()
    {
        base.Update();
        if (lookplayer)
        {
            
            lookAtplayer();
        }
        else
        {
            if (rb.velocity.magnitude > 0)
                rb.velocity = rb.velocity / 1.015f;
        }
    }
    public override void Move()
    {
        // Move towards the player in 2D space
        Vector3 targetPosition = new Vector3(player.position.x, player.position.y, transform.position.z);
        transform.position = Vector3.MoveTowards(transform.position, targetPosition, moveSpeed * Time.deltaTime);
    }
    public virtual void lookAtplayer()
    {
        if(rb)
        {

            // Gradually rotate the enemy to face the player on the Z-axis (2D rotation)
            Vector3 directionToPlayer = player.position - transform.position;
            float angle = Mathf.Atan2(directionToPlayer.y, directionToPlayer.x) * Mathf.Rad2Deg;

            // Smoothly interpolate the current rotation towards the target rotation
            float step = rotationSpeed * Time.deltaTime;
            float newAngle = Mathf.MoveTowardsAngle(rb.rotation, angle, step);
            rb.MoveRotation(newAngle);
        }

    }
    public override void gethit(float damage)
    {
        base.gethit(damage);
        StartCoroutine(LookPlayerController());
    }
    public override void Shoot(Transform bulletSpawn)
    {
        if (lookplayer)
        {          
                StartCoroutine(ShootMultipleTimes(NumberofShoots, bulletSpawn));            
        }
    }

    IEnumerator LookPlayerController()
    {
        lookplayer = false;
        yield return new WaitWhile(() => rb.velocity.magnitude > 3);
        lookplayer = true;
        rb.velocity = Vector3.zero;
    }
    IEnumerator ShootMultipleTimes(int shots, Transform bulletSpawn)
    {
        for (int i = 0; i < shots; i++)
        {
            base.Shoot(bulletSpawn);
            yield return new WaitForSeconds(cdShoots);
        }
    }
}
