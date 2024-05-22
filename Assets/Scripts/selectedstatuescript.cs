using UnityEngine;
using UnityEngine.UI;

public class selectedstatuescript : MonoBehaviour
{
    public StatuesManager statuesManager; // Reference to the StatuesManager script
    public Text SelectedStaueText;

    // Start is called before the first frame update
    void Start()
    {
        // Accessing the StatuesManager script
        if (statuesManager != null)
        {
            // Accessing the SelectedStatue variable from StatuesManager
            string selectedStatue = statuesManager.SelectedStatue;
            SelectedStaueText.text = selectedStatue;
            Debug.Log("Selected Statue: " + selectedStatue);
        }
        else
        {
            Debug.LogError("StatuesManager script is not assigned.");
        }
    }

    // Update is called once per frame
    void Update()
    {

    }
}
