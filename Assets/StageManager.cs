using UnityEngine;
using UnityEngine.InputSystem;

public class StageManager : MonoBehaviour, Team3_inputs.IPlayerActions
{
    [Header("Light and Audio References")]
    [SerializeField] private GameObject lightGameObject;
    [SerializeField] private AudioSource audioSource;
    
    [Header("Light Settings")]
    [SerializeField] private bool lightsAreOn = false;
    
    private Team3_inputs inputActions;
    
    void Awake()
    {
        // Initialize input actions
        inputActions = new Team3_inputs();
    }
    
    void OnEnable()
    {
        // Enable input actions and subscribe to callbacks
        inputActions.Player.AddCallbacks(this);
        inputActions.Player.Enable();
    }
    
    void OnDisable()
    {
        // Disable input actions and unsubscribe from callbacks
        inputActions.Player.RemoveCallbacks(this);
        inputActions.Player.Disable();
    }
    
    void OnDestroy()
    {
        // Clean up input actions
        inputActions?.Dispose();
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // Ensure components are assigned
        if (lightGameObject == null)
        {
            Debug.LogWarning("StageManager: Light GameObject is not assigned!");
        }
        
        if (audioSource == null)
        {
            Debug.LogWarning("StageManager: Audio Source is not assigned!");
        }
        
        // Set initial state
        UpdateLightAndAudio();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    
    private void ToggleLights()
    {
        lightsAreOn = !lightsAreOn;
        UpdateLightAndAudio();
    }
    
    private void UpdateLightAndAudio()
    {
        // Toggle light game object
        if (lightGameObject != null)
        {
            lightGameObject.SetActive(lightsAreOn);
        }
        
        // Control audio source
        if (audioSource != null)
        {
            if (lightsAreOn)
            {
                if (!audioSource.isPlaying)
                {
                    audioSource.Play();
                }
            }
            else
            {
                if (audioSource.isPlaying)
                {
                    audioSource.Stop();
                }
            }
        }
        
        Debug.Log($"Lights are now: {(lightsAreOn ? "ON" : "OFF")}");
    }
    
    // Implementation of IPlayerActions interface
    public void OnSelect(InputAction.CallbackContext context)
    {
        // Handle select input if needed
    }

    public void OnRotate(InputAction.CallbackContext context)
    {
        // Handle rotate input if needed
    }

    public void OnLights(InputAction.CallbackContext context)
    {
        // Handle lights input - toggle when button is pressed (performed phase)
        if (context.performed)
        {
            ToggleLights();
        }
    }
}
