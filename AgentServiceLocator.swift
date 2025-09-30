//
// AgentServiceLocator.swift
// EquiNexAI - Service Locator (Swift)
//

import Foundation

/// Singleton service locator for the EquiNex AI Agents.
/// This ensures a single point of access and control over the active LLM.
class SwiftAgentServiceLocator {
    
    static let instance = SwiftAgentServiceLocator()
    
    private var activeAgent: IQueryAgent = NullAgent()
    
    private init() {
        // Initialize with a default or null agent.
        // In a production app, you might initialize a specific agent here (e.g., NetworkAgent).
        
        // ** TEMPORARY: Set a dummy agent that mimics a successful analysis **
        // To allow the ViewController's AI button to function immediately:
        activeAgent = DebugAnalysisAgent()
    }
    
    /// Retrieves the currently active AI agent.
    func getAgent() -> IQueryAgent {
        return activeAgent
    }
    
    /// Allows the user or system to change the active LLM agent at runtime.
    func setAgent(newAgent: IQueryAgent) {
        activeAgent = newAgent
        print("EquiNex AI Agent switched to: \(type(of: newAgent))")
    }
}

// MARK: - Debug Agent (Temporary for Testing UI)

/// A temporary agent to test the UI flow without an actual API call.
class DebugAnalysisAgent: IQueryAgent {
    func queryText(prompt: String) async -> String {
        return "Debug Agent: Ready to analyze."
    }
    
    func queryWithData(prompt: String, dataContext: String) async -> String {
        // Simulate parsing the RMSE from the data context (JSON string)
        if let rmseValue = dataContext.range(of: "\"rmse\":")?.upperBound,
           let endOfValue = dataContext[rmseValue...].firstIndex(of: ","),
           let rmseString = dataContext[rmseValue..<endOfValue].split(separator: " ").first,
           let rmse = Double(rmseString) {
            
            let assessment = rmse < 0.005
                ? "Excellent: RMSE is below 5mm. Transformation is high quality and stable."
                : "Acceptable: RMSE is \(String(format: "%.4f", rmse))m. Consider recapturing point \(Int.random(in: 1...4))."
            
            return "AI Assessment: \(assessment)"
        }
        
        return "AI Assessment: Data analyzed, but could not extract RMSE value for detailed report."
    }
}