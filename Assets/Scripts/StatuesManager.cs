using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StatuesManager : MonoBehaviour
{
    [SerializeField] private GameObject[] statues;
    private Animator[] animators;
    public string SelectedStatue = "";
    public Text UItext;

    private void Start()
    {
        // Get the animators from each statue
        animators = new Animator[statues.Length];
        for (int i = 0; i < statues.Length; i++)
        {
            animators[i] = statues[i].GetComponent<Animator>();
            animators[i].enabled = false; // Disable the animator initially
        }
    }

    private void Update()
    {
        // Check for touch input
        if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)
        {
            // Raycast to detect if any statue is touched
            Ray ray = Camera.main.ScreenPointToRay(Input.GetTouch(0).position);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit))
            {
                // Check if the hit object is one of the statues
                for (int i = 0; i < statues.Length; i++)
                {
                    if (hit.collider.gameObject == statues[i])
                    {
                        SelectedStatue = statues[i].name;
                        UItext.text = "Selected Statue : " + statues[i].name;
                        // Enable the animator for the touched statue
                        StartCoroutine(ActivateAnimatorForDuration(animators[i], 10f));
                        break; // Exit loop after finding the touched statue
                    }
                }
            }
        }
    }

    private IEnumerator ActivateAnimatorForDuration(Animator animator, float duration)
    {
        // Enable the animator
        animator.enabled = true;

        // Wait for the specified duration
        yield return new WaitForSeconds(duration);

        // Disable the animator after the duration
        animator.enabled = false;
    }
}
