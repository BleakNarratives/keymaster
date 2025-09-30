//
// MetrologyRobustness.cs
// MetrologyCalibration - Robustness Utilities
//

using System.Numerics;
using System.Linq;
using System;

/// <summary>
/// Provides utilities to check the geometric validity of a set of calibration points.
/// </summary>
public static class MetrologyRobustness
{
    private const float CollinearityEpsilon = 1e-4f; // 0.1 mm error tolerance

    /// <summary>
    /// Checks if a set of 3D points are collinear (lie on the same line).
    /// Collinearity prevents solving for the full 3D rotation matrix.
    /// </summary>
    /// <param name="points">An array of 3D points (e.g., centered RWP or ARW points).</param>
    /// <returns>True if the points are collinear, false otherwise.</returns>
    public static bool ArePointsCollinear(Vector3[] points)
    {
        if (points.Length < 3) return false; // Not enough points to check

        // Check for collinearity by calculating the area of a triangle (represented by the cross product).
        // If the area is near zero, the points are collinear.
        
        // Use the first three unique points to define the geometry
        Vector3 p1 = points[0];
        Vector3 p2 = points[1];
        Vector3 p3 = points[2];
        
        // Vectors forming two sides of the triangle
        Vector3 v12 = p2 - p1;
        Vector3 v13 = p3 - p1;

        // The magnitude of the cross product (v12 x v13) is twice the area of the triangle.
        float crossProductMagnitude = Vector3.Cross(v12, v13).Length();

        // If cross product magnitude is close to zero, points are collinear.
        return crossProductMagnitude < CollinearityEpsilon;
    }
    
    // NOTE: You can add ArePointsCoplanar here as well, which is necessary for N > 4 points
    // if you want to explicitly forbid 3D rotations from coplanar inputs.
}