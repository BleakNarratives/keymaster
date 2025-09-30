//
// RWPData.swift
// ARMetrologyApp - Configuration Data Model
//

import simd

/// A structure for deserializing Real-World Physical (RWP) coordinates
/// from a configuration file (RWPConfig.json).
struct RWPData: Decodable {
    let id: String
    let x: Float
    let y: Float
    let z: Float
    
    // Helper to convert to a SIMD3<Float>
    var coordinate: SIMD3<Float> {
        return SIMD3<Float>(x, y, z)
    }
}

/// Root structure for the RWP configuration file, holding a list of points.
struct RWPConfiguration: Decodable {
    let points: [RWPData]
}

//
// CalibrationLoader.swift
// ARMetrologyApp - Utility Layer
//

import Foundation

/// Handles loading and parsing of the Real-World Physical (RWP) reference points
/// from the RWPConfig.json file, making the app dynamically configurable.
class CalibrationLoader {
    
    /// Loads RWP points from a JSON file in the main bundle and populates the CalibrationManager.
    /// - Parameters:
    ///   - manager: The CalibrationManager instance to populate.
    ///   - fileName: The name of the JSON file (e.g., "RWPConfig").
    /// - Returns: An ordered list of point IDs.
    /// - Throws: An error if the file cannot be loaded or parsed.
    static func loadRwpPoints(into manager: CalibrationManager, from fileName: String = "RWPConfig") throws -> [String] {
        guard let url = Bundle.main.url(forResource: fileName, withExtension: "json") else {
            throw CalibrationError.custom(message: "RWP Configuration file '\(fileName).json' not found in bundle.")
        }
        
        let data = try Data(contentsOf: url)
        let decoder = JSONDecoder()
        let config = try decoder.decode(RWPConfiguration.self, from: data)
        
        guard !config.points.isEmpty else {
            throw CalibrationError.custom(message: "RWP configuration contains no points.")
        }
        
        var orderedPointIds: [String] = []
        
        for rwpData in config.points {
            manager.addRealWorldReferencePoint(id: rwpData.id, coordinate: rwpData.coordinate)
            orderedPointIds.append(rwpData.id)
        }
        
        return orderedPointIds
    }
}