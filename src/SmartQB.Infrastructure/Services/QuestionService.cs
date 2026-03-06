using System.Collections.Generic;
using System.Threading.Tasks;
using SmartQB.Core.Entities;
using SmartQB.Core.Interfaces;
using SmartQB.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace SmartQB.Infrastructure.Services;

public class QuestionService(IServiceScopeFactory scopeFactory) : IQuestionService
{
    private readonly IServiceScopeFactory _scopeFactory = scopeFactory;

    public async Task<List<Question>> GetQuestionsAsync(int? tagId = null)
    {
        using var scope = _scopeFactory.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<SmartQBDbContext>();

        var query = dbContext.Questions.Include(q => q.Tags).AsNoTracking();

        if (tagId.HasValue)
        {
            query = query.Where(q => q.Tags.Any(t => t.Id == tagId.Value));
        }

        return await query.ToListAsync();
    }

    public async Task<List<Tag>> GetAllTagsAsync()
    {
        using var scope = _scopeFactory.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<SmartQBDbContext>();

        return await dbContext.Tags.AsNoTracking().ToListAsync();
    }
}