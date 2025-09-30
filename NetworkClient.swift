
import Foundation

// MARK: - Data Models

/// Model representing a 3D point with x, y, z coordinates
struct PointModel: Codable {
    let x: Float
    let y: Float
    let z: Float
}

/// Model representing the result of a calibration operation
struct CalibrationResultModel: Codable {
    let rmse: Float
    let transformElements: [Float]  // 4x4 transformation matrix as 16-element array
    let sourcePlatform: String
    let numPointsUsed: Int
}

/// Model for calibration request containing ARW and RWP points
struct CalibrationRequestModel: Codable {
    let arw_points: [PointModel]
    let rwp_points: [PointModel]
}

/// Model for analysis request with optional calibration result and prompt
struct AnalysisRequestModel: Codable {
    let result: CalibrationResultModel?
    let prompt: String
}

// MARK: - Network Error Enum

/// Errors that can occur during network operations
enum NetworkError: Error, LocalizedError {
    case invalidURL
    case serverError(statusCode: Int)
    case decodingError(Error)
    case encodingError(Error)
    case networkError(Error)

    var errorDescription: String? {
        switch self {
        case .invalidURL:
            return "Invalid URL"
        case .serverError(let statusCode):
            return "Server error with status code: \(statusCode)"
        case .decodingError(let error):
            return "Failed to decode response: \(error.localizedDescription)"
        case .encodingError(let error):
            return "Failed to encode request: \(error.localizedDescription)"
        case .networkError(let error):
            return "Network error: \(error.localizedDescription)"
        }
    }
}

// MARK: - Network Client

/// Static class for handling network requests to the local Python FastAPI bridge
class NetworkClient {
    /// Base URL for the local FastAPI server
    private static let baseURL = "http://127.0.0.1:8000/api/"

    /// Generic asynchronous function to handle POST requests with JSON serialization
    /// - Parameters:
    ///   - endpoint: The API endpoint (e.g., "calibrate")
    ///   - data: The encodable data to send in the request body
    ///   - responseType: The type of the expected response
    /// - Returns: The decoded response object
    /// - Throws: NetworkError for various failure conditions
    static func postJsonAsync<T: Encodable, U: Decodable>(
        endpoint: String,
        data: T,
        responseType: U.Type
    ) async throws -> U {
        // Construct the full URL
        guard let url = URL(string: baseURL + endpoint) else {
            throw NetworkError.invalidURL
        }

        // Create the request
        var request = URLRequest(url: url)
        request.httpMethod = "POST"
        request.setValue("application/json", forHTTPHeaderField: "Content-Type")

        // Encode the data to JSON
        do {
            let jsonData = try JSONEncoder().encode(data)
            request.httpBody = jsonData
        } catch {
            throw NetworkError.encodingError(error)
        }

        // Perform the network request
        do {
