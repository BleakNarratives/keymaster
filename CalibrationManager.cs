//
// CalibrationManager.cs
// MetrologyCalibration - Core Logic
//
// Created by AI Assistant on 2023-10-27.
//

using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

/// <summary>
/// Manages calibration points and computes the 6-DOF rigid body transformation
/// from the AR World coordinate system to a Real-World Physical coordinate system
/// using Procrustes analysis via SVD, with integrated metrology checks.
/// </summary>
public class CalibrationManager
{
    private Dictionary<string, CalibrationPoint> _calibrationPoints = new Dictionary<string, CalibrationPoint>();

    /// <summary>
    /// The computed transformation matrix from AR World (ARW) to Real-World Physical (RWP).
    /// </summary>
    public Matrix4x4? ArWorldToRealWorldTransform { get; private set; }

    /// <summary>
    /// The Root Mean Square Error (RMSE) of the final transformation fit.
    /// Lower values indicate a better fit and higher accuracy.
    /// </summary>
    public double? RootMeanSquareError { get; private set; }

    // MARK: - Public Point Management

    public void AddRealWorldReferencePoint(string id, Vector3 coordinate)
    {
        var point = new CalibrationPoint(id, coordinate);
        if (_calibrationPoints.TryGetValue(id, out var existingPoint) && existingPoint.ArWorldCoordinate.HasValue)
        {
            point.SetArWorldCoordinate(existingPoint.ArWorldCoordinate.Value);
        }
        _calibrationPoints[id] = point;
        OnCalibrationPointsUpdated?.Invoke(this, _calibrationPoints.Values.ToList());
    }

    public void CaptureArWorldCoordinate(string id, Vector3 arWorldPosition)
    {
        if (_calibrationPoints.TryGetValue(id, out var point))
        {
            point.SetArWorldCoordinate(arWorldPosition);
            _calibrationPoints[id] = point;
            OnCalibrationPointsUpdated?.Invoke(this, _calibrationPoints.Values.ToList());
        }
        else
        {
            throw CalibrationException.Custom($"Error: Attempted to capture AR coordinate for unknown point ID: {id}");
        }
    }

    public IReadOnlyList<CalibrationPoint> GetAllCalibrationPoints()
    {
        return _calibrationPoints.Values.OrderBy(p => p.Id).ToList();
    }

    public void ResetCalibration()
    {
        var pointsToReset = _calibrationPoints.Keys.ToList();
        foreach (var id in pointsToReset)
        {
            var point = _calibrationPoints[id];
            _calibrationPoints[id] = new CalibrationPoint(point.Id, point.RealWorldCoordinate);
        }
        ArWorldToRealWorldTransform = null;
        RootMeanSquareError = null;
        OnCalibrationPointsUpdated?.Invoke(this, _calibrationPoints.Values.ToList());
        OnCalibrationReset?.Invoke(this, EventArgs.Empty);
    }

    // MARK: - Core Transformation

    /// <summary>
    /// Computes the 6-DOF rigid body transformation and the quality of the fit (RMSE).
    /// </summary>
    /// <exception cref="CalibrationException">Thrown if insufficient points or computation fails.</exception>
    public void ComputeTransformation()
    {
        var completedPoints = _calibrationPoints.Values.Where(p => p.ArWorldCoordinate.HasValue).ToList();
        if (completedPoints.Count < 3)
        {
            throw CalibrationException.InsufficientPoints(needed: 3, provided: completedPoints.Count);
        }

        Vector3[] realWorldPoints = completedPoints.Select(p => p.RealWorldCoordinate).ToArray();
        Vector3[] arWorldPoints = completedPoints.Select(p => p.ArWorldCoordinate.Value).ToArray();

        // 1. Calculate centroids and center the points
        Vector3 centroidARW = arWorldPoints.Aggregate(Vector3.Zero, (current, p) => current + p) / arWorldPoints.Length;
        Vector3 centroidRWP = realWorldPoints.Aggregate(Vector3.Zero, (current, p) => current + p) / realWorldPoints.Length;

        Vector3[] centeredARW = arWorldPoints.Select(p => p - centroidARW).ToArray();
        Vector3[] centeredRWP = realWorldPoints.Select(p => p - centroidRWP).ToArray();

        // ** NEW ROBUSTNESS CHECK: Ensure points are not collinear before proceeding **
        if (MetrologyRobustness.ArePointsCollinear(centeredARW))
        {
            throw CalibrationException.ComputationFailed(
                "Input points are collinear. Cannot solve for 3D rotation. Recapture points with better spatial distribution."
            );
        }
        
        // 2. Compute the covariance matrix H (H = sum(ARW_i * RWP_i^T))
        Matrix3x3 H = Matrix3x3.Zero();
        for (int i = 0; i < completedPoints.Count; i++)
        {
            H += Matrix3x3.OuterProduct(centeredARW[i], centeredRWP[i]);
        }

        // 3. Perform SVD on H: H = U * S * V.Transpose()
        SVDResult svdResult;
        try
        {
            svdResult = SVD.Compute(H);
        }
        catch (Exception ex)
        {
            throw CalibrationException.ComputationFailed($"SVD failed during covariance decomposition: {ex.Message}");
        }
        
        Matrix3x3 U = svdResult.U;
        Matrix3x3 V = svdResult.V;

        // 4. Compute Rotation R = V * U.Transpose()
        Matrix3x3 R = V * U.Transpose();

        // 5. Check for reflection and fix (ensuring det(R) = +1)
        if (R.Determinant() < 0)
        {
            // Flip the sign of the last column of V to convert reflection to rotation
            V = new Matrix3x3(
                V.M11, V.M12, -V.M13,
                V.M21, V.M22, -V.M23,
                V.M31, V.M32, -V.M33
            );
            R = V * U.Transpose();
        }

        // 6. Compute Translation t = C_RWP - R * C_ARW
        Vector3 t = centroidRWP - Matrix3x3.Multiply(R, centroidARW);

        // 7. Construct the final 4x4 transformation matrix
        ArWorldToRealWorldTransform = new Matrix4x4(
            R.M11, R.M12, R.M13, 0,
            R.M21, R.M22, R.M23, 0,
            R.M31, R.M32, R.M33, 0,
            t.X, t.Y, t.Z, 1
        );
        
        // 8. Calculate RMSE (Quality of Fit)
        CalculateRmse(realWorldPoints, arWorldPoints, R, t);

        OnTransformComputed?.Invoke(this, ArWorldToRealWorldTransform.Value);
    }

    /// <summary>
    /// Calculates the Root Mean Square Error (RMSE) of the transformation.
    /// </summary>
    private void CalculateRmse(Vector3[] realWorldPoints, Vector3[] arWorldPoints, Matrix3x3 R, Vector3 t)
    {
        double sumSquaredError = 0;
        int N = realWorldPoints.Length;

        for (int i = 0; i < N; i++)
        {
            // Transform the ARW point: R * ARW + t
            Vector3 transformedPoint = Matrix3x3.Multiply(R, arWorldPoints[i]) + t;
            
            // Squared magnitude of the error
            Vector3 errorVector = realWorldPoints[i] - transformedPoint;
            sumSquaredError += errorVector.LengthSquared();
        }

        RootMeanSquareError = Math.Sqrt(sumSquaredError / N);
    }
    
    // MARK: - Transformation Methods (Helper)
    
    public Vector3? TransformArWorldToRealWorld(Vector3 arWorldPosition)
    {
        if (!ArWorldToRealWorldTransform.HasValue) return null;
        return Vector3.Transform(arWorldPosition, ArWorldToRealWorldTransform.Value);
    }

    public Vector3? TransformRealWorldToArWorld(Vector3 realWorldPosition)
    {
        if (!ArWorldToRealWorldTransform.HasValue) return null;
        if (Matrix4x4.Invert(ArWorldToRealWorldTransform.Value, out Matrix4x4 inverseTransform))
        {
            return Vector3.Transform(realWorldPosition, inverseTransform);
        }
        return null;
    }

    // MARK: - Events (C# equivalent of the Swift Delegate)
    public event EventHandler<IReadOnlyList<CalibrationPoint>> OnCalibrationPointsUpdated;
    public event EventHandler<Matrix4x4> OnTransformComputed;
    public event EventHandler OnCalibrationReset;
}