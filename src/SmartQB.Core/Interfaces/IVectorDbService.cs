namespace SmartQB.Core.Interfaces;

public interface IVectorDbService
{
    Task UpsertAsync(string id, float[] vector, Dictionary<string, object> metadata);
    Task<List<string>> SearchAsync(float[] vector, int limit);
}
