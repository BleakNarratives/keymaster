// CalibrationResult.swift
// MetrologyCalibration - ModMind Data Standard
import Foundation

struct MatrixElement: Codable {
    let m: Float
}

struct CalibrationResult: Codable {
    let timestamp: Date
    let rmse: Double
    let transform_elements: [MatrixElement]
    let source_platform: String
}
