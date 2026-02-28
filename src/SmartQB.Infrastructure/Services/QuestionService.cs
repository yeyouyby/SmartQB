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

    public async Task<List<Question>> GetAllQuestionsAsync()
    {
        using var scope = _scopeFactory.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<SmartQBDbContext>();

        return await dbContext.Questions.Include(q => q.Tags).AsNoTracking().ToListAsync();
    }
}