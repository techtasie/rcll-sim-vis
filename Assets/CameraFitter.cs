using UnityEngine;

public class CameraFitAllSprites : MonoBehaviour
{
    public float padding = 50f; // Padding in pixels
    private Camera cam;

    void Start()
    {
        cam = GetComponent<Camera>();
        if (cam == null)
        {
            Debug.LogError("CameraFitAllSprites script must be attached to a Camera object.");
            return;
        }
    }

    void Update()
    {
        FitCameraToAllSprites();
    }

    void FitCameraToAllSprites()
    {
        SpriteRenderer[] sprites = FindObjectsOfType<SpriteRenderer>();
        if (sprites.Length == 0)
        {
            Debug.LogWarning("No sprites found in the scene to fit the camera.");
            return;
        }

        // Calculate bounds that include all sprites
        Bounds combinedBounds = sprites[0].bounds;
        foreach (SpriteRenderer sprite in sprites)
        {
            combinedBounds.Encapsulate(sprite.bounds);
        }

        // Convert padding from pixels to world units
        float paddingWorldUnits = padding / cam.pixelWidth * cam.orthographicSize * 2f * (float)Screen.height / Screen.width;
        combinedBounds.Expand(new Vector3(paddingWorldUnits, 0, 0));

        // Set the camera position to be centered
        cam.transform.position = new Vector3(combinedBounds.center.x, combinedBounds.center.y, cam.transform.position.z);

        // Adjust orthographic size to fit the width of the bounds
        float screenRatio = (float)Screen.width / Screen.height;
        cam.orthographicSize = combinedBounds.size.x / 2f / screenRatio + paddingWorldUnits;
    }
}
