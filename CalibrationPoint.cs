//
//  CalibrationPoint.cs
//  MetrologyCalibration
//
//  Created by AI Assistant on 2023-10-27.
//

using System.Numerics; // For Vector3 and Matrix4x4

/// <summary>
/// Represents a single physical reference point with its known real-world coordinates
/// and its corresponding observed coordinate in the ARKit/ARCore World.
/// </summary>
public struct CalibrationPoint
{
    public string Id { get; }
    public Vector3 RealWorldCoordinate { get; } // Known coordinate in RWP (e.g., from CAD)
    public Vector3? ArWorldCoordinate { get; private set; } // Observed coordinate in ARW

    public CalibrationPoint(string id, Vector3 realWorldCoordinate)
    {
        Id = id;
        RealWorldCoordinate = realWorldCoordinate;
        ArWorldCoordinate = null;
    }

    /// <summary>
    /// Updates the AR World coordinate for this calibration point.
    /// </summary>
    /// <param name="arWorldCoordinate">The observed coordinate in the AR World.</param>
    public void SetArWorldCoordinate(Vector3 arWorldCoordinate)
    {
        ArWorldCoordinate = arWorldCoordinate;
    }
}