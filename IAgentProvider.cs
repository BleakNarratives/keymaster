//
// IAgentProvider.cs
// EquiNexAI - Abstraction Layer
//

/// <summary>
/// Defines a contract for a service that can provide the currently active
/// IQueryAgent instance (e.g., GeminiWrapper, LlamaWrapper, etc.).
/// </summary>
public interface IAgentProvider
{
    /// <summary>
    /// Gets the singleton instance of the active AI agent.
    /// </summary>
    IQueryAgent GetAgent();

    /// <summary>
    /// Allows the user or system to change the active LLM agent at runtime.
    /// </summary>
    /// <param name="newAgent">The new IQueryAgent instance to use.</param>
    void SetAgent(IQueryAgent newAgent);
}