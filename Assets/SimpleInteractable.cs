using UnityEngine;

public class SimpleInteractable : MonoBehaviour, IHoverable, ISelectable
{
    public Material defaultMaterial;
    public Material hoveredMaterial;
    public Material selectedMaterial;

    [Header("Spawn Settings")]
    public GameObject prefabToSpawn; // Drag your prefab here in the Inspector
    public float spawnOffsetFromSurface = 0.01f; // Small offset to avoid Z-fighting

    private MeshRenderer _meshRenderer;
    private GameObject _selectedObjectRef; // To check if THIS object is selected

    void Awake()
    {
        _meshRenderer = GetComponent<MeshRenderer>();
        if (_meshRenderer == null) Debug.LogWarning("No MeshRenderer found on SimpleInteractable.", this);
        SetMaterial(defaultMaterial);
    }

    void SetMaterial(Material mat)
    {
        if (_meshRenderer != null && mat != null) _meshRenderer.material = mat;
    }

    // IHoverable Methods
    public void OnHoverEnter(RaycastHit hit) // Now receives RaycastHit
    {
        if (_selectedObjectRef == null) // Only change material if not already selected
            SetMaterial(hoveredMaterial);
        Debug.Log(gameObject.name + " Hovered! Hit at: " + hit.point);
        // You could also spawn a temporary visual indicator here based on hit.point
    }

    public void OnHoverExit()
    {
        if (_selectedObjectRef == null) // Only revert if not selected
            SetMaterial(defaultMaterial);
        Debug.Log(gameObject.name + " Hover Exited!");
    }

    // ISelectable Methods
    public void OnSelectEnter(RaycastHit hit) // Now receives RaycastHit
    {
        _selectedObjectRef = this.gameObject; // Mark this object as selected
        SetMaterial(selectedMaterial);
        Debug.Log(gameObject.name + " Selected! Performing action...");

        // --- SPAWN PREFAB HERE ---
        if (prefabToSpawn != null)
        {
            // Calculate spawn position slightly offset from the surface
            Vector3 spawnPosition = hit.point + hit.normal * spawnOffsetFromSurface;

            // Calculate spawn rotation:
            // Quaternion.LookRotation(hit.normal) would make the prefab's Z-axis point along the normal.
            // If your prefab's "up" is its Y-axis, and you want it to stand on the surface,
            // you'll want its Y-axis to align with the hit.normal.
            // Quaternion.FromToRotation(Vector3.up, hit.normal) creates a rotation from Vector3.up to hit.normal.
            // If you want it to also align with the controller's forward:
            // Quaternion spawnRotation = Quaternion.LookRotation(_controllerTransform.forward, hit.normal);
            // For a simple object placed on the surface, `FromToRotation` is often best.
            Quaternion spawnRotation = Quaternion.FromToRotation(Vector3.up, hit.normal);

            // If you want the spawned object to face *out* from the surface,
            // but also align its "forward" (Z-axis) with some other direction,
            // e.g., the controller's forward projected onto the plane:
            // Vector3 projectedForward = Vector3.ProjectOnPlane(_controllerTransform.forward, hit.normal);
            // Quaternion spawnRotation = Quaternion.LookRotation(projectedForward, hit.normal);

            // Make sure that the prefab is looking at the main camera or the controller's forward direction. Only change the y-axis.
            float y_rotation = Camera.main.transform.eulerAngles.y; // Get the camera's Y rotation
            Quaternion spawnRotationModified = Quaternion.Euler(0, y_rotation, 0);

            GameObject spawnedObject = Instantiate(prefabToSpawn, spawnPosition, spawnRotationModified);
            Debug.Log($"Spawned {prefabToSpawn.name} at {spawnPosition} with rotation {spawnRotation.eulerAngles}");

            // Optional: You could pass context to the spawned object,
            // e.g., if it needs to know which controller spawned it.
            // spawnedObject.GetComponent<MySpawnedObjectScript>()?.Initialize(this);
        }
        else
        {
            Debug.LogWarning("Prefab to Spawn is not assigned in SimpleInteractable on " + gameObject.name, this);
        }
    }

    public void OnSelectExit()
    {
        _selectedObjectRef = null; // Mark as deselected
        SetMaterial(defaultMaterial);
        Debug.Log(gameObject.name + " Deselected!");
    }
}