using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.SocialPlatforms.Impl;

public class MenuManager : MonoBehaviour
{
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI timeText;
    public List<GameObject> objectToSpawn;
    public Vector2 Min;
    public Vector2 Max;
    public float MinCd;
    public float MaxCd;
    public float MaxTorqueForce;

    private GameObject CurrentObj;
    private Rigidbody2D CurrentRb;
    private Vector3 offset;
    public float fuerzaMouse;
    public Texture2D customCursor;

    public void Start()
    {

        loadGameMenu();
        ObjectGenerator();
    }
    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            // Raycast para detectar objetos en la posición del ratón
            RaycastHit2D hit = Physics2D.Raycast(Camera.main.ScreenToWorldPoint(Input.mousePosition), Vector2.zero);

            if (hit.collider != null && hit.collider.gameObject.CompareTag("Draggable"))
            {
                CurrentObj = hit.collider.gameObject;
                offset = CurrentObj.transform.position - Camera.main.ScreenToWorldPoint(Input.mousePosition);
                CurrentRb = CurrentObj.GetComponent<Rigidbody2D>();
                CurrentRb.gravityScale = 0;
            }
        }

        if (Input.GetMouseButton(0) && CurrentObj != null)
        {
            // Actualizar la posición del objeto utilizando el Rigidbody
            Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            CurrentRb.MovePosition(new Vector2(mousePos.x + offset.x, mousePos.y + offset.y));

        }

        if (Input.GetMouseButtonUp(0))
        {
            // Soltar el objeto cuando se suelta el botón del ratón
            if(CurrentRb != null)
            {
                // Soltar el objeto y aplicar una fuerza basada en la velocidad del ratón
                Vector2 mouseVelocity = new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"));
                CurrentRb.velocity = mouseVelocity * fuerzaMouse;
                CurrentRb.gravityScale = 1;
                CurrentObj = null;
            }

            CurrentRb = null;
        }
    }

    public void Play()
    {
        SaveGameMenu();
        SceneManager.LoadScene("Game");
    }

    public void Exit()
    {
        SaveGameMenu();
        Application.Quit();
    }

    private void ObjectGenerator()
    {
        GameObject objetoAleatorio = objectToSpawn[Random.Range(0, objectToSpawn.Count)];
        float posX = Random.Range(Min.x, Max.x);
        float rotacionAleatoria = Random.Range(0f, 360f);
        // Crear el objeto en la posición generada
        GameObject newOBJ = Instantiate(objetoAleatorio, new Vector2(posX, Min.y), Quaternion.Euler(0f, 0f, rotacionAleatoria));

        // Obtener el componente Rigidbody2D del objeto
        Rigidbody2D rb = newOBJ.GetComponent<Rigidbody2D>();

        if (rb != null)
        {
            float torqueForce = Random.Range(0, MaxTorqueForce);
            // Aplicar torque al objeto
            rb.AddTorque(torqueForce);
        }

        float time = Random.Range(MinCd, MaxCd);
      
        Invoke("ObjectGenerator", time);

    }
    public void loadGameMenu()
    {
        bool savedToggleState = PlayerPrefs.GetInt("MuteMusic", 0) == 1;
        float savedSliderValue = PlayerPrefs.GetFloat("volumeValue", 0f);
        SoundController.soundController.volumeSlider.value = savedSliderValue;
        SoundController.soundController.musicToggle.isOn = savedToggleState;
        scoreText.text = PlayerPrefs.GetString("bestScore");
        timeText.text = PlayerPrefs.GetString("bestGameTimer");
    }
    private void SaveGameMenu()
    {
        PlayerPrefs.SetFloat("volumeValue", SoundController.soundController.volumeSlider.value);

        int toggleState = SoundController.soundController.musicToggle.isOn ? 1 : 0;
        PlayerPrefs.SetInt("MuteMusic", toggleState);

    }
    private void OnApplicationQuit()
    {
        SaveGameMenu();
    }
}
