using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using SmartQB.Core.Entities;
using SmartQB.Core.Interfaces;
using SmartQB.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace SmartQB.Infrastructure.Services;

public class VectorService : IVectorService
{
    private readonly ILLMService _llmService;
    private readonly IServiceScopeFactory _scopeFactory;

    public VectorService(ILLMService llmService, IServiceScopeFactory scopeFactory)
    {
        _llmService = llmService;
        _scopeFactory = scopeFactory;
    }

    public async Task<List<Question>> SearchSimilarAsync(string query, int limit = 10)
    {
        var queryVector = await _llmService.GetEmbeddingAsync(query);
        if (queryVector.Length == 0) return new List<Question>();

        using (var scope = _scopeFactory.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<SmartQBDbContext>();
            var questions = await dbContext.Questions.AsNoTracking()
                .Where(q => q.EmbeddingJson != null)
                .ToListAsync();

            if (!questions.Any()) return new List<Question>();

            // Calculate similarities in memory
            var ranked = questions
                .Select(q =>
                {
                    try {
                        var vector = JsonSerializer.Deserialize<float[]>(q.EmbeddingJson!);
                        if (vector == null) return new { Question = q, Sim = -1.0f };
                        return new { Question = q, Sim = CosineSimilarity(queryVector, vector) };
                    } catch {
                        return new { Question = q, Sim = -1.0f };
                    }
                })
                .Where(x => x.Sim >= 0)
                .OrderByDescending(x => x.Sim)
                .Take(limit)
                .Select(x => x.Question)
                .ToList();

            return ranked;
        }
    }

    private float CosineSimilarity(float[] vector1, float[] vector2)
    {
        if (vector1.Length != vector2.Length) return 0f;

        float dotProduct = 0f;
        float magnitude1 = 0f;
        float magnitude2 = 0f;

        for (int i = 0; i < vector1.Length; i++)
        {
            dotProduct += vector1[i] * vector2[i];
            magnitude1 += vector1[i] * vector1[i];
            magnitude2 += vector2[i] * vector2[i];
        }

        if (magnitude1 == 0 || magnitude2 == 0) return 0f;

        return dotProduct / ((float)System.Math.Sqrt(magnitude1) * (float)System.Math.Sqrt(magnitude2));
    }
}
