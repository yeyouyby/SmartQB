using System;
using System.Linq;
using System.Threading.Tasks;
using SmartQB.Core.Entities;
using SmartQB.Core.Interfaces;
using SmartQB.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace SmartQB.Infrastructure.Services;

public class TaggingService : ITaggingService
{
    private readonly ILLMService _llmService;
    private readonly IVectorService _vectorService;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<TaggingService> _logger;

    public TaggingService(ILLMService llmService, IVectorService vectorService, IServiceScopeFactory scopeFactory, ILogger<TaggingService> logger)
    {
        _llmService = llmService;
        _vectorService = vectorService;
        _scopeFactory = scopeFactory;
        _logger = logger;
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
                _logger.LogError(ex, "Backfill error for tag {TagName}", tagName);
            }
        });

        return Task.CompletedTask;
    }

    private async Task ProcessBackfillAsync(int tagId, string tagName, string tagDef)
    {
        // 1. Find candidates using vector search
        var candidates = await _vectorService.SearchSimilarAsync(tagDef, limit: 50);

        if (!candidates.Any()) return;

        // Use a single scope for the batch
        using (var scope = _scopeFactory.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<SmartQBDbContext>();

            // We need to operate on questions within THIS context.
            // Candidates came from VectorService which has closed its context.
            // So we iterate through candidates and re-fetch them in the current context.

            foreach (var candidate in candidates)
            {
                try
                {
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
                        // Use FindAsync to get the tracked entity
                        var tagInContext = await dbContext.Tags.FindAsync(tagId);

                        // If tag is not found in context (unlikely if passed from outside but good to check),
                        // we might need to attach it, but FindAsync is safer.
                        if (tagInContext != null)
                        {
                            question.Tags.Add(tagInContext);
                            // Save changes per question or batch?
                            // Batching is better for perf, but individual is safer against partial failures.
                            // Given "ProcessBackfillAsync" is background, saving per item is safer to progress.
                            await dbContext.SaveChangesAsync();
                            _logger.LogInformation("Auto-tagged Question {QuestionId} with {TagName}", question.Id, tagName);
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Error processing candidate question {QuestionId} for tag {TagName}", candidate.Id, tagName);
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

        string response = await _llmService.ChatAsync(prompt);
        return response?.Trim().ToUpper().StartsWith("YES") == true;
    }

    public Task TagQuestionAsync(int questionId)
    {
        // Placeholder implementation
        return Task.CompletedTask;
    }
}
