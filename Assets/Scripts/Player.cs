using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Assertions.Must;

public class Player : MonoBehaviour
{

    public static Player Instance;
    private ParticleSystem deathParticle;
    private ParticleSystem jumpParticle;
    private ParticleSystem fallParticle;


    [Header("Movement")]
    Vector2 movimiento = Vector2.zero;
    float speed = 0f;
    public float moveSpeed = 5f;
    public float airSpeed = 5f;
    public float maxSpeed = 5f;
    public int armor = 0;
    public float dmgReductionBase = 0.05f;
    public float currentArmorReduction = 0;
    public float counterMovement = 0.175f;
    private float moviDir;
    private Rigidbody2D rb;
    public float fuerzaSalto = 5f;
    public bool Grounded = false;
    public Transform comprobadorSuelo;
    public float radioComprobador = 0.2f;
    public LayerMask FloorLayers;
    public LayerMask IgnoreInDash;
    public float factorRotacion;
    public float fuerzaRotacion;
    public float maxAirJumps;
    public float airJumpCount;
    public float airJumps;
    public bool dashcount;



    [Header("Dash")]
    public float dashSpeed = 10f;
    public float dashDuration = 0.5f;
    public float dashCd = 0.5f;
    public bool isDashing = false;



    [Header("Shoot")]
    public GameObject balaPrefab;
    public Transform shootpoint;

    public float fuerzaDisparo = 10f;
    public float fuerzaImpulso = 10f;
    public float distanciaPuntoDisparo = 1;



    public float mincdShoot;
    public int bulletCount = 5;
    public float anguloVariacionRifle = 30f;
    public float anguloVariacionShotGun = 50f;
    bool DashShoot = false;

    [Header("Cool down")]
    public float CDShootBase = 1; //Se empieza la partida con el
    public float RealCurrenCDShoot; //El que se tiene durante la partida, sumando mejoras pero independientemente del arma
    public float CDReductionBase;

    [Header("Damage")]
    public float realBulletDMG;
    public float bulletDMGWithWeapon;


    public float life = 20;
    public float explosionForce;
    public float explosionRadius;

    public int coins;

    private SpriteRenderer spriteRenderer;
    private Color originalColor;

    public GunController gun;

    public Dictionary<string, bool> actionCooldowns = new Dictionary<string, bool>();
    public Dictionary<string, float> cooldownTimes = new Dictionary<string, float>();

    public enum Weapons
    {
        gun,
        shotgun,
        submachinegun
    }
    [SerializeField] public Weapons weapon;
    public List<int> Currentweapons = new List<int>();

    [Header("Sounds")]
    private AudioSource playerSource;
    public AudioClip shootSound;
    public AudioClip getDamageSound;
    public AudioClip dashSound;
    public AudioClip changeWeaponSound;

    private void Awake()
    {

        if (Instance == null)
        {
            Instance = this;
            // Resto de la lógica de inicialización del jugador
        }
        else
        {
            // El objeto ya tiene asignada una instancia, así que este objeto se destruye
            if (!DashShoot)
                Destroy(this);
        }
        gameObject.SetActive(false);
        spriteRenderer = GetComponent<SpriteRenderer>();
        originalColor = spriteRenderer.color;
        playerSource = GetComponent<AudioSource>();

    }
    void Start()
    {

        RealCurrenCDShoot = CDShootBase;
        currentArmorReduction = dmgReductionBase;
        deathParticle = Resources.Load<ParticleSystem>("Particle/Death/Particle Enemy Death");
        jumpParticle = Resources.Load<ParticleSystem>("Particle/Jump Particle");
        fallParticle = Resources.Load<ParticleSystem>("Particle/Fall Particle");
        rb = GetComponent<Rigidbody2D>();
        InitActions();
        ChangeWeapon();

    }
    private void Update()
    {
            InputSystem();
    }
    void FixedUpdate()
    {
        CheckGrounded();
        if (rb)
            Movement();
    }
    void InitActions()
    {
        // Inicializa los cooldowns y tiempos de cooldown para diferentes acciones
        actionCooldowns["Shoot"] = true;
        cooldownTimes["Shoot"] = RealCurrenCDShoot; // Tiempo de cooldown para disparar

        actionCooldowns["Dash"] = true;
        cooldownTimes["Dash"] = 0.7f; // Tiempo de cooldown para el dash
    }
    void InputSystem()
    {
        if (!GameManager.instance.paused && !GameManager.instance.upgrading && Player.Instance.life > 0)
        {
            if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.Space))
            {
                if (rb)
                    Jump();
            }

            if (Input.GetMouseButton(0) && actionCooldowns["Shoot"] && !isDashing)
            {
                Shoot();
                StartCoroutine(Cooldown("Shoot", cooldownTimes["Shoot"]));
            }

            if (Input.GetAxis("Mouse ScrollWheel") != 0)
            {
                ChangeWeapon();
            }

            aim();
        }


        if (Input.GetKeyDown(KeyCode.Escape))
        {
            GameManager.instance.Pause();
        }

        if (Input.GetKeyDown(KeyCode.LeftShift) && !isDashing && actionCooldowns["Dash"])
        {

            StartCoroutine(Dash());
            StartCoroutine(Cooldown("Dash", cooldownTimes["Dash"]));
        }
    }
    private void Movement()
    {
        moviDir = Input.GetAxis("Horizontal");

        if (Grounded)
        {
            speed = moveSpeed;
        }
        else
        {
            speed = airSpeed;
        }

        if (rb.velocity.magnitude < maxSpeed)
        {
            movimiento = new Vector2(moviDir * speed, rb.velocity.y);
            if (Grounded)
            {
                if (moviDir < 0 && rb.velocity.x > 0 || moviDir > 0 && rb.velocity.x < 0)
                {
                    rb.velocity = new Vector2(rb.velocity.x / counterMovement, rb.velocity.y);
                }
            }
            rb.AddForce(movimiento, ForceMode2D.Force);
        }

    }
    private void Jump()
    {
        if (airJumps >= airJumpCount && !Grounded)
        {
            return;
        }
        else if (!Grounded)
        {
            airJumps++;
            if (rb.velocity.x > 0 && moviDir < 0 || rb.velocity.x < 0 && moviDir > 0)
                rb.velocity = new Vector2(-rb.velocity.x, rb.velocity.y);
        }



        rb.velocity = new Vector2(rb.velocity.x, fuerzaSalto);
        float rotacion = fuerzaRotacion * Mathf.Abs(rb.velocity.x) * factorRotacion;
        if (rotacion < 25)
            rotacion = 25;


        if (moviDir < 0)
        {
            rb.AddTorque(rotacion);
        }
        else if (moviDir >= 0)
        {
            rb.AddTorque(-rotacion);
        }
        Instantiate(jumpParticle, new Vector2(transform.position.x, transform.position.y - 0.5f), jumpParticle.transform.rotation);
    }
    private void aim()
    {
        // Girar el punto de disparo alrededor del jugador
        Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector3 direccionDisparo = mousePosition - transform.position;
        float angulo = Mathf.Atan2(direccionDisparo.y, direccionDisparo.x) * Mathf.Rad2Deg;
        shootpoint.position = transform.position + Quaternion.Euler(0f, 0f, angulo) * Vector3.right;
        shootpoint.rotation = Quaternion.Euler(0f, 0f, angulo);
    }
    private void Shoot()
    {
        SoundController.soundController.StartSound(playerSource, shootSound, false, 0.4f);
        switch (weapon)
        {
            case Weapons.shotgun:
                // Código para la escopeta

                for (int i = 0; i < 4; i++)
                {
                    float anguloshotgun = UnityEngine.Random.Range(-anguloVariacionShotGun, anguloVariacionShotGun);
                    Quaternion rotacionBalashotgun = Quaternion.AngleAxis(anguloshotgun, Vector3.forward);
                    GameObject balashotgun = Instantiate(balaPrefab, shootpoint.position, rotacionBalashotgun);
                    balashotgun.GetComponent<Bullet>().bulletDmg = bulletDMGWithWeapon;
                    Rigidbody2D rbBalashotgun = balashotgun.GetComponent<Rigidbody2D>();
                    rbBalashotgun.AddForce(rotacionBalashotgun * shootpoint.right * fuerzaDisparo, ForceMode2D.Impulse);
                }

                // Empujar al jugador hacia el lado contrario del disparo
                if (!Grounded)
                {
                    rb.velocity = Vector2.zero;
                    rb.AddForce(-shootpoint.right * fuerzaImpulso, ForceMode2D.Impulse);
                }
                break;

            case Weapons.gun:

                GameObject balarifle = Instantiate(balaPrefab, shootpoint.position, Quaternion.identity);
                balarifle.GetComponent<Bullet>().bulletDmg = bulletDMGWithWeapon;
                Rigidbody2D rbBalarifle = balarifle.GetComponent<Rigidbody2D>();
                rbBalarifle.AddForce(shootpoint.right * fuerzaDisparo, ForceMode2D.Impulse);

                break;

            case Weapons.submachinegun:


                float angulo = UnityEngine.Random.Range(-anguloVariacionRifle, anguloVariacionRifle);
                Quaternion rotacionBala = Quaternion.AngleAxis(angulo, Vector3.forward);
                GameObject balasubmachinegun = Instantiate(balaPrefab, shootpoint.position, rotacionBala);
                balasubmachinegun.GetComponent<Bullet>().bulletDmg = bulletDMGWithWeapon;
                Rigidbody2D rbBalasubmachinegun = balasubmachinegun.GetComponent<Rigidbody2D>();
                rbBalasubmachinegun.AddForce(rotacionBala * shootpoint.right * fuerzaDisparo, ForceMode2D.Impulse);


                break;

            default:
                // Cualquier otro manejo que necesites para otro tipo de arma
                break;
        }
    }
    private void ChangeWeapon()
    {
        // Obtener la cantidad de armas en el enum Weapons
        int weaponCount = Enum.GetNames(typeof(Weapons)).Length;

        // Determinar la dirección del desplazamiento del mouse
        int direction = Input.GetAxis("Mouse ScrollWheel") > 0 ? 1 : -1;

        // Bucle para buscar la siguiente arma válida
        do
        {
            // Calcular el índice de la siguiente arma dentro de los límites del enum
            weapon = (Weapons)(((int)weapon + weaponCount + direction) % weaponCount);
        } while (!Currentweapons.Contains((int)weapon)); // Continuar hasta encontrar un arma válida

        getCurrentWeaponStats();

        // Pasar el nombre del valor actual de weapon a la función gun.changeSprite()
        string weaponName = Enum.GetName(typeof(Weapons), weapon);
        gun.changeSprite(weaponName);
        GameManager.instance.UpdatestatsUI();
        SoundController.soundController.StartSound(playerSource, changeWeaponSound, false, 0.8f);
    }
    void getCurrentWeaponStats()
    {
        // Obtener el nombre del valor actual de weapon


        switch (weapon)
        {
            case Weapons.shotgun:
                cooldownTimes["Shoot"] = RealCurrenCDShoot * 2;
                bulletDMGWithWeapon = realBulletDMG * 1.3f;
                break;

            case Weapons.gun:
                cooldownTimes["Shoot"] = RealCurrenCDShoot;
                bulletDMGWithWeapon = realBulletDMG;
                break;

            case Weapons.submachinegun:
                cooldownTimes["Shoot"] = RealCurrenCDShoot / 2;
                bulletDMGWithWeapon = realBulletDMG / 1.2f;
                break;

            default:
                // Cualquier otro manejo que necesites para otro tipo de arma
                break;
        }
    }
    IEnumerator Dash()
    {
        isDashing = true;
        SoundController.soundController.StartSound(playerSource, dashSound, false);
        BoxCollider2D boxCollider = GetComponent<BoxCollider2D>();
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        Vector2 dashDirection = (shootpoint.position - transform.position).normalized;
        //boxCollider.enabled = false;
        rb.velocity = dashDirection * dashSpeed;
        rb.AddTorque(400);

        float trailSpawnInterval = 0.08f; // Intervalo entre cada spawn de la estela
        float timer = 0f;
        float traillifetime;
        if (!DashShoot)
            traillifetime = 0.3f;
        else
            traillifetime = 0.6f;

        float CloneAlpha = 0.3f;
        float ColorMod = 0.2f;


        Physics2D.IgnoreLayerCollision(gameObject.layer, 12, true);
        Physics2D.IgnoreLayerCollision(gameObject.layer, 8, true);

        // Bucle para generar la estela mientras dure el dash
        while (timer < dashDuration)
        {
            // Crea una nueva instancia del objeto Trail en la posición actual del jugador
            GameObject playerObjet = Instantiate(gameObject, transform.position, Quaternion.identity);
            playerObjet.gameObject.SetActive(true);
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

        Physics2D.IgnoreLayerCollision(gameObject.layer, 12, false);
        Physics2D.IgnoreLayerCollision(gameObject.layer, 8, false);
        // Restaurar la velocidad y activar el collider después del dash
        rb.gravityScale = 2f;
        rb.velocity /= 2;
        rb.angularVelocity /= 4;
        boxCollider.enabled = true;
        isDashing = false;
    }
    public void gethit(float damage)
    {
        GameManager.instance.shakeCamera(0.8f, 0.15f);
        SoundController.soundController.StartSound(playerSource, getDamageSound, false, 0.45f);
        // Calcular el daño reducido por la armadura
        float reducedDamage = damage / (1 + armor * currentArmorReduction);

        life -= reducedDamage;
        GameManager.instance.lifeSlider.value = life;
        GameManager.instance.UpdatestatsUI();

        // Iniciar la corutina para cambiar el color gradualmente
        StartCoroutine(ChangeColorTemporarily());

        if (life <= 0)
        {
            StartCoroutine(Dead());
        }
    }
    float calculateDMGReduction()
    {
        float DMGReduction = dmgReductionBase;

        if (armor >= 20)
        {
            DMGReduction *= 0.5f;
        }
        else if (armor >= 12)
        {
            DMGReduction *= 1f;
        }
        else if (armor >= 5)
        {

            DMGReduction *= 2f;
        }

        return DMGReduction;
    }
    void calculateCDReduction()
    {
        float CDReduction = CDReductionBase;

        if (cooldownTimes["Shoot"] > 6)
        {
            CDReduction *= 0.95f; // 80% de reducción
        }


        float reducedCD = CDShootBase / (1 + cooldownTimes["Shoot"] * CDReduction);

        RealCurrenCDShoot = reducedCD;

        RealCurrenCDShoot = Mathf.Max(RealCurrenCDShoot, mincdShoot); // Asegura que CurrentcdShoot no sea menor que mincdShoot
    }
    private void CheckGrounded()
    {
        // Creamos un Raycast hacia abajo desde el comprobadorSuelo para detectar el suelo
        Grounded = Physics2D.Raycast(comprobadorSuelo.position, Vector2.down, radioComprobador, FloorLayers);
    }
    IEnumerator Cooldown(string action, float cooldownTime)
    {
        actionCooldowns[action] = false; // Deshabilita la acción durante el cooldown
        yield return new WaitForSeconds(cooldownTime);
        actionCooldowns[action] = true; // Habilita la acción después del cooldown
    }
    IEnumerator ChangeColorTemporarily()
    {
        // Cambiar gradualmente el color del sprite a FF9999
        Color targetColor = new Color(143f / 255f, 195f / 255f, 255f / 255f, 1f);
        float changeDuration = 0.1f; // Duración del cambio de color en segundos

        float currentTime = 0f;
        while (currentTime < changeDuration)
        {
            spriteRenderer.color = Color.Lerp(originalColor, targetColor, currentTime / changeDuration);
            currentTime += Time.deltaTime;
            yield return null;
        }

        // Cambiar gradualmente el color del sprite de regreso a FF0000
        currentTime = 0f;
        while (currentTime < changeDuration)
        {
            spriteRenderer.color = Color.Lerp(targetColor, originalColor, currentTime / changeDuration);
            currentTime += Time.deltaTime;
            yield return null;
        }
    }
    IEnumerator Dead()
    {
        GetComponent<SpriteRenderer>().enabled = false;
        GetComponent<BoxCollider2D>().enabled = false;
        DeathParticle();
        Time.timeScale = 0.25f;
        Time.fixedDeltaTime = Time.timeScale * .02f;
        yield return new WaitForSeconds(1);
        GameManager.instance.GameOver();
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
    public void Explode()
    {
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
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (Grounded)
        {
            Instantiate(fallParticle, new Vector2(transform.position.x, transform.position.y - 0.5f), fallParticle.transform.rotation);
            airJumps = 0;
        }
    }
    void OnDrawGizmosSelected()
    {

        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, radioComprobador);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, explosionRadius);
    }

    public void GetUpgrade(Dictionary<string, int> effects)
    {
        foreach (var effect in effects)
        {
            string effectName = effect.Key;
            int effectValue = effect.Value;

            // Imprime el nombre del efecto
            Debug.Log("Efecto recibido: " + effectName);

            // Realiza diferentes acciones según el efecto
            switch (effectName)
            {
                case "Armor":
                    armor += effectValue;
                    currentArmorReduction = calculateDMGReduction();
                    break;
                case "Bullets":
                    bulletCount += effectValue;
                    break;
                case "CD":
                    cooldownTimes["Shoot"] += effectValue;
                    if (RealCurrenCDShoot > mincdShoot)
                    {
                        calculateCDReduction();
                    }
                    else
                        print("Maximo CD alcanzado");
                    break;
                case "Damage":
                    realBulletDMG += effectValue;
                    break;
                case "Healing":
                    float healingAmount = 0f;

                    switch (effectValue)
                    {
                        case 1:
                            healingAmount = 0.2f;
                            break;
                        case 2:
                            healingAmount = 0.35f;
                            break;
                        case 3:
                            healingAmount = 0.5f;
                            break;
                        case 4:
                            healingAmount = 0.8f;
                            break;
                        case 5:
                            healingAmount = 1f;
                            break;
                        default:
                            // Si effectvalue no coincide con ningún caso, no se realiza curación.
                            break;
                    }

                    life += GameManager.instance.lifeSlider.maxValue * healingAmount; // Aumentar la vida en función del porcentaje de curación.

                    if (life > GameManager.instance.lifeSlider.maxValue)
                        life = GameManager.instance.lifeSlider.maxValue;

                    GameManager.instance.lifeSlider.value = life;
                    break;
                case "Jump":
                    if (airJumpCount < maxAirJumps)
                        airJumpCount++;
                    break;
                case "Max Health":

                    float Amount = 0f;

                    switch (effectValue)
                    {
                        case 1:
                            Amount = 5f;
                            break;
                        case 2:
                            Amount = 10f;
                            break;
                        case 3:
                            Amount = 15f;
                            break;
                        case 4:
                            Amount = 20f;
                            break;
                        case 5:
                            Amount = 30f;
                            break;
                        default:
                            // Si effectvalue no coincide con ningún caso, no se realiza curación.
                            break;
                    }
                    life += Amount;
                    GameManager.instance.lifeSlider.maxValue += Amount;
                    GameManager.instance.lifeSlider.value = life;
                    break;
                case "Weapon":
                    Currentweapons.Add(effectValue);
                    break;
                default:
                    // Acción predeterminada en caso de un efecto desconocido
                    Debug.LogWarning("Efecto desconocido: " + effectName);
                    break;
            }
            getCurrentWeaponStats();
            GameManager.instance.UpdatestatsUI();

        }

    }

}
