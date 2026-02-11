using System.Threading.Tasks;

namespace SmartQB.Core.Interfaces;

public interface ILLMService
{
    /// <summary>
    /// Sends a prompt to the LLM and returns the text response.
    /// </summary>
    Task<string> ChatAsync(string prompt, string? systemPrompt = null);

    /// <summary>
    /// Sends an image and a prompt to the LLM (Vision capabilities).
    /// </summary>
    Task<string> AnalyzeImageAsync(byte[] imageBytes, string prompt);
}
