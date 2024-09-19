using System.Collections;
using UnityEngine;

public class EnemyController : MonoBehaviour
{
    [Header("Move")]
    public Transform player;
    public float moveSpeed = 3f;

    [Header("Shooting")]
    public float shootingRange = 5f;
    public GameObject bulletPrefab;
    public Transform bulletSpawnPoint;
    public float shootingCooldown = 1f;
    public float fuerzaDisparo = 10f;
    public float bulletDMG = 1f;
    public LayerMask WhatEnemySee;


    [Header("Stats")]
    public float Life = 10f;
    public float scoreForKill = 100;
    public float explosionForce;
    public float explosionRadius;
    private SpriteRenderer spriteRenderer;
    private Color originalColor;
    private ParticleSystem deathParticle;
    public bool canShoot = true;
    public bool death = false;
    public int coins;

    float minimumCollisionSpeed = 5.0f;

    Color balaColor;
    Gradient balaGradient;
    Color onHitColor;

    public GameObject PrefCoin;
    public AudioSource enemySource { get; private set; }
    [Header("Sound")]
    AudioClip deathSound;
    AudioClip ShootSound;
    AudioClip getDamageSound;
    public virtual void Start()
    {
  

        spriteRenderer = GetComponent<SpriteRenderer>();
        originalColor = spriteRenderer.color;
        player = Player.Instance.gameObject.transform;
        minimumCollisionSpeed = 14f;
        //Calculamos el color que tendran las balas y las guardamos para calcularlo una sola vez
        balaColor = GetComponent<SpriteRenderer>().color;
        balaGradient = getGradient();

        //Calculamos el color que tendran el enemigo al ser golpeado y lo guardamos para calcularlo una sola vez    
        onHitColor = getHitColor();

        canShoot = false;
        Invoke("SetCanShootTrue", 1f);
        deathParticle = Resources.Load<ParticleSystem>("Particle/Death/Particle Enemy Death");
        loadSounds();
    }
    public virtual void loadSounds()
    {
        enemySource = GetComponent<AudioSource>();
        ShootSound = Resources.Load<AudioClip>("Sound/Shoots/Enemy Shoot");
        deathSound = Resources.Load<AudioClip>("Sound/Death/Enemy Death");
        getDamageSound = Resources.Load<AudioClip>("Sound/Damage/Enemy Damage");
    }
    public virtual void Update()
    {

        // Verificar si el jugador está dentro del rango de disparo
        float distanceToPlayer = Vector2.Distance(transform.position, player.position);

        if (distanceToPlayer <= shootingRange && CheckIfPlayerInSight())
        {
            Aim();
            // Disparar al jugador si puede hacerlo
            if (canShoot && !death)
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

    public virtual void Move()
    {
    
        Vector3 targetPosition = new Vector3(player.position.x, transform.position.y, transform.position.z);
        transform.position = Vector3.MoveTowards(transform.position, targetPosition, moveSpeed * Time.deltaTime);
    }

    public virtual void Aim()
    {
        // Girar el punto de disparo alrededor del jugador
        Vector3 direccionDisparo = player.transform.position - transform.position;
        float angulo = Mathf.Atan2(direccionDisparo.y, direccionDisparo.x) * Mathf.Rad2Deg;
        bulletSpawnPoint.position = transform.position + Quaternion.Euler(0f, 0f, angulo) * Vector3.right;
        bulletSpawnPoint.rotation = Quaternion.Euler(0f, 0f, angulo);
    }

    public virtual void Shoot(Transform shootpoint)
    {
        if (death)
            return;
        SoundController.soundController.StartSound(enemySource, ShootSound, false, 0.4f);
        GameObject bala = Instantiate(bulletPrefab, shootpoint.position, transform.rotation);
        bala.GetComponent<Bullet>().bulletDmg = bulletDMG;
        bala.GetComponent<SpriteRenderer>().color = balaColor;

        TrailRenderer trailRenderer = bala.GetComponent<TrailRenderer>();
        trailRenderer.colorGradient = balaGradient;


        Rigidbody2D rbBala = bala.GetComponent<Rigidbody2D>();
        rbBala.AddForce(shootpoint.right * fuerzaDisparo, ForceMode2D.Impulse);
    }



    public IEnumerator ShootingCooldown()
    {

        canShoot = false;
        yield return new WaitForSeconds(shootingCooldown);
        canShoot = true;
    }

    public virtual void gethit(float damage)
    {
        Life -= damage;
        SoundController.soundController.StartSound(enemySource, getDamageSound, false, 0.15f);
        // Iniciar la corutina para cambiar el color gradualmente
        StartCoroutine(ChangeColorTemporarily());

        if (Life <= 0 && !death)
        {
            death = true;
            Explode();
            Death();
        }
    }

    public void Death()
    {
        GameManager.instance.shakeCamera(2f, 0.15f);
        createCoin();
        DeathParticle();
        spriteRenderer.enabled = false;
        gameObject.GetComponent<Collider2D>().enabled = false;
        Destroy(gameObject, 0.5f);
    }
    void createCoin()
    {
        GameObject NewCoin = Instantiate(PrefCoin, transform.position, Quaternion.identity);
        NewCoin.GetComponent<CoinController>().value = coins;
        NewCoin.GetComponent<CoinController>().exp = scoreForKill;
    }
    void DeathParticle()
    {
        // Crear una instancia de la misma partícula
        ParticleSystem particleSystem = Instantiate(deathParticle, transform.position, Quaternion.identity);

        // Modificar los parámetros de la partícula
        var colorOverLifetime = particleSystem.colorOverLifetime;
        Gradient gradient = new Gradient();
        gradient.SetKeys(
            new GradientColorKey[] { new GradientColorKey(spriteRenderer.color, 1.0f), new GradientColorKey(Color.white, 0.0f) },
            new GradientAlphaKey[] { new GradientAlphaKey(1f, 1.0f), new GradientAlphaKey(1f, 0.5f), new GradientAlphaKey(0f, 0.0f) }
        );
        colorOverLifetime.color = gradient;

        // Obtén el sprite del enemigo
        Sprite enemySprite = spriteRenderer.sprite;

        // Asigna el sprite al material del sistema de partículas
        ParticleSystemRenderer particleRenderer = particleSystem.GetComponent<ParticleSystemRenderer>();
        particleRenderer.material.mainTexture = enemySprite.texture;

        // Accede al módulo de emisión
        var trailsModule = particleSystem.trails;

        // Configura el color del Trail para que sea igual que el color de colorOverLifetime
        trailsModule.enabled = true; // Asegura que la emisión esté habilitada
        trailsModule.colorOverLifetime = gradient;
        trailsModule.colorOverTrail = gradient;

        // Opcionalmente, puedes ajustar otras propiedades del módulo de emisión según sea necesario
    }

    IEnumerator ChangeColorTemporarily()
    {
        float changeDuration = 0.2f; // Duración del cambio de color en segundos

        float currentTime = 0f;
        while (currentTime < changeDuration)
        {
            spriteRenderer.color = Color.Lerp(originalColor, onHitColor, currentTime / changeDuration);
            currentTime += Time.deltaTime;
            yield return null;
        }

        // Cambiar gradualmente el color del sprite de regreso al color original
        currentTime = 0f;
        while (currentTime < changeDuration)
        {
            spriteRenderer.color = Color.Lerp(onHitColor, originalColor, currentTime / changeDuration);
            currentTime += Time.deltaTime;
            yield return null;
        }
    }

    void Explode()
    {

        SoundController.soundController.StartSound(enemySource, deathSound, false);
        // Encontrar todos los objetos dentro del radio de explosión
        Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, explosionRadius);

        // Aplicar una fuerza a cada objeto encontrado
        foreach (Collider2D col in colliders)
        {
            Rigidbody2D rb = col.GetComponent<Rigidbody2D>();

            if (rb != null)
            {
                // Calcular la dirección desde este objeto al objeto actual
                Vector2 direction = (col.transform.position - transform.position).normalized;

                // Aplicar la fuerza en la dirección calculada
                rb.AddForce(direction * explosionForce, ForceMode2D.Impulse);
            }
        }
    }

    public virtual void OnDrawGizmosSelected()
    {
        // Dibujar un gizmo que represente el rango de disparo del enemigo
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, shootingRange);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, explosionRadius);
    }


    private void SetCanShootTrue()
    {
        canShoot = true;
    }
    public virtual bool CheckIfPlayerInSight()
    {
        Vector2 directionToPlayer = player.position - bulletSpawnPoint.position;
        RaycastHit2D hit = Physics2D.Raycast(bulletSpawnPoint.position, directionToPlayer, shootingRange, WhatEnemySee);


        // Verificar si el rayo golpea al jugador
        if (hit.collider != null && hit.collider.CompareTag("Player"))
        {
            return true;
        }

        return false;
    }


    public virtual void OnCollisionEnter2D(Collision2D collision)
    {
        float relativeSpeed = collision.relativeVelocity.magnitude;

        if (relativeSpeed < minimumCollisionSpeed || collision.gameObject.layer == 6)
        {
            return; // No hagas nada si la velocidad es demasiado baja o si es el mismo layer 6.
        }

        float dmg = 1;

        if (relativeSpeed >= 30)
        {

            dmg = relativeSpeed;
        }
        else if (relativeSpeed >= 20)
        {

            dmg = relativeSpeed / 3;
        }
        else if (relativeSpeed >= 16)
        {

            dmg = relativeSpeed / 6;
        }
        gethit(dmg);
    }

    Color getHitColor()
    {
        // Calcular un nuevo color más claroS
        Color hitColor = new Color(
            Mathf.Min(1f, originalColor.r + 0.6f), // Aumentar el componente rojo
            Mathf.Min(1f, originalColor.g + 0.6f), // Aumentar el componente verde
            Mathf.Min(1f, originalColor.b + 0.6f), // Aumentar el componente azul
            originalColor.a
        );

        return hitColor;
    }

    Gradient getGradient()
    {
        Gradient gradient = new Gradient();

        GradientColorKey[] colorKeys = new GradientColorKey[2];
        colorKeys[0].color = balaColor;
        colorKeys[0].time = 0.0f;
        colorKeys[1].color = new Color(balaColor.r, balaColor.g, balaColor.b, 0); // Color con alpha a 0
        colorKeys[1].time = 1.0f;

        GradientAlphaKey[] alphaKeys = new GradientAlphaKey[2];
        alphaKeys[0].alpha = 1.0f;
        alphaKeys[0].time = 0.0f;
        alphaKeys[1].alpha = 0.0f;
        alphaKeys[1].time = 1.0f;

        gradient.SetKeys(colorKeys, alphaKeys);
        return gradient;
    }
}