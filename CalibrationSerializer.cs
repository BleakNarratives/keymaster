//
// CalibrationSerializer.cs
// MetrologyCalibration - Serialization Utility
//

using System.Numerics;
using System.Text.Json;
using System.IO;
using System.Linq;

public static class CalibrationSerializer
{
    /// <summary>
    /// Converts a CalibrationManager's result into a serializable CalibrationResult object.
    /// </summary>
    public static CalibrationResult ToCalibrationResult(CalibrationManager manager, string sourcePlatform)
    {
        if (!manager.ArWorldToRealWorldTransform.HasValue || !manager.RootMeanSquareError.HasValue)
        {
            throw new InvalidOperationException("Cannot serialize: Transformation has not been computed.");
        }

        Matrix4x4 m = manager.ArWorldToRealWorldTransform.Value;

        // Extract and flatten the 16 elements of the Matrix4x4 (Row-Major in System.Numerics)
        var elements = new MatrixElement[]
        {
            new MatrixElement { Value = m.M11 }, new MatrixElement { Value = m.M12 }, new MatrixElement { Value = m.M13 }, new MatrixElement { Value = m.M14 },
            new MatrixElement { Value = m.M21 }, new MatrixElement { Value = m.M22 }, new MatrixElement { Value = m.M23 }, new MatrixElement { Value = m.M24 },
            new MatrixElement { Value = m.M31 }, new MatrixElement { Value = m.M32 }, new MatrixElement { Value = m.M33 }, new MatrixElement { Value = m.M34 },
            new MatrixElement { Value = m.M41 }, new MatrixElement { Value = m.M42 }, new MatrixElement { Value = m.M43 }, new MatrixElement { Value = m.M44 }
        };

        return new CalibrationResult
        {
            Timestamp = DateTime.UtcNow,
            Rmse = manager.RootMeanSquareError.Value,
            TransformElements = elements,
            SourcePlatform = sourcePlatform,
            NumPointsUsed = manager.GetAllCalibrationPoints().Count(p => p.ArWorldCoordinate.HasValue)
        };
    }

    /// <summary>
    /// Loads a CalibrationResult from a JSON file.
    /// </summary>
    public static CalibrationResult LoadFromJson(string filePath)
    {
        string jsonString = File.ReadAllText(filePath);
        return JsonSerializer.Deserialize<CalibrationResult>(jsonString) 
            ?? throw new JsonException("Failed to deserialize CalibrationResult.");
    }
    
    /// <summary>
    /// Saves a CalibrationResult to a JSON file.
    /// </summary>
    public static void SaveToJson(CalibrationResult result, string filePath)
    {
        var options = new JsonSerializerOptions { WriteIndented = true };
        string jsonString = JsonSerializer.Serialize(result, options);
        File.WriteAllText(filePath, jsonString);
    }

    /// <summary>
    /// Reconstructs a Matrix4x4 from a CalibrationResult.
    /// </summary>
    public static Matrix4x4 ToMatrix4x4(CalibrationResult result)
    {
        if (result.TransformElements.Length != 16)
        {
            throw new InvalidDataException("JSON element array must contain 16 elements for 4x4 matrix.");
        }

        return new Matrix4x4(
            result.TransformElements[0].Value, result.TransformElements[1].Value, result.TransformElements[2].Value, result.TransformElements[3].Value,
            result.TransformElements[4].Value, result.TransformElements[5].Value, result.TransformElements[6].Value, result.TransformElements[7].Value,
            result.TransformElements[8].Value, result.TransformElements[9].Value, result.TransformElements[10].Value, result.TransformElements[11].Value,
            result.TransformElements[12].Value, result.TransformElements[13].Value, result.TransformElements[14].Value, result.TransformElements[15].Value
        );
    }
}