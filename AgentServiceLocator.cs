//
// AgentServiceLocator.cs
// EquiNexAI - Service Locator
//

using System;

/// <summary>
/// Singleton service locator for the EquiNex AI Agents.
/// This ensures a single point of access and control over the active LLM.
/// </summary>
public class AgentServiceLocator : IAgentProvider
{
    // The singleton instance
    private static AgentServiceLocator _instance;
    private static readonly object Lock = new object();

    // The active IQueryAgent instance (defaults to a null/dummy agent)
    private IQueryAgent _activeAgent = new NullAgent(); 

    // Private constructor to enforce the Singleton pattern
    private AgentServiceLocator() { }

    /// <summary>
    /// Gets the single instance of the AgentServiceLocator.
    /// </summary>
    public static AgentServiceLocator Instance
    {
        get
        {
            if (_instance == null)
            {
                lock (Lock)
                {
                    if (_instance == null)
                    {
                        _instance = new AgentServiceLocator();
                    }
                }
            }
            return _instance;
        }
    }

    /// <summary>
    /// Retrieves the currently active AI agent.
    /// </summary>
    public IQueryAgent GetAgent()
    {
        return _activeAgent;
    }

    /// <summary>
    /// Sets a new IQueryAgent as the active agent (e.g., swapping Gemini for Llama).
    /// </summary>
    public void SetAgent(IQueryAgent newAgent)
    {
        _activeAgent = newAgent ?? throw new ArgumentNullException(nameof(newAgent), "The new agent cannot be null.");
        Console.WriteLine($"EquiNex AI Agent switched to: {_activeAgent.GetType().Name}");
    }
    
    // MARK: - Internal Null Agent (Fallback)

    /// <summary>
    /// A minimal agent implementation used as a fallback when no real agent is set.
    /// </summary>
    private class NullAgent : IQueryAgent
    {
        public Task<string> QueryText(string prompt)
        {
            return Task.FromResult("EquiNex AI: No active LLM agent configured. Please select an agent.");
        }

        public Task<string> QueryWithData(string prompt, string dataContext)
        {
            return Task.FromResult("EquiNex AI: No active LLM agent configured for data analysis.");
        }
    }
}