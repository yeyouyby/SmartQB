using SmartQB.Core.Interfaces;

namespace SmartQB.Infrastructure.Services;

public class OpenAiLlmService : ILLMService
{
    public Task<string> ExtractLogicPathAsync(string text)
    {
        return Task.FromResult("Mock Logic Path");
    }

    public Task<List<string>> SuggestTagsAsync(string text)
    {
        return Task.FromResult(new List<string> { "Mock Tag 1", "Mock Tag 2" });
    }
}
