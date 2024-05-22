using UnityEngine;

public class DialogueScript : MonoBehaviour
{
    private Animator animator;
    public string idleParameter = "Idle";
    public string talkingParameter = "Talk";

    // Reference to the AudioSource component in the ChatGPT object
    public AudioSource chatGPTAudioSource;

    void Start()
    {
        animator = GetComponentInParent<Animator>();
        if (animator == null)
        {
            Debug.LogError("Animator component not found on the parent GameObject.");
        }

        // Ensure the AudioSource component is assigned
        if (chatGPTAudioSource == null)
        {
            Debug.LogError("AudioSource component reference not assigned.");
        }
    }

    void Update()
    {
        // Check if the AudioSource in the ChatGPT object is playing
        if (chatGPTAudioSource != null && chatGPTAudioSource.isPlaying)
        {
            // Trigger the talking animation while the AudioSource is playing
            animator.SetBool(idleParameter, false);
            animator.SetBool(talkingParameter, true);
        }
        else
        {
            // Return to idle animation when the AudioSource finishes playing
            animator.SetBool(idleParameter, true);
            animator.SetBool(talkingParameter, false);
        }
    }
}
