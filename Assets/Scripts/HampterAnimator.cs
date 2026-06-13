using UnityEngine;

public class HampterAnimator : MonoBehaviour
{
    [Header("Animator")]
    [SerializeField] private Animator animator;

    [Header("Animation State Names")]
    [SerializeField] private string[] idleAnimations;
    [SerializeField] private string[] talkAnimations;

    private string currentAnimation;

    public void StartIdle()
    {
        PlayRandom(idleAnimations);
    }

    public void StartTalk()
    {
        PlayRandom(talkAnimations);
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
