using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class EnemyCharge : EnemyFly
{

    public float rotationTime = 3.0f; // Tiempo total de rotación
    public bool attacking, isDashing;
    public float maxRotationSpeed = 100.0f; // Velocidad máxima de rotación
    public float dashSpeed;
    public float pushForce;
    public float dashDuration;
    public float deflectForce = 10f; // Ajusta la fuerza de desviación según tus necesidades
    public float detectionRadius = 5f; // Ajusta el radio de detección según tus necesidades
    public LayerMask bulletLayer; // Asigna la capa de las balas en el Inspector

    Rigidbody2D playerRB;
    Rigidbody2D Rb;

    [Header("Sound")]
     AudioClip dashSound;
    public override void Start()
    {
        base.Start();
        Rb = GetComponent<Rigidbody2D>();
        playerRB = Player.Instance.GetComponent<Rigidbody2D>();
    }
    public override void loadSounds()
    {
        base.loadSounds();
        dashSound = Resources.Load<AudioClip>("Sound/Dash/Enemy dash");
    }
    public override void Update()
    {
        // Verificar si el jugador está dentro del rango de disparo
        float distanceToPlayer = Vector2.Distance(transform.position, player.position);

        if (distanceToPlayer <= shootingRange && CheckIfPlayerInSight())
        {
         
            // Disparar al jugador si puede hacerlo
            if (canShoot)
            {
                Shoot(bulletSpawnPoint);
                StartCoroutine(ShootingCooldown());
            }
        }
        else
        {

            Move();
        }
    }
    public override void Shoot(Transform bulletSpawn)
    {
        if (!attacking)
            StartCoroutine(AttackRoutine());

    }
    public override void Move()
    {
        if (!isDashing)
            base.Move();
    }

    IEnumerator AttackRoutine()
    {

        attacking = true;
        // Frenar la velocidad de movimiento gradualmente hasta quedarse quieto
        float stopTime = 1.0f; // Tiempo para frenar completamente
        float stopElapsedTime = 0f;

        Rigidbody2D rb = GetComponent<Rigidbody2D>();

        while (stopElapsedTime < stopTime)
        {


            // Calcular la nueva posición basada en la velocidad actual
            Vector2 newPosition = rb.position - rb.velocity * Time.deltaTime;

            // Aplicar la nueva posición
            rb.MovePosition(newPosition);

            stopElapsedTime += Time.deltaTime;
            yield return null;
        }



        // Hacer que el objeto gire varias veces cada vez más rápido
        float spinElapsedTime = 0f;


        while (spinElapsedTime < rotationTime)
        {
            float currentRotationSpeed = Mathf.Lerp(rotationSpeed, maxRotationSpeed, spinElapsedTime / rotationTime);
            transform.Rotate(Vector3.forward, currentRotationSpeed * Time.deltaTime);

            spinElapsedTime += Time.deltaTime;
            yield return null;
        }


        // Puedes llamar a tu función de ataque aquí si es necesario
        StartCoroutine(Attack());


    }


    public override bool CheckIfPlayerInSight()
    {
        return true;
    }

    IEnumerator Attack()
    {
        SoundController.soundController.StartSound(enemySource, dashSound, false);

        isDashing = true;
    
        Rigidbody2D rb = GetComponent<Rigidbody2D>();

        Vector2 dashDirection = (player.position - transform.position).normalized;

        rb.velocity = dashDirection * dashSpeed;


        float trailSpawnInterval = 0.1f; // Intervalo entre cada spawn de la estela
        float timer = 0f;
        float traillifetime = 0.3f;

        float CloneAlpha = 0.3f;
        float ColorMod = 0.2f;

        gameObject.layer = 13;

        // Bucle para generar la estela mientras dure el dash
        while (timer < dashDuration)
        {
            // Crea una nueva instancia del objeto Trail en la posición actual del jugador
            GameObject playerObjet = Instantiate(gameObject, transform.position, Quaternion.identity);
            Destroy(playerObjet.GetComponent<BoxCollider2D>());
            Destroy(playerObjet.GetComponent<Rigidbody2D>());
            Destroy(playerObjet, traillifetime);
            // Asigna la transparencia al sprite de la estela (puedes ajustar este valor según tus necesidades)
            Color trailColor = playerObjet.GetComponent<SpriteRenderer>().color;
            trailColor.r = Mathf.Min(1.0f, trailColor.r + ColorMod); // Incrementa el canal rojo
            trailColor.g = Mathf.Min(1.0f, trailColor.g + ColorMod); // Incrementa el canal verde
            trailColor.b = Mathf.Min(1.0f, trailColor.b + ColorMod); // Incrementa el canal azul
            trailColor.a = CloneAlpha; // Transparencia (0 a 1)
            playerObjet.GetComponent<SpriteRenderer>().color = trailColor;
            CloneAlpha += 0.2f;
            ColorMod -= 0.05f;
            // Espera el intervalo antes de generar el siguiente segmento de la estela
            yield return new WaitForSeconds(trailSpawnInterval);
            timer += trailSpawnInterval;
        }

        gameObject.layer = 9;

        // Restaurar la velocidad y activar el collider después del dash
        rb.velocity /= 2;
        rb.angularVelocity /= 2;
        attacking = false;
        isDashing = false;
    }

    public override void gethit(float damage)
    {
        if (!isDashing)
        {
            base.gethit(damage);
        }

    }
    public override void OnCollisionEnter2D(Collision2D collision)
    {

        if (collision.gameObject.layer == 7 && isDashing)
        {
            HitPlayer();
        }
        else if (!isDashing)
        {
            base.OnCollisionEnter2D(collision);
        }
    }
    void HitPlayer()
    {
        Player.Instance.gethit(bulletDMG);
        playerRB.AddForce(Rb.velocity * pushForce, ForceMode2D.Impulse);

    }
    public override void OnDrawGizmosSelected()
    {
        base.OnDrawGizmosSelected();
        // Dibujar el área de detección en el Editor
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
    }
}
