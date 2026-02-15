using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SmartQB.Core.Entities;
using SmartQB.Core.Interfaces;
using SmartQB.Infrastructure.Data;

namespace SmartQB.Infrastructure.Services;

public class QuestionService : IQuestionService
{
    private readonly IServiceScopeFactory _scopeFactory;

    public QuestionService(IServiceScopeFactory scopeFactory)
    {
        _scopeFactory = scopeFactory;
    }

    public async Task<List<Question>> GetQuestionsAsync(int? tagId = null, int limit = 50)
    {
        using var scope = _scopeFactory.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<SmartQBDbContext>();

        IQueryable<Question> query = dbContext.Questions.AsNoTracking();

        if (tagId.HasValue)
        {
            query = query.Where(q => q.Tags.Any(t => t.Id == tagId.Value));
        }

        return await query
            .Include(q => q.Tags) // Include tags for display
            .OrderByDescending(q => q.Id) // Show newest first
            .Take(limit)
            .ToListAsync();
    }

    public async Task<List<Tag>> GetTagsAsync()
    {
        using var scope = _scopeFactory.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<SmartQBDbContext>();

        return await dbContext.Tags.AsNoTracking()
            .OrderBy(t => t.Name)
            .ToListAsync();
    }
}
