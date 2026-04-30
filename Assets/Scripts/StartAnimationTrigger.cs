using UnityEngine;

public class StartAnimationTrigger : MonoBehaviour
{
    public Animator anim;
    public bool soundFile;
    public AudioSource audioSource;
    public bool destroy;
    void OnTriggerEnter(Collider other)
    {
        anim.enabled = true;
        if (soundFile) audioSource.Play();
        if (destroy) Destroy(gameObject);
    }
}
