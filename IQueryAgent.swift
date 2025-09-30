//
// IQueryAgent.swift
// EquiNexAI - Abstraction Layer (Swift)
//

import Foundation

/// Defines the contract for an End-to-End AI Agent used for processing
/// user queries, analyzing calibration data, or generating reports.
/// Agents can be remote (calling Python/C# services) or local (e.g., CoreML).
protocol IQueryAgent {
    
    /// Sends a text prompt to the LLM and awaits a response.
    /// - Parameter prompt: The user query or instruction.
    /// - Returns: The LLM's generated response text.
    func queryText(prompt: String) async -> String
    
    /// Sends a text prompt along with serialized data (like CalibrationResult)
    /// for advanced analysis.
    /// - Parameters:
    ///   - prompt: The instruction for the LLM.
    ///   - dataContext: Serialized data (JSON string) to provide context.
    /// - Returns: The LLM's generated response text, potentially containing analysis.
    func queryWithData(prompt: String, dataContext: String) async -> String
}

// MARK: - NullAgent (Fallback)

/// A minimal agent implementation used as a fallback when no real agent is set.
class NullAgent: IQueryAgent {
    func queryText(prompt: String) async -> String {
        return "EquiNex AI: No active LLM agent configured. Please select an agent."
    }
    
    func queryWithData(prompt: String, dataContext: String) async -> String {
        return "EquiNex AI: No active LLM agent configured for data analysis."
    }
}