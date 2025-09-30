//
// CalibrationManager.cs
// MetrologyCalibration - Core Logic
//

using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;

/// <summary>
/// Manages calibration points and computes the 6-DOF rigid body transformation
/// from the AR World coordinate system to a Real-World Physical coordinate system
/// using Procrustes analysis via SVD, with integrated metrology checks and AI analysis.
/// </summary>
public class CalibrationManager
{
    private Dictionary<string, CalibrationPoint> _calibrationPoints = new Dictionary<string, CalibrationPoint>();

    public Matrix4x4? ArWorldToRealWorldTransform { get; private set; }
    public double? RootMeanSquareError { get; private set; }

    // MARK: - Public Point Management (Omitted for brevity, assumed unchanged)

    // ... (AddRealWorldReferencePoint, CaptureArWorldCoordinate, GetAllCalibrationPoints, ResetCalibration) ...

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
    
    // MARK: - Core Transformation (Omitted internal math functions for brevity, assumed unchanged)

    public void ComputeTransformation()
    {
        var completedPoints = _calibrationPoints.Values.Where(p => p.ArWorldCoordinate.HasValue).ToList();
        if (completedPoints.Count < 3)
        {
            throw CalibrationException.InsufficientPoints(needed: 3, provided: completedPoints.Count);
        }

        Vector3[] realWorldPoints = completedPoints.Select(p => p.RealWorldCoordinate).ToArray();
        Vector3[] arWorldPoints = completedPoints.Select(p => p.ArWorldCoordinate.Value).ToArray();

        Vector3 centroidARW = arWorldPoints.Aggregate(Vector3.Zero, (current, p) => current + p) / arWorldPoints.Length;
        Vector3 centroidRWP = realWorldPoints.Aggregate(Vector3.Zero, (current, p) => current + p) / realWorldPoints.Length;

        Vector3[] centeredARW = arWorldPoints.Select(p => p - centroidARW).ToArray();
        Vector3[] centeredRWP = realWorldPoints.Select(p => p - centroidRWP).ToArray();

        // ** ROBUSTNESS CHECK: Check for collinearity **
        if (MetrologyRobustness.ArePointsCollinear(centeredARW))
        {
            throw CalibrationException.ComputationFailed(
                "Input points are collinear. Cannot solve for 3D rotation. Recapture points with better spatial distribution."
            );
        }
        
        // ... (SVD, Rotation, Translation calculation omitted for length, assumed correct) ...
        
        // Compute the covariance matrix H...
        Matrix3x3 H = Matrix3x3.Zero();
        for (int i = 0; i < completedPoints.Count; i++)
        {
            H += Matrix3x3.OuterProduct(centeredARW[i], centeredRWP[i]);
        }

        // Perform SVD on H... (error handling omitted for brevity)
        SVDResult svdResult = SVD.Compute(H); // Assuming SVD class is available

        Matrix3x3 U = svdResult.U;
        Matrix3x3 V = svdResult.V;

        Matrix3x3 R = V * U.Transpose();

        // Check for reflection and fix...
        if (R.Determinant() < 0)
        {
            V = new Matrix3x3(
                V.M11, V.M12, -V.M13,
                V.M21, V.M22, -V.M23,
                V.M31, V.M32, -V.M33
            );
            R = V * U.Transpose();
        }

        Vector3 t = centroidRWP - Matrix3x3.Multiply(R, centroidARW);

        // Construct the final 4x4 transformation matrix...
        ArWorldToRealWorldTransform = new Matrix4x4(
            R.M11, R.M12, R.M13, 0,
            R.M21, R.M22, R.M23, 0,
            R.M31, R.M32, R.M33, 0,
            t.X, t.Y, t.Z, 1
        );
        
        // Calculate RMSE...
        // ... (CalculateRmse method logic) ...
        
        double sumSquaredError = 0;
        int N = realWorldPoints.Length;

        for (int i = 0; i < N; i++)
        {
            Vector3 transformedPoint = Matrix3x3.Multiply(R, arWorldPoints[i]) + t;
            Vector3 errorVector = realWorldPoints[i] - transformedPoint;
            sumSquaredError += errorVector.LengthSquared();
        }

        RootMeanSquareError = Math.Sqrt(sumSquaredError / N);
        
        OnTransformComputed?.Invoke(this, ArWorldToRealWorldTransform.Value);
    }

    // MARK: - AI/EquiNex Integration (NEW)

    /// <summary>
    /// Packages the current transformation result and sends it to the active EquiNex AI agent for analysis.
    /// </summary>
    /// <param name="analysisPrompt">The specific instruction for the AI (e.g., "Review quality and suggest improvements").</param>
    /// <param name="sourcePlatform">The platform generating the result (e.g., "Unity-ARF", "ARKit-Swift").</param>
    public async Task<string> AnalyzeCalibrationWithAI(string analysisPrompt, string sourcePlatform)
    {
        if (!ArWorldToRealWorldTransform.HasValue || !RootMeanSquareError.HasValue)
        {
            return "Error: Cannot analyze; calibration has not been computed.";
        }
        
        // 1. Package the result into the ModMind data standard (JSON)
        var result = CalibrationSerializer.ToCalibrationResult(this, sourcePlatform);
        string jsonContext = CalibrationSerializer.SerializeToJson(result);

        // 2. Get the active AI agent from the EquiNex service locator
        IQueryAgent agent = AgentServiceLocator.Instance.GetAgent();

        // 3. Send the prompt and data context to the AI
        return await agent.QueryWithData(analysisPrompt, jsonContext);
    }
    
    // MARK: - Transformation Methods (Helper) (Omitted for brevity, assumed unchanged)
    
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

    // MARK: - Events (Omitted for brevity, assumed unchanged)
    public event EventHandler<IReadOnlyList<CalibrationPoint>> OnCalibrationPointsUpdated;
    public event EventHandler<Matrix4x4> OnTransformComputed;
    public event EventHandler OnCalibrationReset;
}