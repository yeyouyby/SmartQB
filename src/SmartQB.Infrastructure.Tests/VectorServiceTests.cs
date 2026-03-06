using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using SmartQB.Core.Entities;
using SmartQB.Core.Interfaces;
using SmartQB.Infrastructure.Data;
using SmartQB.Infrastructure.Services;
using Xunit;

namespace SmartQB.Infrastructure.Tests;

public class VectorServiceTests
{
    private readonly Mock<ILLMService> _mockLlmService;
    private readonly Mock<IServiceScopeFactory> _mockScopeFactory;
    private readonly Mock<IServiceScope> _mockScope;
    private readonly Mock<IServiceProvider> _mockServiceProvider;
    private readonly Mock<ILogger<VectorService>> _mockLogger;
    private readonly SmartQBDbContext _dbContext;
    private readonly VectorService _vectorService;

    public VectorServiceTests()
    {
        _mockLlmService = new Mock<ILLMService>();
        _mockScopeFactory = new Mock<IServiceScopeFactory>();
        _mockScope = new Mock<IServiceScope>();
        _mockServiceProvider = new Mock<IServiceProvider>();
        _mockLogger = new Mock<ILogger<VectorService>>();

        var options = new DbContextOptionsBuilder<SmartQBDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        _dbContext = new SmartQBDbContext(options);

        _mockServiceProvider.Setup(x => x.GetService(typeof(SmartQBDbContext))).Returns(_dbContext);
        _mockScope.Setup(x => x.ServiceProvider).Returns(_mockServiceProvider.Object);
        _mockScopeFactory.Setup(x => x.CreateScope()).Returns(_mockScope.Object);

        _vectorService = new VectorService(
            _mockLlmService.Object,
            _mockScopeFactory.Object,
            _mockLogger.Object);
    }

    [Fact]
    public async Task SearchSimilarAsync_WithInvalidJsonEmbedding_LogsWarningAndIgnoresQuestion()
    {
        // Arrange
        var query = "test query";
        var queryEmbedding = new float[] { 1f, 0f, 0f };
        _mockLlmService.Setup(x => x.GetEmbeddingAsync(query))
            .ReturnsAsync(queryEmbedding);

        var validQuestion = new Question
        {
            Id = 1,
            Content = "Valid Question",
            EmbeddingJson = "[1.0, 0.0, 0.0]"
        };

        var invalidQuestion = new Question
        {
            Id = 2,
            Content = "Invalid JSON Question",
            EmbeddingJson = "{ invalid json ]"
        };

        _dbContext.Questions.Add(validQuestion);
        _dbContext.Questions.Add(invalidQuestion);
        await _dbContext.SaveChangesAsync();

        // Act
        var results = await _vectorService.SearchSimilarAsync(query, 10);

        // Assert
        Assert.NotNull(results);
        Assert.Single(results);
        Assert.Equal(validQuestion.Id, results[0].Id);

        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Failed to deserialize embedding for question 2")),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception?, string>>((v, t) => true)),
            Times.Once);
    }

    [Fact]
    public async Task SearchSimilarAsync_WithEmptyQueryEmbedding_ReturnsEmptyList()
    {
        // Arrange
        _mockLlmService.Setup(x => x.GetEmbeddingAsync(It.IsAny<string>()))
            .ReturnsAsync(Array.Empty<float>());

        // Act
        var results = await _vectorService.SearchSimilarAsync("test", 10);

        // Assert
        Assert.NotNull(results);
        Assert.Empty(results);
    }

    [Fact]
    public async Task SearchSimilarAsync_WithNullEmbeddingJson_IgnoresQuestion()
    {
        // Arrange
        var queryEmbedding = new float[] { 1f, 0f, 0f };
        _mockLlmService.Setup(x => x.GetEmbeddingAsync(It.IsAny<string>()))
            .ReturnsAsync(queryEmbedding);

        var nullJsonQuestion = new Question
        {
            Id = 3,
            Content = "Null JSON",
            EmbeddingJson = null
        };
        var validQuestion = new Question
        {
            Id = 4,
            Content = "Valid JSON",
            EmbeddingJson = "[1.0, 0.0, 0.0]"
        };

        _dbContext.Questions.Add(nullJsonQuestion);
        _dbContext.Questions.Add(validQuestion);
        await _dbContext.SaveChangesAsync();

        // Act
        var results = await _vectorService.SearchSimilarAsync("test", 10);

        // Assert
        Assert.Single(results);
        Assert.Equal(validQuestion.Id, results[0].Id);
    }

    [Fact]
    public async Task SearchSimilarAsync_WithMismatchedVectorLength_IgnoresQuestion()
    {
        // Arrange
        var queryEmbedding = new float[] { 1f, 0f, 0f };
        _mockLlmService.Setup(x => x.GetEmbeddingAsync(It.IsAny<string>()))
            .ReturnsAsync(queryEmbedding);

        var mismatchedQuestion = new Question
        {
            Id = 5,
            Content = "Mismatched Length",
            EmbeddingJson = "[1.0, 0.0]"
        };
        var validQuestion = new Question
        {
            Id = 6,
            Content = "Valid Length",
            EmbeddingJson = "[1.0, 0.0, 0.0]"
        };

        _dbContext.Questions.Add(mismatchedQuestion);
        _dbContext.Questions.Add(validQuestion);
        await _dbContext.SaveChangesAsync();

        // Act
        var results = await _vectorService.SearchSimilarAsync("test", 10);

        // Assert
        Assert.Single(results);
        Assert.Equal(validQuestion.Id, results[0].Id);
    }
}
