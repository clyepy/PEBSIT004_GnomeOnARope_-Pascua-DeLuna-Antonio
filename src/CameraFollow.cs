using UnityEngine;

public class CameraFollow : MonoBehaviour {
    // The object we want to follow (the gnome’s Body)
    public Transform target;

    // The highest point the camera can go
    public float topLimit = 10.0f;

    // The lowest point the camera can go
    public float bottomLimit = -10.0f;

    // How quickly the camera moves toward the target
    public float followSpeed = 0.5f;

    // After everything has updated, move the camera
    void LateUpdate () {
        if (target != null) {
            // Keep camera’s current position
            Vector3 newPosition = transform.position;

            // Smoothly move camera’s Y position toward the target
            newPosition.y = Mathf.Lerp(
                newPosition.y,
                target.position.y,
                followSpeed
            );

            // Clamp camera between top and bottom limits
            newPosition.y = Mathf.Min(newPosition.y, topLimit);
            newPosition.y = Mathf.Max(newPosition.y, bottomLimit);

            // Apply new position
            transform.position = newPosition;
        }
    }

    // Draw a yellow line in the Scene view for limits
    void OnDrawGizmosSelected() {
        Gizmos.color = Color.yellow;
        Vector3 topPoint = new Vector3(transform.position.x, topLimit, transform.position.z);
        Vector3 bottomPoint = new Vector3(transform.position.x, bottomLimit, transform.position.z);
        Gizmos.DrawLine(topPoint, bottomPoint);
    }
}
