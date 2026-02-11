using SmartQB.Core.Interfaces;

namespace SmartQB.Infrastructure.Services;

public class ChromaVectorService : IVectorDbService
{
    public Task UpsertAsync(string id, float[] vector, Dictionary<string, object> metadata)
    {
        return Task.CompletedTask;
    }

    public Task<List<string>> SearchAsync(float[] vector, int limit)
    {
        return Task.FromResult(new List<string>());
    }
}
