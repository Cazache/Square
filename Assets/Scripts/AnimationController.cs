using UnityEngine;

public class AnimationController : MonoBehaviour
{
    public static AnimationController instance;

    private void Awake()
    {
        instance = this;
    }

    // Funci�n para iniciar una animaci�n
    public void PlayAnimation(string animationName, Animator animator, float animationSpeed)
    {
        if (animator != null)
        {
            animator.speed = animationSpeed; // Establecer la velocidad de reproducci�n
            animator.Play(animationName); // Iniciar la animaci�n por su nombre
        }
    }

    // Funci�n para detener una animaci�n
    public void StopAnimation(Animator animator)
    {
        if (animator != null)
        {
            animator.speed = 0f; // Detener la animaci�n estableciendo la velocidad a 0
        }
    }
}
