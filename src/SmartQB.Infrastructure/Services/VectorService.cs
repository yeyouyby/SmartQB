using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using SmartQB.Core.Entities;
using SmartQB.Core.Interfaces;
using SmartQB.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace SmartQB.Infrastructure.Services;

public class VectorService : IVectorService
{
    private readonly ILLMService _llmService;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<VectorService> _logger;

    public VectorService(ILLMService llmService, IServiceScopeFactory scopeFactory, ILogger<VectorService> logger)
    {
        _llmService = llmService;
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    public async Task<List<Question>> SearchSimilarAsync(string query, int limit = 10)
    {
        var queryVector = await _llmService.GetEmbeddingAsync(query);
        if (queryVector.Length == 0) return new List<Question>();

        using (var scope = _scopeFactory.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<SmartQBDbContext>();

            // Limit candidates to 1000 to avoid loading too much into memory
            var questions = await dbContext.Questions.AsNoTracking()
                .Where(q => q.EmbeddingJson != null)
                .Take(1000)
                .ToListAsync();

            if (!questions.Any()) return new List<Question>();

            // Calculate similarities in memory
            var ranked = questions
                .Select(q => new { Question = q, Sim = TryGetSimilarity(q, queryVector) })
                .Where(x => x.Sim.HasValue)
                .OrderByDescending(x => x.Sim!.Value)
                .Take(limit)
                .Select(x => x.Question)
                .ToList();

            return ranked;
        }
    }

    private float? TryGetSimilarity(Question q, float[] queryVector)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(q.EmbeddingJson)) return null;

            var vector = JsonSerializer.Deserialize<float[]>(q.EmbeddingJson);
            if (vector == null) return null;

            return CosineSimilarity(queryVector, vector);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to deserialize embedding for question {QuestionId}", q.Id);
            return null;
        }
    }

    private float? CosineSimilarity(float[] vector1, float[] vector2)
    {
        if (vector1.Length != vector2.Length) return null;

        float dotProduct = 0f;
        float magnitude1 = 0f;
        float magnitude2 = 0f;

        for (int i = 0; i < vector1.Length; i++)
        {
            dotProduct += vector1[i] * vector2[i];
            magnitude1 += vector1[i] * vector1[i];
            magnitude2 += vector2[i] * vector2[i];
        }

        if (magnitude1 == 0 || magnitude2 == 0) return null;

        return dotProduct / ((float)Math.Sqrt(magnitude1) * (float)Math.Sqrt(magnitude2));
    }
}
