using UnityEngine;
using UnityEngine.InputSystem;
// No UnityEngine.XR.Interaction.Toolkit if you're not using XRIT's interactors

public class OVRRaycastAndOrientation : MonoBehaviour
{
    [Header("OVR Camera Rig References")]
    public OVRCameraRig ovrCameraRig; // Drag your OVRCameraRig here from the scene
    public bool useRightHand = true; // Toggle between right and left hand for interaction

    private Transform _controllerTransform; // The active controller transform

    [Header("Raycast Properties")]
    public float raycastDistance = 100f;
    public LayerMask interactableLayers; // Define what layers your ray can hit

    [Header("Ray Visuals")]
    public LineRenderer rayLineRenderer; // Assign a LineRenderer component
    public Transform hitPointIndicator; // Optional: a small sphere or cube to show hit point

    [Header("Input Actions")]
    public InputActionReference selectAction; // Link to your "Select" Input Action
    public InputActionReference rotateAction; // Link to your "Rotate" Input Action (Vector2)

    private GameObject _currentHoveredObject;
    private GameObject _selectedObject; // The object currently being oriented/manipulated

    // Store the last hit for spawning purposes
    private RaycastHit _lastHit;
    private bool _hasHitLastFrame = false;

    void Start()
    {
        if (ovrCameraRig == null)
        {
            Debug.LogError("OVRCameraRig not assigned! Please assign it in the Inspector.", this);
            enabled = false;
            return;
        }

        // Determine which hand's transform to use
        _controllerTransform = useRightHand ? ovrCameraRig.rightHandAnchor : ovrCameraRig.leftHandAnchor;

        if (_controllerTransform == null)
        {
            Debug.LogError("Controller anchor not found on OVRCameraRig. Make sure RightHandAnchor/LeftHandAnchor exist.", this);
            enabled = false;
            return;
        }

        if (rayLineRenderer != null)
        {
            rayLineRenderer.positionCount = 2;
        }
        else
        {
            Debug.LogWarning("Ray Line Renderer not assigned. Ray visual will not be displayed.", this);
        }

        if (hitPointIndicator != null)
        {
            hitPointIndicator.gameObject.SetActive(false); // Start hidden
        }
    }

    void OnEnable()
    {
        if (selectAction != null)
        {
            selectAction.action.Enable();
            selectAction.action.performed += OnSelectPerformed;
            selectAction.action.canceled += OnSelectCanceled;
        }
        if (rotateAction != null)
        {
            rotateAction.action.Enable();
            rotateAction.action.performed += OnRotatePerformed;
            rotateAction.action.canceled += OnRotateCanceled;
        }
    }

    void OnDisable()
    {
        if (selectAction != null)
        {
            selectAction.action.performed -= OnSelectPerformed;
            selectAction.action.canceled -= OnSelectCanceled;
            selectAction.action.Disable();
        }
        if (rotateAction != null)
        {
            rotateAction.action.performed -= OnRotatePerformed;
            rotateAction.action.canceled -= OnRotateCanceled;
            rotateAction.action.Disable();
        }
    }

    void Update()
    {
        if (_controllerTransform == null) return;

        PerformRaycast();

        // If an object is selected, apply rotation based on input
        if (_selectedObject != null && rotateAction != null && rotateAction.action.enabled)
        {
            Vector2 rotateInput = rotateAction.action.ReadValue<Vector2>();
            if (rotateInput.magnitude > 0.1f) // Deadzone for thumbstick
            {
                _selectedObject.transform.Rotate(Vector3.up, rotateInput.x * Time.deltaTime * 100f, Space.World);
                _selectedObject.transform.Rotate(_selectedObject.transform.right, rotateInput.y * Time.deltaTime * 100f, Space.World);
            }
        }
    }

    private void PerformRaycast()
    {
        RaycastHit hit;
        Ray ray = new Ray(_controllerTransform.position, _controllerTransform.forward);

        _hasHitLastFrame = false; // Reset for current frame

        if (Physics.Raycast(ray, out hit, raycastDistance, interactableLayers))
        {
            _hasHitLastFrame = true;
            _lastHit = hit; // Store the hit information

            Debug.DrawRay(ray.origin, ray.direction * hit.distance, Color.green);
            UpdateRayVisuals(ray.origin, hit.point, true);

            if (hitPointIndicator != null)
            {
                hitPointIndicator.position = hit.point;
                hitPointIndicator.gameObject.SetActive(true);
            }

            // Handle hover logic
            if (_currentHoveredObject != hit.collider.gameObject)
            {
                // Call hover exit on previous, enter on new
                if (_currentHoveredObject != null && _currentHoveredObject.TryGetComponent<IHoverable>(out var prevHoverable))
                {
                    prevHoverable.OnHoverExit();
                }
                _currentHoveredObject = hit.collider.gameObject;
                if (_currentHoveredObject != null && _currentHoveredObject.TryGetComponent<IHoverable>(out var newHoverable))
                {
                    newHoverable.OnHoverEnter(hit); // Pass the hit data!
                }
            }
        }
        else
        {
            // Ray hit nothing
            Debug.DrawRay(ray.origin, ray.direction * raycastDistance, Color.red);
            UpdateRayVisuals(ray.origin, ray.origin + ray.direction * raycastDistance, false);

            if (hitPointIndicator != null)
            {
                hitPointIndicator.gameObject.SetActive(false);
            }

            // Handle hover exit for previously hovered object
            if (_currentHoveredObject != null)
            {
                if (_currentHoveredObject.TryGetComponent<IHoverable>(out var prevHoverable))
                {
                    prevHoverable.OnHoverExit();
                }
                _currentHoveredObject = null;
            }
        }
    }

    private void UpdateRayVisuals(Vector3 startPoint, Vector3 endPoint, bool hit)
    {
        if (rayLineRenderer != null)
        {
            rayLineRenderer.enabled = true;
            rayLineRenderer.SetPosition(0, startPoint);
            rayLineRenderer.SetPosition(1, endPoint);
        }
    }

    private void OnSelectPerformed(InputAction.CallbackContext context)
    {
        // Check if we hit something last frame
        if (_hasHitLastFrame && _currentHoveredObject != null)
        {
            _selectedObject = _currentHoveredObject; // Now 'selectedObject' could be null if no hover
            Debug.Log($"Selected: {_selectedObject.name}");
            if (_selectedObject.TryGetComponent<ISelectable>(out var selectable))
            {
                // Pass the full RaycastHit data when selecting
                selectable.OnSelectEnter(_lastHit);
            }
        }
        else
        {
            // If no object was hit, but the trigger was pulled,
            // you might still want to spawn something in front of the ray,
            // or just not do anything. This depends on your game's logic.
            // Example: Spawn a default object 5 units in front of the controller
            // if nothing specific was hit.
            Debug.Log("Select performed, but no object hit or selected.");
            // Example of spawning if nothing was hit:
            // if (prefabToSpawnIfNoHit != null) {
            //     Vector3 spawnPos = _controllerTransform.position + _controllerTransform.forward * 5f;
            //     Instantiate(prefabToSpawnIfNoHit, spawnPos, _controllerTransform.rotation);
            // }
        }
    }

    private void OnSelectCanceled(InputAction.CallbackContext context)
    {
        if (_selectedObject != null)
        {
            Debug.Log($"Deselected: {_selectedObject.name}");
            if (_selectedObject.TryGetComponent<ISelectable>(out var selectable))
            {
                selectable.OnSelectExit();
            }
            _selectedObject = null;
        }
    }

    private void OnRotatePerformed(InputAction.CallbackContext context)
    {
        // Continuous rotation handled in Update
    }

    private void OnRotateCanceled(InputAction.CallbackContext context)
    {
        // Stop rotating
    }
}

// Optional: Define interfaces for better organization and extensibility
// Modified to pass RaycastHit context
public interface IHoverable
{
    void OnHoverEnter(RaycastHit hit); // Pass hit info on hover enter
    void OnHoverExit();
}

public interface ISelectable
{
    void OnSelectEnter(RaycastHit hit); // Pass hit info on select enter
    void OnSelectExit();
}