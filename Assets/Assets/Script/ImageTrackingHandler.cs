using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

/// <summary>
/// Spawns a 3D model on a detected reference image, keeps it aligned with the
/// tracked image, and handles tracking lost / tracking regained.
/// Attach to the XR Origin (same object as ARTrackedImageManager).
/// Written for AR Foundation 6.x (Unity 6).
/// </summary>
[RequireComponent(typeof(ARTrackedImageManager))]
public class ImageTrackingHandler : MonoBehaviour
{
    [Header("Model")]
    [Tooltip("Prefab to spawn on top of the detected image.")]
    [SerializeField] private GameObject modelPrefab;

    [Tooltip("Scale the model relative to the physical width of the image. " +
             "With this on, a prefab at scale 1 (about 1 m wide) becomes as wide as the image.")]
    [SerializeField] private bool scaleToImageSize = true;

    [Tooltip("Extra size multiplier. 1 = same width as the image, 0.5 = half, and so on.")]
    [SerializeField] private float sizeMultiplier = 0.8f;

    [Header("UI (optional)")]
    [SerializeField] private TMP_Text statusText;
    [SerializeField] private string searchingMessage = "Point the camera at the marker";
    [SerializeField] private string trackingMessage = "Tracking: {0}";
    [SerializeField] private string lostMessage = "Tracking lost - point the camera back at the marker";

    private ARTrackedImageManager trackedImageManager;

    // One spawned model per tracked image
    private readonly Dictionary<TrackableId, GameObject> spawnedModels = new();
    // Remembers last known state so we can tell "lost" from "regained"
    private readonly Dictionary<TrackableId, bool> wasTracking = new();

    private void Awake()
    {
        trackedImageManager = GetComponent<ARTrackedImageManager>();
        SetStatus(searchingMessage);
    }

    private void OnEnable()
    {
        trackedImageManager.trackablesChanged.AddListener(OnTrackablesChanged);
    }

    private void OnDisable()
    {
        trackedImageManager.trackablesChanged.RemoveListener(OnTrackablesChanged);
    }

    private void OnTrackablesChanged(ARTrackablesChangedEventArgs<ARTrackedImage> args)
    {
        // Image detected for the first time -> spawn the model
        foreach (ARTrackedImage image in args.added)
        {
            SpawnModel(image);
        }

        // Image moved or its tracking state changed -> update visibility
        foreach (ARTrackedImage image in args.updated)
        {
            RefreshModel(image);
        }

        // Image removed by the subsystem -> clean up
        foreach (var pair in args.removed)
        {
            RemoveModel(pair.Key);
        }
    }

    private void SpawnModel(ARTrackedImage image)
    {
        if (modelPrefab == null)
        {
            Debug.LogWarning("ImageTrackingHandler: Model Prefab is not assigned.");
            return;
        }

        if (spawnedModels.ContainsKey(image.trackableId))
        {
            RefreshModel(image);
            return;
        }

        // Parenting to the tracked image keeps the model aligned with it automatically.
        // AR Foundation updates image.transform every frame while the image is tracked.
        GameObject model = Instantiate(modelPrefab, image.transform);
        model.transform.localPosition = Vector3.zero;
        model.transform.localRotation = Quaternion.identity;

        if (scaleToImageSize && image.size.x > 0f)
        {
            // image.size is the physical size in meters
            model.transform.localScale = modelPrefab.transform.localScale * (image.size.x * sizeMultiplier);
        }
        else
        {
            model.transform.localScale = modelPrefab.transform.localScale;
        }

        spawnedModels[image.trackableId] = model;
        wasTracking[image.trackableId] = false; // so the first Tracking state logs as detected

        Debug.Log($"Image detected: {image.referenceImage.name}. Model spawned.");
        RefreshModel(image);
    }

    private void RefreshModel(ARTrackedImage image)
    {
        if (!spawnedModels.TryGetValue(image.trackableId, out GameObject model)) return;

        // Tracking = image is visible and its pose is being updated.
        // Limited / None = image is not currently visible (or pose is unreliable).
        bool isTracking = image.trackingState == TrackingState.Tracking;
        bool previously = wasTracking.TryGetValue(image.trackableId, out bool prev) && prev;

        // Show the model only while the image is really tracked,
        // so it never floats at a stale position.
        model.SetActive(isTracking);

        if (isTracking && !previously)
        {
            Debug.Log($"Tracking regained: {image.referenceImage.name}");
            SetStatus(string.Format(trackingMessage, image.referenceImage.name));
        }
        else if (!isTracking && previously)
        {
            Debug.Log($"Tracking lost: {image.referenceImage.name}");
            SetStatus(lostMessage);
        }

        wasTracking[image.trackableId] = isTracking;
    }

    private void RemoveModel(TrackableId id)
    {
        if (spawnedModels.TryGetValue(id, out GameObject model))
        {
            if (model != null) Destroy(model);
            spawnedModels.Remove(id);
        }

        wasTracking.Remove(id);
        SetStatus(searchingMessage);
    }

    private void SetStatus(string message)
    {
        if (statusText != null) statusText.text = message;
    }
}
