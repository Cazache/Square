
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class SoundController : MonoBehaviour
{
    public static SoundController soundController;


    public AudioSource StartMusicsource;
    public AudioSource mainMusicSource;
    public AudioSource UISource;
    public AudioClip startTheme;
    public AudioClip mainTheme;
    public AudioClip selectButton;
    public AudioClip clickSound;
    public AudioClip levelUpSound;

    private bool startThemeFinished = false;
    public AudioMixer mixerGroup;


    // Rango mínimo y máximo del slider y el volumen del mixer
    public float minSliderValue = 0.0f;
    public float maxSliderValue = 1.0f;
    public float minVolume = -80.0f; // El volumen mínimo que acepta AudioMixerGroup
    public float maxVolume = 20.0f; // El volumen máximo que acepta AudioMixerGroup

    public Toggle musicToggle;
    public Slider volumeSlider;
    // Start is called before the first frame update
    private void Awake()
    {
        soundController = this;
    }
    void Start()
    {
        muteMusic(musicToggle.isOn);
        SetVolume(volumeSlider.value);
        if(StartMusicsource != null)
        {
            StartMusicsource.clip = startTheme;
            StartMusicsource.Play();
        }
    }

    // Update is called once per frame
    void Update()
    {
 
        Scene currentScene = SceneManager.GetActiveScene();
      
        // Verifica si la escena actual es la escena "Game"
        if (currentScene.name == "Game")
        {
            // Llama a la función solo si estás en la escena "Game"
            startMainMusic();
        }
    }

    public void startMainMusic()
    {

        // Verificar si el clip startTheme ha terminado
        if (!startThemeFinished && StartMusicsource.time >= 31.99f)
        {
            startThemeFinished = true;
            StartSound(mainMusicSource, mainTheme, true, 0.4f);
        }
    }
    // Método para iniciar la reproducción de un AudioSource específico
    public void StartSound(AudioSource audioSource, AudioClip clip, bool loop = false, float volume = 1)
    {
        audioSource.clip = clip;
        audioSource.loop = loop;
        audioSource.volume = volume;
        audioSource.Play();
    }

    // Método para detener la reproducción de un AudioSource específico
    public void StopSound(AudioSource audioSource)
    {
        audioSource.Stop();
    }



    // Método llamado cuando el valor del slider cambia
    public void SetVolume(float sliderValue)
    {
        mixerGroup.SetFloat("MasterVolume", Mathf.Log10(sliderValue) * 20); // Reemplaza "Volumen" con el nombre del parámetro de volumen en tu AudioMixer
    }
    public void muteMusic(bool mute)
    {
        if(StartMusicsource != null)
        StartMusicsource.mute = !mute;

        if (mainMusicSource != null)
            mainMusicSource.mute = !mute;
    }
    public void Selectedbutton()
    {
        StartSound(UISource, selectButton);
    }
    public void Clickbutton()
    {
        StartSound(UISource, clickSound);
    }
}

