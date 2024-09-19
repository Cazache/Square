using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuObject : MonoBehaviour
{
    private SpriteRenderer spriteRenderer;
    AudioClip hit;
    private Color originalColor;
    private AudioSource audioSource;
    Color onHitColor;
    public ParticleSystem hitParticle;
    // Start is called before the first frame update
    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        audioSource = GetComponent<AudioSource>();
        originalColor = spriteRenderer.color;
        onHitColor = getHitColor();
        hit = Resources.Load<AudioClip>("Sound/Damage/Enemy Damage");
    }

    // Update is called once per frame
    void Update()
    {
        if(transform.position.y < -6)
            Destroy(gameObject);
    }
    private void OnCollisionEnter2D(Collision2D collision)
    {
        getParticleColor(collision.transform.position);
        SoundController.soundController.StartSound(audioSource, hit,false, 0.2f);
        StartCoroutine(ChangeColorTemporarily());
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
    void getParticleColor(Vector2 position)
    {
        Color balaColor = GetComponent<SpriteRenderer>().color;
        ParticleSystem destroyParticle = Instantiate(hitParticle, position, Quaternion.identity);
        ParticleSystem particleSystem = destroyParticle.GetComponent<ParticleSystem>();
        var mainModule = particleSystem.colorOverLifetime;

        Gradient particleGradient = new Gradient();
        GradientColorKey[] particleColorKeys = new GradientColorKey[2];
        particleColorKeys[0].color = balaColor;
        particleColorKeys[0].time = 0.0f;
        particleColorKeys[1].color = new Color(balaColor.r, balaColor.g, balaColor.b, 0); // Color con alpha a 0
        particleColorKeys[1].time = 1.0f;

        GradientAlphaKey[] particleAlphaKeys = new GradientAlphaKey[2];
        particleAlphaKeys[0].alpha = 1.0f;
        particleAlphaKeys[0].time = 0.3f;
        particleAlphaKeys[1].alpha = 0.0f;
        particleAlphaKeys[1].time = 1.0f;

        particleGradient.SetKeys(particleColorKeys, particleAlphaKeys);
        mainModule.color = particleGradient;
    }
}
