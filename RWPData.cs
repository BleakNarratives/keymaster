//
// RWPData.cs
// MetrologyCalibration - Configuration Data Model
//

using System.Numerics;
using System.Text.Json.Serialization;

/// <summary>
/// A structure for deserializing Real-World Physical (RWP) coordinates
/// from a configuration file (e.g., RWPConfig.json).
/// </summary>
public struct RWPData
{
    [JsonPropertyName("id")]
    public string Id { get; set; }

    [JsonPropertyName("x")]
    public float X { get; set; }

    [JsonPropertyName("y")]
    public float Y { get; set; }

    [JsonPropertyName("z")]
    public float Z { get; set; }
    
    // Helper to convert to a System.Numerics.Vector3
    public Vector3 ToVector3()
    {
        return new Vector3(X, Y, Z);
    }
}

/// <summary>
/// Root structure for the RWP configuration file, holding a list of points.
/// </summary>
public struct RWPConfiguration
{
    [JsonPropertyName("points")]
    public RWPData[] Points { get; set; }
}