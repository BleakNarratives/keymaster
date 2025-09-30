//
// CalibrationError.cs
// MetrologyCalibration - Core Error Handling
//

using System;

/// <summary>
/// Defines specific, localized errors for the calibration process.
/// This structure mirrors the error definition in the cross-platform (Swift/C#) AR code.
/// </summary>
public class CalibrationException : Exception
{
    public enum ErrorType
    {
        InsufficientPoints,
        ComputationFailed,
        Custom
    }

    public ErrorType Type { get; }
    public int Needed { get; }
    public int Provided { get; }

    // Private constructor to force use of static factory methods
    private CalibrationException(string message, ErrorType type, int needed = 0, int provided = 0)
        : base(message)
    {
        Type = type;
        Needed = needed;
        Provided = provided;
    }

    /// <summary>
    /// Creates an exception for when not enough calibration points have been provided.
    /// </summary>
    public static CalibrationException InsufficientPoints(int needed, int provided)
    {
        string message = $"Insufficient calibration points. {needed} points are needed, but only {provided} were provided.";
        return new CalibrationException(message, ErrorType.InsufficientPoints, needed, provided);
    }

    /// <summary>
    /// Creates an exception for general computation failures (e.g., SVD stability issues).
    /// </summary>
    public static CalibrationException ComputationFailed(string reason)
    {
        string message = $"Calibration computation failed: {reason}";
        return new CalibrationException(message, ErrorType.ComputationFailed);
    }
    
    /// <summary>
    /// Creates a generic custom exception.
    /// </summary>
    public static CalibrationException Custom(string message)
    {
        return new CalibrationException(message, ErrorType.Custom);
    }
}