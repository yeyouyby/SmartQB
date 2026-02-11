using SmartQB.Core.Interfaces;

namespace SmartQB.Infrastructure.Services;

public class PdfiumService : IPdfService
{
    public Task<List<string>> ExtractPageTextAsync(string filePath)
    {
        return Task.FromResult(new List<string> { "Mock Page Text" });
    }

    public Task<byte[]> RenderPageAsync(string filePath, int pageIndex)
    {
        return Task.FromResult(Array.Empty<byte>());
    }
}
