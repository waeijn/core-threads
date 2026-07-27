using UnityEngine;

public class MouseUtil : MonoBehaviour
{
    // FIX: Remove the 'private static Camera' variable completely!

    public static Vector3 GetMousePositionInWorldSpace(float zValue = 0f)
    {
        // 1. Safely get the active camera every time
        Camera cam = Camera.main;
        if (cam == null) return Vector3.zero;

        // 2. Use Vector3.back to ensure the plane is perfectly flat and facing the camera
        Plane dragPlane = new Plane(Vector3.back, new Vector3(0, 0, zValue));
        Ray ray = cam.ScreenPointToRay(Input.mousePosition);

        if (dragPlane.Raycast(ray, out float distance))
        {
            return ray.GetPoint(distance);
        }

        return Vector3.zero;
    }
}