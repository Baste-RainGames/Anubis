using UnityEngine;

public static class TransformExtension {
    /// <summary>
    /// As LookAt2D, but over time.
    /// </summary>
    /// <param name="transform">Transform to turn</param>
    /// <param name="worldPosition">The world position the transform should look at</param>
    /// <param name="maxDegreesDelta">How many degrees the transform should rotate</param>
    public static void LookAt2D(this Transform transform, Vector3 worldPosition, float maxDegreesDelta) {
        Vector3 target2D = new Vector3(worldPosition.x, transform.position.y, worldPosition.z);
        if (target2D == transform.position) {
            return;
        }

        Quaternion wantedRotation = Quaternion.LookRotation(target2D - transform.position);
        transform.rotation = Quaternion.RotateTowards(transform.rotation, wantedRotation, maxDegreesDelta);
    }

    public static float AngleTo2D(this Transform transform, Vector3 worldPosition) {
        Vector3 target2D = new Vector3(worldPosition.x, transform.position.y, worldPosition.z);

        Quaternion wantedRotation = Quaternion.LookRotation(target2D - transform.position);
        Quaternion currentRotation = Quaternion.Euler(new Vector3(transform.forward.x, 0f, transform.forward.z).normalized);

        return Quaternion.Angle(wantedRotation, currentRotation);
    }

    public static Vector3 Normalized2D(this Vector3 vector) {
        vector.y = 0f;
        return vector.normalized;
    }
}