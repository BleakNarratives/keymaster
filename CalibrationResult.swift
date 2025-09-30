//
// CalibrationResult.swift
// ARMetrologyApp - ModMind Data Standard
//

import Foundation
import simd

/// Serializable structure representing a single element of the 4x4 transformation matrix.
struct MatrixElement: Codable {
    let m: Float
}

/// The unified data structure for the final calibration result, adhering to the 
/// ModMind/EquiNex cross-platform data standard.
struct CalibrationResult: Codable {
    
    // Date/time of computation (ISO 8601 string format is default for Date in JSONEncoder)
    let timestamp: Date
    
    // Root Mean Square Error (Quality of Fit)
    let rmse: Double

    // Flat 16-element array representing the 4x4 transformation matrix (must match C# row-major order)
    let transform_elements: [MatrixElement]

    let source_platform: String
    
    let num_points_used: Int
    
    // MARK: - Utility Methods
    
    /// Reconstructs a simd_float4x4 matrix from the serialized elements.
    func toSimdMatrix() throws -> simd_float4x4 {
        guard transform_elements.count == 16 else {
            throw CalibrationError.custom(message: "JSON element array must contain 16 elements.")
        }
        
        let m = transform_elements.map { $0.m }

        // Reconstructs the simd_float4x4, which uses column-major indexing for its initial columns.
        // We must map the C# (row-major) serialization back to the Swift/simd (column-major) internal representation.
        // C# Matrix4x4(M11, M12, M13, M14, ...) -> simd_float4x4(col0, col1, col2, col3)
        return simd_float4x4(
            // Column 0
            SIMD4<Float>(m[0], m[4], m[8], m[12]), 
            // Column 1
            SIMD4<Float>(m[1], m[5], m[9], m[13]),
            // Column 2
            SIMD4<Float>(m[2], m[6], m[10], m[14]),
            // Column 3
            SIMD4<Float>(m[3], m[7], m[11], m[15])
        )
    }
    
    /// Creates a serializable CalibrationResult from the CalibrationManager output.
    static func fromManager(manager: CalibrationManager, sourcePlatform: String) throws -> CalibrationResult {
        guard let transform = manager.arWorldToRealWorldTransform, let rmse = manager.rootMeanSquareError else {
            throw CalibrationError.custom(message: "Cannot serialize: Transformation has not been computed.")
        }
        
        // Simd matrices are column-major. We must convert to a predictable row-major serialization order 
        // that matches the C# System.Numerics standard (M11, M12, M13, M14, M21, ... M44).
        let m = transform.transpose // Transpose to get row-major elements
        
        let elements: [MatrixElement] = [
            m[0][0], m[0][1], m[0][2], m[0][3],
            m[1][0], m[1][1], m[1][2], m[1][3],
            m[2][0], m[2][1], m[2][2], m[2][3],
            m[3][0], m[3][1], m[3][2], m[3][3]
        ].map { MatrixElement(m: $0) }
        
        return CalibrationResult(
            timestamp: Date(),
            rmse: rmse,
            transform_elements: elements,
            source_platform: sourcePlatform,
            num_points_used: manager.calibrationPoints.values.filter { $0.arWorldCoordinate != nil }.count
        )
    }
}

// MARK: - Serialization Utility Extension

extension CalibrationResult {
    
    /// Saves the result to a specified file URL.
    func save(to url: URL) throws {
        let encoder = JSONEncoder()
        encoder.outputFormatting = .prettyPrinted
        let data = try encoder.encode(self)
        try data.write(to: url, options: .atomic)
    }
    
    /// Loads a result from a specified file URL.
    static func load(from url: URL) throws -> CalibrationResult {
        let data = try Data(contentsOf: url)
        let decoder = JSONDecoder()
        return try decoder.decode(CalibrationResult.self, from: data)
    }
}