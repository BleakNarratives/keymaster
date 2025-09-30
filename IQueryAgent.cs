//
// IQueryAgent.cs
// EquiNexAI - Abstraction Layer
//

using System.Threading.Tasks;

/// <summary>
/// Defines the contract for an End-to-End AI Agent used for processing
/// user queries, analyzing calibration data, or generating reports.
/// </summary>
public interface IQueryAgent
{
    /// <summary>
    /// Sends a text prompt to the LLM and awaits a response.
    /// </summary>
    /// <param name="prompt">The user query or instruction.</param>
    /// <returns>The LLM's generated response text.</returns>
    Task<string> QueryText(string prompt);

    /// <summary>
    /// Sends a text prompt along with serialized data (like CalibrationResult)
    /// for advanced analysis (e.g., "Analyze this RMSE data for outliers.").
    /// </summary>
    /// <param name="prompt">The instruction for the LLM.</param>
    /// <param name="dataContext">Serialized data (e.g., JSON) to provide context.</param>
    /// <returns>The LLM's generated response text, potentially containing analysis.</returns>
    Task<string> QueryWithData(string prompt, string dataContext);
    
    // Optional: Add methods for streaming, multimodal, or tool-use later.
}