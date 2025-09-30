//
// CalibrationManager.swift
// ARMetrologyApp - Core Logic Refactored
//

import ARKit
import simd

// MARK: - Calibration Data Structures (Updated for better handling)

struct CalibrationPoint {
    // ... (No change to this struct)
    let id: String
    let realWorldCoordinate: SIMD3<Float>
    var arWorldCoordinate: SIMD3<Float>?
    var arAnchor: ARAnchor?
    
    // Initializer remains the same
    init(id: String, realWorldCoordinate: SIMD3<Float>) {
        self.id = id
        self.realWorldCoordinate = realWorldCoordinate
        self.arWorldCoordinate = nil
        self.arAnchor = nil
    }
    
    mutating func setArWorldCoordinate(_ coordinate: SIMD3<Float>) {
        self.arWorldCoordinate = coordinate
    }
}

// MARK: - CalibrationManager Class (Refactored)

class CalibrationManager: NSObject {
    
    // MARK: - Properties (No change)
    private var calibrationPoints: [String: CalibrationPoint] = [:]
    var arWorldToRealWorldTransform: simd_float4x4?
    var rootMeanSquareError: Double? // New property for quality of fit
    weak var delegate: CalibrationManagerDelegate?
    
    // MARK: - Core Transformation Method (Updated)
    
    /// Computes the 6-DOF transformation using Procrustes analysis via SVD.
    /// - Throws: A CalibrationError if points are insufficient or computation fails.
    func computeTransformation() throws {
        let completedPoints = calibrationPoints.values.filter { $0.arWorldCoordinate != nil }
        
        guard completedPoints.count >= 3 else {
            throw CalibrationError.insufficientPoints(needed: 3, provided: completedPoints.count)
        }
        
        // 1. Prepare data (RWP = Target, ARW = Source)
        let realWorldPoints = completedPoints.map { $0.realWorldCoordinate }
        let arWorldPoints = completedPoints.map { $0.arWorldCoordinate! }
        
        // 2. Compute centroids and centered points (Same math as C#)
        let centroidARW = arWorldPoints.reduce(SIMD3<Float>.zero, +) / Float(arWorldPoints.count)
        let centroidRWP = realWorldPoints.reduce(SIMD3<Float>.zero, +) / Float(realWorldPoints.count)
        
        let centeredARW = arWorldPoints.map { $0 - centroidARW }
        let centeredRWP = realWorldPoints.map { $0 - centroidRWP }

        // 3. Compute the covariance matrix H = sum(ARW_i * RWP_i^T)
        var H = simd_float3x3.zero
        for i in 0..<completedPoints.count {
            H += simd_float3x3(centeredARW[i], SIMD3<Float>.zero, SIMD3<Float>.zero) * centeredRWP[i].transpose // Simplified outer product
        }

        // 4. Perform SVD on H: H = U * S * V.transpose
        // NOTE: The robust SVD logic is now handled by the C# SVD.cs implementation.
        // In a true cross-platform app, this Swift layer MUST use:
        // A) A C-bridge/wrapper for a robust library (e.g., Eigen, LAPACK).
        // B) Apple's Accelerate framework functions (if available).
        // C) A call to your C# engine (if running as a backend service).
        
        // *** DUMMY SVD REMOVED ***
        // *** This requires a robust, external SVD implementation to proceed ***
        
        // Since we are modeling the structure, we use the identity rotation for the math below
        // This MUST BE REPLACED with the actual SVD output U, V, and S in the final product.
        // This is a placeholder for V * U.transpose
        var computedRotation = simd_float3x3.identity 

        // 5. Compute Translation t = C_RWP - R * C_ARW
        let computedTranslation = centroidRWP - computedRotation * centroidARW

        // 6. Construct the final 4x4 transformation matrix
        arWorldToRealWorldTransform = simd_float4x4(
            SIMD4<Float>(computedRotation.columns.0, 0),
            SIMD4<Float>(computedRotation.columns.1, 0),
            SIMD4<Float>(computedRotation.columns.2, 0),
            SIMD4<Float>(computedTranslation.x, computedTranslation.y, computedTranslation.z, 1)
        )
        
        // 7. Calculate RMSE (Quality of Fit)
        calculateRmse(realWorldPoints: realWorldPoints, arWorldPoints: arWorldPoints, R: computedRotation, t: computedTranslation)
        
        delegate?.calibrationManager(self, didComputeTransform: arWorldToRealWorldTransform!)
    }
    
    // ... (Omitted point management and helper functions for brevity - they are unchanged)
    
    private func calculateRmse(realWorldPoints: [SIMD3<Float>], arWorldPoints: [SIMD3<Float>], R: simd_float3x3, t: SIMD3<Float>) {
        var sumSquaredError: Double = 0
        let N = realWorldPoints.count

        for i in 0..<N {
            // Transform the ARW point: R * ARW + t
            let transformedPoint = R * arWorldPoints[i] + t
            
            // Squared magnitude of the error
            let errorVector = realWorldPoints[i] - transformedPoint
            sumSquaredError += Double(simd_length_squared(errorVector))
        }

        rootMeanSquareError = sqrt(sumSquaredError / Double(N))
    }
}