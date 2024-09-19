using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Cinemachine;
using Random = UnityEngine.Random;
using TMPro;
using UnityEngine.EventSystems;
using System;


public class GameManager : MonoBehaviour
{
    public static GameManager instance;
    public CinemachineVirtualCamera cinemachineVirtualCamera;
    CinemachineBasicMultiChannelPerlin _cbmcp;

    [Header("Particles")]
    public ParticleSystem spawnParticle;
    public ParticleSystem levelUpPaticle;
    public ParticleSystem PlayerspawnParticle;
    public Transform[] spawnPoints;


    public List<DifficultyInterval> difficultylevels;
    [SerializeField] private int currentDifficultyLevel;

    [Header("UI")]
    public Texture2D customCursor;
    public Slider expSlider;
    public Slider lifeSlider;
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI levelText;
    public TextMeshProUGUI timerText;
    public TextMeshProUGUI difficultyText;
    public TextMeshProUGUI lifeText;
    public TextMeshProUGUI DmgText;
    public TextMeshProUGUI ArmorText;
    public TextMeshProUGUI BulletsText;
    public TextMeshProUGUI CdText;
    public TextMeshProUGUI CoinsText;
    public Vector2 scoreStandarPos;
    public Vector2 scorePausePos;
    public Vector2 scoreDeathPos;
    public GameObject controlsPanel;
    public GameObject pausePanel;
    public GameObject DeadPanel;
    public GameObject upgradesPanel;
    public GameObject upgradesGrid;
    public GameObject currentUpgradesGrid;
    public GameObject upgradeUI;
    public float fillDuration;
    public Image progressBarImage;
    public Image LevelUpImage;
    private float fillAmount = 0.0f;
    private float elapsedTime = 0.0f;
    public int startSpawnSecond;

    [Header("EnemieScaler")]
    public float lifeScalerPerLevel;
    public float DmgScalerPerLevel;
    public float givedEXPscaler;

    [Header("OtherScaler")]
    public float LevelUpExpNeededScaler;
    public float upgradePriceScaler;


    [Header("Camera")]
    public float duracionTransicion = 3.0f;
    public Vector2 posicionFinal = Vector2.zero;
    public float orthographicSizeFinal = 10.0f;
    bool enTransicion;
    private float tiempoInicio;

    private float score = 0;
    private int lastSpawnedEnemyIndex = -1;
    private Animator animatorUI;
    public bool upgrading;

    //Exp
    private float expForLevelUp = 1000;
    private int level = 0;
    private float currentExp = 0;
    public bool paused = false;

    public int round;
    float timeForSpawn;

    public Transform enemyContiner;
    public List<GameObject> Maps = new List<GameObject>();
    [SerializeField] private float gameTimer = 0;

    [Serializable]
    public class UpgradeData
    {
        public List<Upgrade> Upgrades;
    }
    [System.Serializable]
    public class DifficultyInterval
    {
        public int difficultyLevel;
        public float startTime;
        public float endTime;
        public int numberofSpawns = 1;
        public float spawnInterval = 5f;
        public List<GameObject> Enemies = new List<GameObject>();
    }

    public UpgradeData upgradeData;

    [Serializable]
    public class Upgrade
    {
        public string Name { get; set; }
        public int Rarity { get; set; }
        public string Description { get; set; }
        public Dictionary<string, int> Effects { get; set; }
        public int Price;
    }

    private void Awake()
    {
        instance = this;
        // Ajusta la posición del cursor para que el centro de la textura esté en las coordenadas del mouse

        animatorUI = Canvas.FindObjectOfType<Canvas>().GetComponent<Animator>();
        pausePanel.SetActive(false);
        DeadPanel.SetActive(false);
        upgradesPanel.SetActive(false);
        controlsPanel.SetActive(true);
        elapsedTime = 0;
        fillAmount = 0;
        expSlider.value = 0;
        LoadUpgradesFromJSON("Upgrades.json");
        _cbmcp = cinemachineVirtualCamera.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();

        var life = Player.Instance.life;
        lifeSlider.maxValue = life;
        lifeSlider.value = life;
    }

    private void Start()
    {
        StartCoroutine(spawnPlayer());
        Vector2 hotSpot = new Vector2(customCursor.width / 2, customCursor.height / 2);
        // Establece el cursor con la nueva posición ajustada
        Cursor.SetCursor(customCursor, hotSpot, CursorMode.ForceSoftware);



        gameTimer = 0;
        score = 0;
        scoreText.rectTransform.anchoredPosition = scoreStandarPos;
        scoreText.alignment = TextAlignmentOptions.Midline;
        IniciarTransicion();
        LoadMap();
        loadGame();


    }

    private void Update()
    {
        if (enTransicion)
        {
            ActualizarTransicion();
        }
        UpdateTimer();

        if (currentDifficultyLevel < difficultylevels.Count - 1)
            CicularDificultBar();

        if (Input.GetKeyUp(KeyCode.K))
        {
            GetExp(1000);
        }
        if (Input.GetKeyUp(KeyCode.L))
        {
            Time.timeScale = 50;
        }

        if (upgrading)
        {
            GetCanvasObjectUnderMouse();
        }
        if (gameTimer > startSpawnSecond)
        {
            if (enemyContiner.childCount == 0 || timeForSpawn >= difficultylevels[currentDifficultyLevel].spawnInterval)
            {
                SpawnEnemy();
                controlsPanel.SetActive(false);
            }
        }

    }

    IEnumerator spawnPlayer()
    {
        Instantiate(PlayerspawnParticle, Player.Instance.transform.position, Quaternion.Euler(0, 0, 0));
        yield return new WaitForSeconds(PlayerspawnParticle.main.duration);
        Player.Instance.gameObject.SetActive(true);
    }

    public void Pause()
    {
        paused = !paused;
        if (paused)
        {
            scoreText.rectTransform.anchoredPosition = scorePausePos;
            scoreText.alignment = TextAlignmentOptions.MidlineLeft;
        }
        else
        {
            scoreText.rectTransform.anchoredPosition = scoreStandarPos;
            scoreText.alignment = TextAlignmentOptions.Midline;
        }



        if (!upgrading)
            Time.timeScale = paused ? 0 : 1;

        pausePanel.SetActive(paused);
        FormatTimer();
    }
    public void RestartGame()
    {
        SaveGame();

        Pause();

        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    private void SpawnEnemy()
    {
        round++;

        timeForSpawn = 0;

        for (int i = 0; i < difficultylevels[currentDifficultyLevel].numberofSpawns; i++)
        {
            Vector2 randomSpawnPoint;
            int maxAttempts = 10; // Número máximo de intentos para encontrar un punto vacío
            int currentAttempts = 0;
            Collider2D[] colliders = new Collider2D[0];
            do
            {
                randomSpawnPoint = new Vector2(Random.Range(spawnPoints[0].position.x, spawnPoints[1].position.x), Random.Range(spawnPoints[2].position.y, spawnPoints[3].position.y));
                currentAttempts++;

                // Verificar si el punto está ocupado por otro objeto, por ejemplo, usando OverlapCircle
                colliders = Physics2D.OverlapCircleAll(randomSpawnPoint, 1.5f); // Reemplaza 'yourEnemySize' por el tamaño de tu enemigo

            } while (colliders.Length > 0 && currentAttempts < maxAttempts);

            if (currentAttempts >= maxAttempts)
            {
                // No se pudo encontrar un punto vacío después de ciertos intentos, puedes manejar esto de acuerdo a tus necesidades
                Debug.Log("No se pudo encontrar un punto vacío después de varios intentos.");
                return;
            }

            int enemyIndex = GetRandomUniqueEnemyIndex();

            lastSpawnedEnemyIndex = enemyIndex;

            StartCoroutine(Spawn(randomSpawnPoint, enemyIndex));
        }
    }


    void SpawnPaticle(Vector2 randomSpawnPoint, int enemyIndex)
    {
        // Crear una instancia de la misma partícula
        ParticleSystem particleSystem = Instantiate(spawnParticle, randomSpawnPoint, Quaternion.identity);
        particleSystem.transform.SetParent(enemyContiner);
        // Obtener el color del SpriteRenderer del enemigo
        SpriteRenderer enemyColor = difficultylevels[currentDifficultyLevel].Enemies[enemyIndex].GetComponent<SpriteRenderer>();

        // Modificar los parámetros de la partícula
        var colorOverLifetime = particleSystem.colorOverLifetime;
        Gradient gradient = new Gradient();
        gradient.SetKeys(
            new GradientColorKey[] { new GradientColorKey(Color.white, 0.0f), new GradientColorKey(enemyColor.color, 1.0f) },
            new GradientAlphaKey[] { new GradientAlphaKey(0f, 0.0f), new GradientAlphaKey(1f, 0.5f), new GradientAlphaKey(1f, 1.0f) }
        );
        colorOverLifetime.color = gradient;

        // Obtén el sprite del enemigo
        Sprite enemySprite = enemyColor.sprite;
        // Asigna el sprite al material del sistema de partículas
        ParticleSystemRenderer particleRenderer = particleSystem.GetComponent<ParticleSystemRenderer>();
        particleRenderer.material.mainTexture = enemySprite.texture;
    }

    private IEnumerator Spawn(Vector2 randomSpawnPoint, int enemy)
    {
        SpawnPaticle(randomSpawnPoint, enemy);
        yield return new WaitForSeconds(1.5f);
        GameObject newenemy = Instantiate(difficultylevels[currentDifficultyLevel].Enemies[enemy], randomSpawnPoint, Quaternion.identity);
        newenemy.transform.parent = enemyContiner;
        EnemyController enemyController = newenemy.GetComponent<EnemyController>();
        if (currentDifficultyLevel > 0)
        {
            for (int y = 0; y < currentDifficultyLevel; y++)
            {
                enemyController.bulletDMG *= DmgScalerPerLevel;
                enemyController.Life *= lifeScalerPerLevel;
                enemyController.scoreForKill *= givedEXPscaler;
            }
        }

    }

    public void GetExp(float scoreForKill)
    {
        score += scoreForKill;
        expSlider.value += scoreForKill;
        //CurentExp es necesaria aqui ya que es posible que un enemigo de Exp por encima del valor maximo del slider, sin esta variable esa exp se pierde
        currentExp += scoreForKill;
        if (currentExp >= expForLevelUp)
        {
            //actualizamos la experiencia
            currentExp -= expForLevelUp;

            // Reproducir la animación de nivel
            SoundController.soundController.StartSound(SoundController.soundController.UISource, SoundController.soundController.levelUpSound);
            LevelUpImage.enabled = true;
            AnimationController.instance.PlayAnimation("LevelUp", animatorUI, 1.7f);
            Instantiate(levelUpPaticle, Player.Instance.transform.position, levelUpPaticle.transform.rotation);
            Player.Instance.Explode();

            // Invocar la función LevelUp después de un retraso
            Invoke("LevelUp", 0.5f);
        }

        scoreText.text = RoundValue(score).ToString();
    }

    public void GameOver()
    {
        paused = true;
        Time.timeScale = 0;
        DeadPanel.SetActive(true);
        scoreText.rectTransform.anchoredPosition = scoreDeathPos;
        SaveGame();

    }

    private void LevelUp()
    {
        print("LEVEL UP");
        level++;
        LevelUpImage.enabled = false;
        upgrading = true;
        levelText.text = level.ToString();
        expForLevelUp *= LevelUpExpNeededScaler;
        expSlider.maxValue = expForLevelUp;
        expSlider.value = currentExp;
        Time.timeScale = 0;
        upgradesPanel.SetActive(true);
        LeanTween.moveX(upgradesPanel.GetComponent<RectTransform>(), 0, 0.5f).setEase(LeanTweenType.easeInOutCirc);

        List<GameObject> upgradesObj = printUpgrades();

        // Inicia un contador para el retraso
        float delay = 0.7f; // Ajusta el valor del retraso según tus necesidades

        // Obtén la transformada del upgradeGrid para calcular las posiciones locales
        RectTransform upgradeGridTransform = upgradesGrid.GetComponent<RectTransform>();

        // Agrega un delay aquí antes de iniciar el bucle
        float initialDelay = 0.8f; // Cambia este valor al delay deseado antes de que comiencen los movimientos

        LeanTween.delayedCall(initialDelay, () =>
        {
            for (int i = 0; i < upgradesObj.Count; i++)
            {

                RectTransform upgradeObjTransform = upgradesObj[i].GetComponent<RectTransform>();
                RectTransform targetTransform = upgradeGridTransform.GetChild(i).GetComponent<RectTransform>();

                // Obtén las coordenadas locales de las posiciones de destino
                Vector3 targetLocalPosition = targetTransform.localPosition;

                // Mueve el objeto utilizando LeanTween a la posición de destino local
                LeanTween.moveLocalX(upgradeObjTransform.gameObject, targetLocalPosition.x, 0.6f)
                    .setEase(LeanTweenType.easeOutBack)
                    .setDelay(delay * i);
                upgradesObj[i].transform.SetParent(currentUpgradesGrid.transform);

            }

        });

    }

    public List<GameObject> printUpgrades()
    {
        print("printUpgrades");
        List<GameObject> upgrades = new List<GameObject>();
        if (upgradeData != null && upgradeData.Upgrades != null)
        {
            // Elegimos tres mejoras aleatorias
            List<Upgrade> mejorasAleatorias = GetRandomUpgrades(upgradeData.Upgrades, 3);

            for (int i = 0; i < mejorasAleatorias.Count; i++)
            {
                int index = i; // Crear una variable local index
                GameObject NewUpgrade = Instantiate(upgradeUI, GameObject.Find("Canvas").transform);
                NewUpgrade.GetComponent<RectTransform>().position = upgradesGrid.transform.GetChild(index).GetComponent<RectTransform>().position;
                Button newUpgradeButton = NewUpgrade.GetComponent<Button>();

                int Rarity = mejorasAleatorias[index].Rarity;

                NewUpgrade.GetComponent<Image>().sprite = Resources.Load<Sprite>("Sprites/Upgrades/Rarity/" + Rarity);
                NewUpgrade.GetComponent<UpgradeButtonController>().delay = 0.8f + 0.7f * i;

                foreach (Transform child in NewUpgrade.transform)
                {
                    if (child.name == "Name")
                    {
                        child.GetComponent<TextMeshProUGUI>().text = mejorasAleatorias[i].Name;
                    }
                    else if (child.name == "Description")
                    {
                        child.GetComponent<TextMeshProUGUI>().text = mejorasAleatorias[i].Description;
                    }
                    else if (child.name == "Effects")
                    {


                        foreach (var efectos in mejorasAleatorias[i].Effects)
                        {
                            // Cargar la imagen desde Resources
                            string efectoNombre = efectos.Key + " " + efectos.Value; // Asegúrate de que el nombre del efecto coincida con el nombre del archivo de imagen
                            Sprite efectoSprite = Resources.Load<Sprite>("Sprites/Upgrades/" + efectoNombre);

                            if (efectoSprite != null)
                            {
                                // Crea un GameObject para la imagen
                                GameObject imagenObjeto = new GameObject(efectoNombre);

                                imagenObjeto.transform.SetParent(child.transform);

                                if (Rarity == 5)
                                {
                                    Vector2 tamaño = new Vector2(259, 166);
                                    child.GetComponent<RectTransform>().sizeDelta = tamaño;

                                    Vector3 scale = new Vector3(3, 3, 3);

                                    imagenObjeto.transform.localScale = scale;

                                }
                                else
                                {

                                    imagenObjeto.transform.localScale = Vector3.one / 2;
                                }


                                // Agrega un componente Image y establece el sprite
                                Image imagen = imagenObjeto.AddComponent<Image>();
                                imagen.sprite = efectoSprite;


                            }
                            else
                            {
                                Debug.LogWarning("No se encontró la imagen para el efecto: " + efectoNombre);
                            }
                        }


                    }
                    else if (child.name == "Price")
                    {
                        if (mejorasAleatorias[i].Rarity != 5 && currentDifficultyLevel != 0)
                        {
                            for (int y = 0; y < currentDifficultyLevel; y++)
                            {
                                mejorasAleatorias[i].Price = (int)(mejorasAleatorias[i].Price * upgradePriceScaler);
                            }
                        }
                        child.GetComponent<TextMeshProUGUI>().text = mejorasAleatorias[i].Price.ToString();
                        if (Player.Instance.coins < mejorasAleatorias[i].Price)
                        {
                            child.GetComponent<TextMeshProUGUI>().color = Color.red;
                            newUpgradeButton.onClick.AddListener(() => CantGetUpgrade(newUpgradeButton.gameObject));
                        }
                        else
                        {
                            newUpgradeButton.onClick.AddListener(() => GiveUpgrade(mejorasAleatorias[index].Effects, mejorasAleatorias[index].Price, mejorasAleatorias[index], mejorasAleatorias[index].Rarity));
                        }


                    }
                }

                upgrades.Add(NewUpgrade);
            }


        }
        else
        {
            Debug.LogWarning("No se cargaron datos de mejoras desde el archivo JSON.");
        }
        return upgrades;
    }




    public void CantGetUpgrade(GameObject upgradeObj)
    {
        StartCoroutine(TremblingEffect(upgradeObj));
    }

    IEnumerator TremblingEffect(GameObject obj)
    {
        Vector3 originalPos = obj.transform.position;
        float elapsedTime = 0f;
        float duration = 0.6f; // Duración en segundos

        while (elapsedTime < duration)
        {
            float x = originalPos.x + Mathf.Sin(Time.unscaledTime * 30) * 10; // Ajusta la amplitud del temblor cambiando el valor 0.1f
            Vector3 newPos = new Vector3(x, originalPos.y, originalPos.z);
            obj.transform.position = newPos;

            elapsedTime += Time.unscaledDeltaTime;
            yield return null;
        }

        // Asegúrate de que el objeto vuelva a su posición original cuando termine el temblor
        obj.transform.position = originalPos;
    }




    private int GetRandomUniqueEnemyIndex()
    {
        List<GameObject> enemyIndices = difficultylevels[currentDifficultyLevel].Enemies;

        if (enemyIndices.Count == 0)
        {
            Debug.LogWarning("La lista de índices de enemigos está vacía.");
            return -1;
        }

        int randomIndex;

        do
        {
            randomIndex = Random.Range(0, enemyIndices.Count);
        }
        while (randomIndex == lastSpawnedEnemyIndex && enemyIndices.Count > 1);

        return randomIndex;
    }

    private void LoadUpgradesFromJSON(string archiveName)
    {
        string filePath = Application.dataPath + "/Resources/JSON/" + archiveName;

        if (File.Exists(filePath))
        {
            string json = File.ReadAllText(filePath);
            upgradeData = JsonConvert.DeserializeObject<UpgradeData>(json);
        }
        else
        {
            Debug.LogError("El archivo JSON de Upgrades no se encontró en la ruta: " + filePath);
            upgradeData = new UpgradeData();
        }
    }

    public List<Upgrade> GetRandomUpgrades(List<Upgrade> upgrades, int amount)
    {
        List<Upgrade> UpgradesSeleccted = new List<Upgrade>();

        // Verificamos que haya suficientes mejoras para seleccionar
        if (amount > upgrades.Count)
        {
            Debug.LogWarning("No hay suficientes mejoras disponibles.");
            return UpgradesSeleccted;
        }

        while (UpgradesSeleccted.Count < amount)
        {
            int indiceAleatorio = Random.Range(0, upgrades.Count);
            Upgrade mejoraAleatoria = upgrades[indiceAleatorio];

            // Verificamos que la mejora no se haya seleccionado previamente
            if (!UpgradesSeleccted.Contains(mejoraAleatoria))
            {
                UpgradesSeleccted.Add(mejoraAleatoria);
            }
        }

        return UpgradesSeleccted;
    }

    public void GiveUpgrade(Dictionary<string, int> effects, int price, Upgrade upgrade, int rarity)
    {
        foreach (Transform child in currentUpgradesGrid.transform)
        {
            Button button = child.GetComponent<Button>();
            if (button != null)
            {
                button.interactable = false;
            }
        }

        Player.Instance.GetUpgrade(effects);

        //Actualizamos las monedas
        UpdateCoins(-price);

        //Si es una mejora especial la eliminamos para que no vuelva a salir
        if (rarity == 5)
        {
            upgradeData.Upgrades.Remove(upgrade);
        }

        // Ejecuta la animación de movimiento con LeanTween
        EndUpgrade();
    }
    public void EndUpgrade()
    {
        LeanTween.moveX(upgradesPanel.GetComponent<RectTransform>(), 4000, 0.5f).setEase(LeanTweenType.easeInBack)
        .setOnComplete(() =>
        {
            // Esta parte del código se ejecutará cuando la animación haya terminado.
            // Mueve el panel a la posición -4000 en X
            upgradesPanel.GetComponent<RectTransform>().transform.localPosition = new Vector2(-4000, 0);
            upgradesPanel.SetActive(false);
            upgrading = false;
            Time.timeScale = 1f;
            // Destruye todos los hijos de upgradesGrid
            foreach (Transform child in currentUpgradesGrid.transform)
            {
                Destroy(child.gameObject);
            }
        });
    }
    public void UpdateCoins(int amount)
    {
        Player.Instance.coins += amount;
        CoinsText.text = Player.Instance.coins.ToString();
    }
    public void shakeCamera(float shakeIntensity, float shakeTime)
    {

        _cbmcp.m_AmplitudeGain = shakeIntensity;

        Invoke("StopShake", shakeTime);
    }

    void StopShake()
    {
        _cbmcp.m_AmplitudeGain = 0f;
    }

    void UpdateTimer()
    {
        if (!paused && !upgrading)
        {
            gameTimer += Time.deltaTime;
            timeForSpawn += Time.deltaTime;
        }
    }

    void FormatTimer()
    {
        // Calcula los minutos y segundos a partir del tiempo en segundos
        int minutes = Mathf.FloorToInt(gameTimer / 60);
        int seconds = Mathf.FloorToInt(gameTimer % 60);

        // Formatea el tiempo en minutos:segundos
        string timeString = string.Format("{0:00}:{1:00}", minutes, seconds);

        // Actualiza el componente de texto con el tiempo formateado
        timerText.text = timeString;
    }

    void DificultController()
    {
        if (gameTimer > 5)
            currentDifficultyLevel += 1;
        else
            currentDifficultyLevel = 0;
        elapsedTime = 0.0f;
        fillAmount = 1f;
        //Actualizamos lo que tardará la barra de progreso en reiniciarse
        fillDuration = difficultylevels[currentDifficultyLevel].endTime - difficultylevels[currentDifficultyLevel].startTime;

        if (currentDifficultyLevel < difficultylevels.Count - 1)
            difficultyText.text = (currentDifficultyLevel + 1).ToString();
        else
            difficultyText.text = "MAX";

        print(difficultylevels[currentDifficultyLevel].endTime);
    }

    private GameObject GetCanvasObjectUnderMouse()
    {
        PointerEventData eventData = new PointerEventData(EventSystem.current);
        eventData.position = Input.mousePosition;

        // Realiza un raycast desde la posición del ratón
        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventData, results);

        if (results.Count > 0)
        {
            // Encuentra el primer objeto que pertenezca a un Canvas
            foreach (RaycastResult result in results)
            {
                GameObject resultObject = result.gameObject;
                if (resultObject.layer == 11)
                {
                    return resultObject;
                }
            }
        }

        return null; // No se encontraron objetos del canvas bajo el ratón
    }

    private void SaveGame()
    {
        try
        {

            PlayerPrefs.SetFloat("volumeValue", SoundController.soundController.volumeSlider.value);

            int toggleState = SoundController.soundController.musicToggle.isOn ? 1 : 0;
            PlayerPrefs.SetInt("MuteMusic", toggleState);

            string bestScoreString = PlayerPrefs.GetString("bestScore");
            if (int.TryParse(bestScoreString, out int bestScore) || bestScoreString == "")
            {
                if (score > bestScore)
                {
                    PlayerPrefs.SetString("bestScore", score.ToString()); // Convierte 'score' a cadena
                    PlayerPrefs.SetString("bestGameTimer", timerText.text);
                }
            }
            else
            {
                Debug.Log("No podemos guardar los datos del score");
            }

            PlayerPrefs.Save();
            Debug.Log("GUARDADO!");
        }
        catch
        {
            Debug.Log("No podemos guardar los datos");
        }


    }
    public void loadGame()
    {
        bool savedToggleState = PlayerPrefs.GetInt("MuteMusic", 0) == 1;
        float savedSliderValue = PlayerPrefs.GetFloat("volumeValue", 0f);
        SoundController.soundController.volumeSlider.value = savedSliderValue;
        SoundController.soundController.musicToggle.isOn = savedToggleState;
    }
    public void Exit()
    {
        SaveGame();
        Application.Quit();
    }
    public void GotoMenu()
    {
        SaveGame();
        Pause();
        SceneManager.LoadScene("Menu");
    }
    public void LoadMap()
    {
        int map = Random.Range(0, Maps.Count);

        Instantiate(Maps[map]);

    }
    public void CicularDificultBar()
    {
        elapsedTime += Time.deltaTime;

        fillAmount = Mathf.Clamp01(elapsedTime / fillDuration);

        // Cambia el color de verde a rojo, pasando por amarillo y naranja
        Color color = Color.Lerp(Color.green, Color.red, fillAmount);
        progressBarImage.color = color;


        // Actualiza la imagen de la barra de carga
        progressBarImage.fillAmount = fillAmount;

        if (fillAmount >= 1f)
        {
            DificultController();
            // Cuando la barra se llena al máximo, reinicia el temporizador
            elapsedTime = 0.0f;
        }
    }
    public void UpdatestatsUI()
    {
        DmgText.text = Player.Instance.bulletDMGWithWeapon.ToString();
        ArmorText.text = Player.Instance.armor.ToString();
        CdText.text = RoundValue(Player.Instance.cooldownTimes["Shoot"]).ToString();
        BulletsText.text = Player.Instance.bulletCount.ToString();
        lifeText.text = RoundValue(lifeSlider.value) + "/" + RoundValue(lifeSlider.maxValue);
    }

    void IniciarTransicion()
    {
        tiempoInicio = Time.time;
        enTransicion = true;
    }

    void ActualizarTransicion()
    {
        float tiempoTranscurrido = Time.time - tiempoInicio;

        if (tiempoTranscurrido < duracionTransicion)
        {
            // Calcular el porcentaje de completitud de la transición
            float porcentajeCompletitud = tiempoTranscurrido / duracionTransicion;

            // Interpolar la posición y el tamaño ortográfico de manera gradual
            Vector3 nuevaPosicion = new Vector3(
                Mathf.Lerp(cinemachineVirtualCamera.transform.position.x, posicionFinal.x, porcentajeCompletitud),
                Mathf.Lerp(cinemachineVirtualCamera.transform.position.y, posicionFinal.y, porcentajeCompletitud),
                cinemachineVirtualCamera.transform.position.z
            );

            float nuevoOrthographicSize = Mathf.Lerp(cinemachineVirtualCamera.m_Lens.OrthographicSize, orthographicSizeFinal, porcentajeCompletitud);

            // Aplicar los cambios a la cámara
            cinemachineVirtualCamera.transform.position = nuevaPosicion;
            cinemachineVirtualCamera.m_Lens.OrthographicSize = nuevoOrthographicSize;
        }
        else
        {
            // La transición ha terminado
            enTransicion = false;
        }
    }
    float RoundValue(float value)
    {
        if (Mathf.Approximately(value, Mathf.Round(value)))
        {
            return Mathf.Round(value); // Valor entero, sin decimales
        }
        else
        {
            return Mathf.Round(value * 100.0f) / 100.0f; // Redondear a 2 decimales
        }
    }
    private void OnApplicationQuit()
    {
        SaveGame();
    }
}
