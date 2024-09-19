using UnityEngine;
using UnityEngine.UI;

public class GunController : MonoBehaviour
{
    public Transform shootPoint; // Asigna el objeto "ShootPoint" desde el Inspector
    public SpriteRenderer childObject;
    public Image uiWeapon;// Asigna el objeto hijo que contiene el componente SpriteRenderer

    void Update()
    {
        Vector3 direction = shootPoint.position - transform.position;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle);


        if (childObject != null)
        {
            childObject.flipY = (angle >= 90 || angle <= -90);
        }
    }

    public void changeSprite(string weaponName)
    {
        Sprite newSprite = Resources.Load<Sprite>("Sprites/Weapons/" + weaponName);

        childObject.sprite = newSprite;

    }

}

