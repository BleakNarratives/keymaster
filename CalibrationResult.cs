//
// CalibrationResult.cs
// MetrologyCalibration - ModMind Data Standard
//

using System;
using System.Text.Json.Serialization;

/// <summary>
/// Serializable structure representing a single element of the 4x4 transformation matrix.
/// Used to flatten the matrix into a single, JSON-friendly array.
/// </summary>
public struct MatrixElement
{
    [JsonPropertyName("m")]
    public float Value { get; set; }
}

/// <summary>
/// The unified data structure for the final calibration result, adhering to the 
/// ModMind/EquiNex cross-platform data standard.
/// </summary>
public struct CalibrationResult
{
    // The date/time of computation for auditability (EquiLex requirement)
    [JsonPropertyName("timestamp")]
    public DateTime Timestamp { get; set; }
    
    // Root Mean Square Error (Quality of Fit)
    [JsonPropertyName("rmse")]
    public double Rmse { get; set; }

    // Flat 16-element array representing the 4x4 transformation matrix (column-major storage is common)
    // NOTE: C# System.Numerics.Matrix4x4 uses row-major indexing, but we serialize based on the 16 fields.
    [JsonPropertyName("transform_elements")]
    public MatrixElement[] TransformElements { get; set; }

    [JsonPropertyName("source_platform")]
    public string SourcePlatform { get; set; }

    [JsonPropertyName("num_points_used")]
    public int NumPointsUsed { get; set; }
}