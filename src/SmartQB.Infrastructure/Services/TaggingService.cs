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

public class TaggingService(ILLMService llmService, IVectorService vectorService, IServiceScopeFactory scopeFactory, ILogger<TaggingService> logger) : ITaggingService
{
    private readonly ILLMService _llmService = llmService;
    private readonly IVectorService _vectorService = vectorService;
    private readonly IServiceScopeFactory _scopeFactory = scopeFactory;
    private readonly ILogger<TaggingService> _logger = logger;

    public event EventHandler? QuestionProcessed;

    public Task BackfillTagAsync(Tag tag)
    {
        int tagId = tag.Id;
        string tagName = tag.Name;
        string? tagDef = tag.Definition;

        if (string.IsNullOrWhiteSpace(tagDef)) return Task.CompletedTask;

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
        var candidates = await _vectorService.SearchSimilarAsync(tagDef, limit: 50);

        if (!candidates.Any()) return;

        using (var scope = _scopeFactory.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<SmartQBDbContext>();

            foreach (var candidate in candidates)
            {
                try
                {
                    var question = await dbContext.Questions
                        .Include(q => q.Tags)
                        .FirstOrDefaultAsync(q => q.Id == candidate.Id);

                    if (question == null) continue;

                    if (question.Tags.Any(t => t.Id == tagId)) continue;

                    bool isMatch = await CheckTagMatchAsync(question, tagName, tagDef);

                    if (isMatch)
                    {
                        var tagInContext = await dbContext.Tags.FindAsync(tagId);

                        if (tagInContext != null)
                        {
                            question.Tags.Add(tagInContext);
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

    public async Task TagQuestionAsync(int questionId)
    {
        using var scope = _scopeFactory.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<SmartQBDbContext>();

        var question = await dbContext.Questions
            .Include(q => q.Tags)
            .FirstOrDefaultAsync(q => q.Id == questionId);

        if (question == null)
        {
            _logger.LogWarning("Question {QuestionId} not found for tagging.", questionId);
            return;
        }

        var allTags = await dbContext.Tags.Where(t => t.Definition != null && t.Definition != "").ToListAsync();

        foreach (var tag in allTags)
        {
            if (question.Tags.Any(t => t.Id == tag.Id)) continue;

            bool isMatch = await CheckTagMatchAsync(question, tag.Name, tag.Definition!);

            if (isMatch)
            {
                question.Tags.Add(tag);
                _logger.LogInformation("Auto-tagged Question {QuestionId} with {TagName}", question.Id, tag.Name);
            }
        }

        await dbContext.SaveChangesAsync();

        QuestionProcessed?.Invoke(this, EventArgs.Empty);
    }
}
