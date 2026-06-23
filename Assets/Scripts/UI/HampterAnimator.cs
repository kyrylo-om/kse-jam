using UnityEngine;

public class HampterAnimator : MonoBehaviour
{
    [Header("Animator")]
    [SerializeField] private Animator animator;

    [Header("Animation State Names")]
    [SerializeField] private string[] idleAnimations;
    [SerializeField] private string[] talkAnimations;
    [SerializeField] private string tapedAnimation = "Taped";
    [SerializeField] private string deathAnimation = "Lose";
    [SerializeField] private AudioSource tapeSound;

    private string currentAnimation;

    public void StartIdle()
    {
        PlayRandom(idleAnimations);
    }

    public void StartTalk()
    {
        PlayRandom(talkAnimations);
    }

    public void StartDeath()
    {
        animator.Play(deathAnimation);
    }

    public void StartTaped()
    {
        tapeSound.Play();
        animator.Play(tapedAnimation);
    }

    private void PlayRandom(string[] animations)
    {
        if (animations == null || animations.Length == 0)
            return;

        string animationToPlay;

        do
        {
            animationToPlay =
                animations[Random.Range(0, animations.Length)];
        }
        while (animations.Length > 1 &&
            animationToPlay == currentAnimation);

        currentAnimation = animationToPlay;
        animator.Play(animationToPlay);
    }
}
