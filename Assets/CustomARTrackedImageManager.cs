using OpenAI;
using UnityEngine;
using UnityEngine.XR.ARFoundation;

public class ImageActivation : MonoBehaviour
{
    public ARTrackedImageManager imageManager;
    public GameObject prefabToActivate;


    private void OnEnable()
    {
        imageManager.trackedImagesChanged += OnTrackedImagesChanged;
    }

    private void OnDisable()
    {
        imageManager.trackedImagesChanged -= OnTrackedImagesChanged;
    }

    private void OnTrackedImagesChanged(ARTrackedImagesChangedEventArgs eventArgs)
    {
        foreach (var addedImage in eventArgs.added)
        {
            // Activate prefab only when a new image is added
            if (addedImage.referenceImage != null)
            {
                prefabToActivate.SetActive(true);


            }
        }
    }
}