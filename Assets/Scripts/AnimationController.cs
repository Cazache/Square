using UnityEngine;

public class AnimationController : MonoBehaviour
{
    public static AnimationController instance;

    private void Awake()
    {
        instance = this;
    }

    // Función para iniciar una animación
    public void PlayAnimation(string animationName, Animator animator, float animationSpeed)
    {
        if (animator != null)
        {
            animator.speed = animationSpeed; // Establecer la velocidad de reproducción
            animator.Play(animationName); // Iniciar la animación por su nombre
        }
    }

    // Función para detener una animación
    public void StopAnimation(Animator animator)
    {
        if (animator != null)
        {
            animator.speed = 0f; // Detener la animación estableciendo la velocidad a 0
        }
    }
}
