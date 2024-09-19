using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UpgradeButtonController : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    private Vector3 originalScale; // Almacenamos la escala original del objeto aquí.
    AudioSource upgradeSource;
    public AudioClip printUpgradeSound;
    public float delay;
    private float elapsedTime;
    bool sound;
    private void Start()
    {
        sound = true;
        upgradeSource = GetComponent<AudioSource>();
        originalScale = transform.localScale;

        elapsedTime = 0f;

        Button buttonComponent = GetComponent<Button>();

        // Verificar si existe el componente Button
        if (buttonComponent != null)
        {
            // Agregar la función al evento OnClick
            buttonComponent.onClick.AddListener(OnClickHandler);
        }
    }
    private void Update()
    {
        upgradeSoundController();
    }
    public void upgradeSoundController()
    {
        if (elapsedTime < delay)
        {
            elapsedTime += Time.unscaledDeltaTime; // Incrementa el tiempo sin tener en cuenta Time.timeScale
        }
        else
        {
            if (sound)
            {
                dashSound();
                sound = false;
            }
        }
    }
    public void OnPointerEnter(PointerEventData eventData)
    {
        // Cuando el puntero entra en el objeto, aumentamos la escala por 1.5.
        transform.localScale = originalScale * 1.3f;
        SoundController.soundController.Selectedbutton();

    }

    public void OnPointerExit(PointerEventData eventData)
    {

        // Cuando el puntero sale del objeto, restauramos la escala original.
        transform.localScale = originalScale;

    }
    private void dashSound()
    {
        SoundController.soundController.StartSound(upgradeSource, printUpgradeSound);
    }
    private void OnClickHandler()
    {
        SoundController.soundController.Clickbutton();
    }

}
