//
// CalibrationLoader.cs
// MetrologyCalibration - Utility Layer
//

using System;
using System.IO;
using System.Text.Json;
using System.Collections.Generic;

/// <summary>
/// Handles loading and parsing of the Real-World Physical (RWP) reference points
/// from the RWPConfig.json file.
/// </summary>
public static class CalibrationLoader
{
    /// <summary>
    /// Loads RWP points from a JSON file and populates the CalibrationManager.
    /// </summary>
    /// <param name="manager">The CalibrationManager instance to populate.</param>
    /// <param name="jsonFilePath">The file path to the RWPConfig.json.</param>
    /// <returns>A list of point IDs in the order they were defined.</returns>
    /// <exception cref="FileNotFoundException">Thrown if the JSON file is not found.</exception>
    /// <exception cref="JsonException">Thrown if the JSON parsing fails.</exception>
    public static List<string> LoadRwpPointsFromJson(CalibrationManager manager, string jsonFilePath)
    {
        if (!File.Exists(jsonFilePath))
        {
            throw new FileNotFoundException($"RWP Configuration file not found at: {jsonFilePath}");
        }

        string jsonString = File.ReadAllText(jsonFilePath);
        RWPConfiguration config = JsonSerializer.Deserialize<RWPConfiguration>(jsonString) 
            ?? throw new JsonException("Failed to deserialize RWPConfiguration.");

        if (config.Points == null || config.Points.Length == 0)
        {
            throw new InvalidOperationException("RWP configuration contains no points.");
        }

        List<string> orderedPointIds = new List<string>();

        foreach (var rwpData in config.Points)
        {
            // Add point to the CalibrationManager
            manager.AddRealWorldReferencePoint(rwpData.Id, rwpData.ToVector3());
            orderedPointIds.Add(rwpData.Id);
        }

        return orderedPointIds;
    }
}