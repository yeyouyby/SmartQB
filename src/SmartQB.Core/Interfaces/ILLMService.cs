namespace SmartQB.Core.Interfaces;

public interface ILLMService
{
    Task<string> ExtractLogicPathAsync(string text);
    Task<List<string>> SuggestTagsAsync(string text);
}
