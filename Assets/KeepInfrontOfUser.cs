using UnityEngine;

public class KeepInfrontOfUser : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        //associate this object with the user's camera
        Camera userCamera = Camera.main;
        if (userCamera != null)
        {
            transform.position = userCamera.transform.position + userCamera.transform.forward * 2.0f; // Position it 2 units in front of the camera
            transform.rotation = userCamera.transform.rotation; // Match the camera's rotation
        }
        else
        {
            Debug.LogWarning("No main camera found. KeepInfrontOfUser script requires a camera tagged as 'MainCamera'.");
        }
    }

    // Update is called once per frame
    void Update()
    {
        // Continuously update the position to keep it in front of the user
        Camera userCamera = Camera.main;
        if (userCamera != null)
        {
            transform.position = userCamera.transform.position + userCamera.transform.forward * 3.0f; // Keep it 2 units in front of the camera
            transform.rotation = userCamera.transform.rotation; // Keep the rotation aligned with the camera
        }
        else
        {
            Debug.LogWarning("No main camera found. KeepInfrontOfUser script requires a camera tagged as 'MainCamera'.");
        }
        
    }
}
