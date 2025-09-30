// CalibrationResult.cs
// MetrologyCalibration - ModMind Data Standard
using System;
using System.Text.Json.Serialization;

public struct MatrixElement {
    [JsonPropertyName("m")]
    public float Value { get; set; }
}

public struct CalibrationResult {
    [JsonPropertyName("timestamp")]
    public DateTime Timestamp { get; set; }
    [JsonPropertyName("rmse")]
    public double Rmse { get; set; }
    [JsonPropertyName("transform_elements")]
    public MatrixElement[] TransformElements { get; set; }
    [JsonPropertyName("source_platform")]
    public string SourcePlatform { get; set; }
}
