using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CoinController : MonoBehaviour
{
    public int value;
    public float exp;

    public ParticleSystem destroyCoin;
    public ParticleSystem getCoin;
    // Material original del objeto
    private SpriteRenderer sprite;

    // Duración total antes de destruir
    private float totalDuration = 16;

    // Duración de parpadeo
    private float blinkDuration;

    // Intervalo de parpadeo
    public float initialBlinkInterval = 0.5f;

    private float currentBlinkInterval;

    private float timer = 0.0f;

    bool blinking =false;
    public List<AudioClip> soundList = new List<AudioClip>();

    AudioSource coinSource;
    void Start()
    {
        blinkDuration = totalDuration / 2.5f;
        // Guardar el material original
        sprite = GetComponentInChildren<SpriteRenderer>();
        coinSource = gameObject.GetComponent<AudioSource>();
        // Iniciar la corutina para destruir el objeto
        StartCoroutine(DestroyAfterDelay(totalDuration));
    }

    void Update()
    {
        timer += Time.deltaTime;


        if (timer >= (totalDuration - blinkDuration) && !blinking)
        {
        
            blinking = true;
            StartCoroutine(Blink());
        }
    }

    IEnumerator DestroyAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        Instantiate(destroyCoin, transform.position, Quaternion.identity);
        Destroy(gameObject);
    }

    IEnumerator Blink()
    {
        float timeRemaining;
        while (true)
        {
            // Cambiar el material al material original
            sprite.enabled = false;
            yield return new WaitForSeconds(currentBlinkInterval);

            // Cambiar el material a un material transparente (o al material que desees para el parpadeo)
            sprite.enabled = true;
            yield return new WaitForSeconds(currentBlinkInterval);
            timeRemaining = totalDuration - timer;
            currentBlinkInterval = Mathf.Lerp(0.02f, initialBlinkInterval, timeRemaining / blinkDuration);
        }
    }

    void GiveCoins()
    {
        Player.Instance.coins += value;
        if (GameManager.instance != null)
            GameManager.instance.GetExp(exp);

       GameManager.instance.CoinsText.text = Player.Instance.coins.ToString();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.tag == "Player")
        {

            int clip = Random.Range(0, soundList.Count);
            SoundController.soundController.StartSound(coinSource, soundList[clip], false, 0.6f);
            GiveCoins();
            Instantiate(getCoin, transform.position, Quaternion.identity);
            transform.GetChild(0).gameObject.SetActive(false);
            gameObject.GetComponent<Collider2D>().enabled = false;
            Destroy(gameObject, 0.5f);
        }
    }
}
