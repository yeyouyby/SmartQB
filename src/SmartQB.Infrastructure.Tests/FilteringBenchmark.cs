using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SmartQB.Core.Entities;
using SmartQB.Infrastructure.Data;
using SmartQB.Infrastructure.Services;
using Xunit;
using Xunit.Abstractions;

namespace SmartQB.Infrastructure.Tests;

public class FilteringBenchmark
{
    private readonly ITestOutputHelper _output;

    public FilteringBenchmark(ITestOutputHelper output)
    {
        _output = output;
    }

    [Fact]
    public async Task MeasureBaselinePerformance()
    {
        // Setup in-memory database
        var serviceProvider = new ServiceCollection()
            .AddEntityFrameworkSqlite()
            .BuildServiceProvider();

        var builder = new DbContextOptionsBuilder<SmartQBDbContext>();
        builder.UseSqlite("DataSource=:memory:")
               .UseInternalServiceProvider(serviceProvider);

        var options = builder.Options;
        using var context = new SmartQBDbContext(options);
        context.Database.OpenConnection();
        context.Database.EnsureCreated();

        // Populate data
        int questionCount = 1000;
        int tagCount = 5;
        var tags = Enumerable.Range(1, tagCount).Select(i => new Tag { Name = $"Tag{i}" }).ToList();
        context.Tags.AddRange(tags);
        await context.SaveChangesAsync();

        var questions = new List<Question>();
        for (int i = 0; i < questionCount; i++)
        {
            var q = new Question
            {
                Content = $"Question content {i}",
                Difficulty = i % 5,
                Tags = new List<Tag> { tags[i % tagCount] }
            };
            questions.Add(q);
        }
        context.Questions.AddRange(questions);
        await context.SaveChangesAsync();

        var selectedTag = tags[0];

        // Benchmark in-memory filtering (current approach)
        var sw = Stopwatch.StartNew();

        // Simulating current GetAllQuestionsAsync + manual filtering
        var allQuestions = await context.Questions.Include(q => q.Tags).AsNoTracking().ToListAsync();
        var filteredQuestions = new List<Question>();
        foreach (var q in allQuestions)
        {
            bool hasTag = false;
            foreach (var t in q.Tags)
            {
                if (t.Id == selectedTag.Id)
                {
                    hasTag = true;
                    break;
                }
            }
            if (hasTag) filteredQuestions.Add(q);
        }

        sw.Stop();
        _output.WriteLine($"Baseline (In-memory filtering) for {questionCount} questions: {sw.ElapsedMilliseconds}ms. Count: {filteredQuestions.Count}");

        Assert.Equal(questionCount / tagCount, filteredQuestions.Count);

        // Optimized approach
        sw.Restart();

        var query = context.Questions.Include(q => q.Tags).AsNoTracking();
        query = query.Where(q => q.Tags.Any(t => t.Id == selectedTag.Id));
        var optimizedResults = await query.ToListAsync();

        sw.Stop();
        _output.WriteLine($"Optimized (Database filtering) for {questionCount} questions: {sw.ElapsedMilliseconds}ms. Count: {optimizedResults.Count}");

        Assert.Equal(questionCount / tagCount, optimizedResults.Count);
    }
}
