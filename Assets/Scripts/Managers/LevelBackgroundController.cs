using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class BackgroundLayer
{
    public string layerName;
    public float speed; // Movement speed for the layer
    public List<GameObject> backgroundObjects; // List of objects in the layer
    public Vector3 moveDirection = Vector3.right; // Moving from left to right
}

public class LevelBackgroundController : MonoBehaviour
{
    [Header("Background Layers")]
    public List<BackgroundLayer> layers = new List<BackgroundLayer>();

    [Header("Screen Bounds")]
    public float offScreenPadding = 0.2f; // Padding beyond the screen edges
    
    [Header("Screen Bounds")]
    public float objectWidth = 10f; // Width of background objects for repositioning
    public float repositionOffsetMultiplier = 2f; // Multiplier for the offset
    [Header("Y-Position Randomization")]
    [Tooltip("Minimum Y position for background objects.")]
    public float yMin = -2f; // Minimum Y position for randomization
    [Tooltip("Maximum Y position for background objects.")]
    public float yMax = 2f; // Maximum Y position for randomization

    void Start()
    {
        // Initialize layers by finding child GameObjects
        foreach (Transform layerTransform in transform)
        {
            BackgroundLayer layer = layers.Find(l => l.layerName == layerTransform.name);
            if (layer == null)
            {
                layer = new BackgroundLayer();
                layer.layerName = layerTransform.name;
                layers.Add(layer);
            }

            // Populate background objects for the layer
            layer.backgroundObjects = new List<GameObject>();
            foreach (Transform obj in layerTransform)
            {
                layer.backgroundObjects.Add(obj.gameObject);
            }
        }
    }

    void Update()
    {
        foreach (BackgroundLayer layer in layers)
        {
            foreach (GameObject obj in layer.backgroundObjects)
            {
                // Move the object
                obj.transform.Translate(layer.moveDirection * layer.speed * Time.deltaTime);

                // Check if the object is off-screen, plus padding
                Vector3 viewPos = Camera.main.WorldToViewportPoint(obj.transform.position);
                // if (viewPos.x > 1 + offScreenPadding || viewPos.x < -offScreenPadding ||
                //     viewPos.y > 1 + offScreenPadding || viewPos.y < -offScreenPadding)
                // {
                //     RepositionObject(obj, layer);
                // }
                
                if (viewPos.x > 1 + offScreenPadding)
                {
                    RepositionObject(obj, layer);
                }
            }
        }
    }

    /// <summary>
    /// Repositions the background object to the opposite side with an offset and randomizes its Y position.
    /// </summary>
    /// <param name="obj">The background object to reposition.</param>
    /// <param name="layer">The layer to which the object belongs.</param>
    private void RepositionObject(GameObject obj, BackgroundLayer layer)
    {
        // Determine the direction of movement (only handling horizontal movement)
        Vector3 direction = layer.moveDirection.normalized;

        // Calculate the new X position based on movement direction and reposition offset
        float newX = 0f;
        if (direction == Vector3.right)
        {
            // Moving from left to right, reposition to the left side
            float leftBoundary = Camera.main.ViewportToWorldPoint(new Vector3(0, 0, 0)).x;
            newX = leftBoundary - (objectWidth * repositionOffsetMultiplier);
        }
        else if (direction == Vector3.left)
        {
            // Moving from right to left, reposition to the right side
            float rightBoundary = Camera.main.ViewportToWorldPoint(new Vector3(1, 0, 0)).x;
            newX = rightBoundary + (objectWidth * repositionOffsetMultiplier);
        }

        // Randomize the Y position within the specified range
        float randomY = Random.Range(yMin, yMax);

        // Assign the new position to the object
        obj.transform.position = new Vector3(newX, randomY, obj.transform.position.z);
    }
}
