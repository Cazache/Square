using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    public ParticleSystem destroyParticlePrefab;
    public float bulletDmg = 1;
    void getParticleColor()
    {
        Color balaColor = GetComponent<SpriteRenderer>().color;
        ParticleSystem destroyParticle = Instantiate(destroyParticlePrefab, transform.position, Quaternion.identity);
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
    private void OnCollisionEnter2D(Collision2D collision)
    {
        getParticleColor();

        if (gameObject.layer == LayerMask.NameToLayer("PlayerBullet") && collision.gameObject.layer == LayerMask.NameToLayer("Enemy"))
        {
            collision.gameObject.GetComponent<EnemyController>().gethit(bulletDmg);
        }
        else if (gameObject.layer == LayerMask.NameToLayer("EnemyBullet") && collision.gameObject.layer == LayerMask.NameToLayer("Player"))
        {

            Player.Instance.gethit(bulletDmg);
        }

        if (gameObject.layer == LayerMask.NameToLayer("PlayerBullet") && collision.gameObject.layer != LayerMask.NameToLayer("Player")
           || gameObject.layer == LayerMask.NameToLayer("EnemyBullet") && collision.gameObject.layer != LayerMask.NameToLayer("Enemy") 
           || collision.gameObject.layer != LayerMask.NameToLayer("Enemy") && collision.gameObject.layer != LayerMask.NameToLayer("Player"))
        {
            Destroy(gameObject);
        }
            
    }
}
