using System;
using System.Linq;
using System.Threading.Tasks;
using SmartQB.Core.Entities;
using SmartQB.Core.Interfaces;
using SmartQB.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace SmartQB.Infrastructure.Services;

public class TaggingService : ITaggingService
{
    private readonly ILLMService _llmService;
    private readonly IVectorService _vectorService;
    private readonly IServiceScopeFactory _scopeFactory;

    public TaggingService(ILLMService llmService, IVectorService vectorService, IServiceScopeFactory scopeFactory)
    {
        _llmService = llmService;
        _vectorService = vectorService;
        _scopeFactory = scopeFactory;
    }

    public Task BackfillTagAsync(Tag tag)
    {
        int tagId = tag.Id;
        string tagName = tag.Name;
        string? tagDef = tag.Definition;

        if (string.IsNullOrWhiteSpace(tagDef)) return Task.CompletedTask;

        // Fire and forget background task
        _ = Task.Run(async () =>
        {
            try
            {
                await ProcessBackfillAsync(tagId, tagName, tagDef);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Backfill error for tag {tagName}: {ex.Message}");
            }
        });

        return Task.CompletedTask;
    }

    private async Task ProcessBackfillAsync(int tagId, string tagName, string tagDef)
    {
        // 1. Find candidates using vector search
        // We use tag definition as query
        var candidates = await _vectorService.SearchSimilarAsync(tagDef, limit: 50);

        foreach (var candidate in candidates)
        {
            using (var scope = _scopeFactory.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<SmartQBDbContext>();

                // Re-fetch question with tags
                var question = await dbContext.Questions
                    .Include(q => q.Tags)
                    .FirstOrDefaultAsync(q => q.Id == candidate.Id);

                if (question == null) continue;

                // Skip if already tagged
                if (question.Tags.Any(t => t.Id == tagId)) continue;

                // 2. Ask LLM
                bool isMatch = await CheckTagMatchAsync(question, tagName, tagDef);

                if (isMatch)
                {
                    // Fetch tag from this context to attach
                    var tagInContext = await dbContext.Tags.FindAsync(tagId);
                    if (tagInContext != null)
                    {
                        question.Tags.Add(tagInContext);
                        await dbContext.SaveChangesAsync();
                        Console.WriteLine($"Auto-tagged Question {question.Id} with {tagName}");
                    }
                }
            }
        }
    }

    private async Task<bool> CheckTagMatchAsync(Question question, string tagName, string tagDef)
    {
        string prompt = $@"
Question Content:
{question.Content}

Logic Descriptor:
{question.LogicDescriptor ?? "N/A"}

Does the above question belong to the following tag?
Tag Name: {tagName}
Tag Definition: {tagDef}

Reply with strictly YES or NO.";

        // Use a simpler prompt or system prompt to enforce format?
        // ChatAsync handles it.
        string response = await _llmService.ChatAsync(prompt);
        return response.Trim().ToUpper().StartsWith("YES");
    }

    public Task TagQuestionAsync(int questionId)
    {
        // Placeholder implementation
        return Task.CompletedTask;
    }
}
