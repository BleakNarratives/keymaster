//
// CalibrationError.swift
// ARMetrologyApp - Core Error Handling
//

import Foundation

/// Defines specific, localized errors for the calibration process, mirroring the C# structure.
enum CalibrationError: Error, LocalizedError {
    case insufficientPoints(needed: Int, provided: Int)
    case computationFailed(reason: String)
    case custom(message: String)

    var errorDescription: String? {
        switch self {
        case .insufficientPoints(let needed, let provided):
            return "Insufficient calibration points. \(needed) points are needed, but only \(provided) were provided."
        case .computationFailed(let reason):
            return "Calibration computation failed: \(reason)"
        case .custom(let message):
            return message
        }
    }
}