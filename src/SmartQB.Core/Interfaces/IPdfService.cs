namespace SmartQB.Core.Interfaces;

public interface IPdfService
{
    Task<List<string>> ExtractPageTextAsync(string filePath);
    Task<byte[]> RenderPageAsync(string filePath, int pageIndex);
}
