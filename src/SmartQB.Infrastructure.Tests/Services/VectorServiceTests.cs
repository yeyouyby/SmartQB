using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
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

namespace SmartQB.Infrastructure.Tests.Services;

public class VectorServiceTests : IDisposable
{
    private readonly Mock<ILLMService> _llmServiceMock;
    private readonly Mock<IServiceScopeFactory> _scopeFactoryMock;
    private readonly Mock<IServiceScope> _scopeMock;
    private readonly Mock<IServiceProvider> _serviceProviderMock;
    private readonly Mock<ILogger<VectorService>> _loggerMock;
    private readonly SmartQBDbContext _dbContext;

    public VectorServiceTests()
    {
        _llmServiceMock = new Mock<ILLMService>();
        _scopeFactoryMock = new Mock<IServiceScopeFactory>();
        _scopeMock = new Mock<IServiceScope>();
        _serviceProviderMock = new Mock<IServiceProvider>();
        _loggerMock = new Mock<ILogger<VectorService>>();

        // Setup In-Memory DB
        var options = new DbContextOptionsBuilder<SmartQBDbContext>()
            .UseSqlite("DataSource=:memory:")
            .Options;

        _dbContext = new SmartQBDbContext(options);
        _dbContext.Database.OpenConnection();
        _dbContext.Database.EnsureCreated();

        // Setup Scope Factory to return our DbContext
        _serviceProviderMock.Setup(x => x.GetService(typeof(SmartQBDbContext))).Returns(_dbContext);
        _scopeMock.Setup(x => x.ServiceProvider).Returns(_serviceProviderMock.Object);
        _scopeFactoryMock.Setup(x => x.CreateScope()).Returns(_scopeMock.Object);
    }

    public void Dispose()
    {
        _dbContext.Database.CloseConnection();
        _dbContext.Dispose();
    }

    [Fact]
    public async Task SearchSimilarAsync_WithMismatchedVectorSize_IgnoresQuestion()
    {
        // Arrange
        var queryVector = new float[] { 1f, 0f, 0f };
        _llmServiceMock.Setup(x => x.GetEmbeddingAsync(It.IsAny<string>()))
            .ReturnsAsync(queryVector);

        var validVector = new float[] { 1f, 0f, 0f };
        var mismatchedVector = new float[] { 1f, 0f }; // Length 2 vs 3

        _dbContext.Questions.Add(new Question { Id = 1, Content = "Q1", EmbeddingJson = JsonSerializer.Serialize(validVector) });
        _dbContext.Questions.Add(new Question { Id = 2, Content = "Q2", EmbeddingJson = JsonSerializer.Serialize(mismatchedVector) });
        await _dbContext.SaveChangesAsync();

        var service = new VectorService(_llmServiceMock.Object, _scopeFactoryMock.Object, _loggerMock.Object);

        // Act
        var results = await service.SearchSimilarAsync("test");

        // Assert
        Assert.Single(results);
        Assert.Equal(1, results[0].Id); // Only the one with matching size is returned
    }

    [Fact]
    public async Task SearchSimilarAsync_WithZeroMagnitudeVector_IgnoresQuestion()
    {
        // Arrange
        var queryVector = new float[] { 1f, 0f, 0f };
        _llmServiceMock.Setup(x => x.GetEmbeddingAsync(It.IsAny<string>()))
            .ReturnsAsync(queryVector);

        var validVector = new float[] { 1f, 0f, 0f };
        var zeroVector = new float[] { 0f, 0f, 0f }; // Zero magnitude

        _dbContext.Questions.Add(new Question { Id = 3, Content = "Q3", EmbeddingJson = JsonSerializer.Serialize(validVector) });
        _dbContext.Questions.Add(new Question { Id = 4, Content = "Q4", EmbeddingJson = JsonSerializer.Serialize(zeroVector) });
        await _dbContext.SaveChangesAsync();

        var service = new VectorService(_llmServiceMock.Object, _scopeFactoryMock.Object, _loggerMock.Object);

        // Act
        var results = await service.SearchSimilarAsync("test");

        // Assert
        Assert.Single(results);
        Assert.Equal(3, results[0].Id); // Only the valid one is returned
    }

    [Fact]
    public async Task SearchSimilarAsync_WithQueryZeroMagnitude_ReturnsEmptyList()
    {
        // Arrange
        var queryVector = new float[] { 0f, 0f, 0f }; // Zero magnitude
        _llmServiceMock.Setup(x => x.GetEmbeddingAsync(It.IsAny<string>()))
            .ReturnsAsync(queryVector);

        var validVector = new float[] { 1f, 0f, 0f };

        _dbContext.Questions.Add(new Question { Id = 5, Content = "Q5", EmbeddingJson = JsonSerializer.Serialize(validVector) });
        await _dbContext.SaveChangesAsync();

        var service = new VectorService(_llmServiceMock.Object, _scopeFactoryMock.Object, _loggerMock.Object);

        // Act
        var results = await service.SearchSimilarAsync("test");

        // Assert
        Assert.Empty(results); // Both magnitude1 and magnitude2 are checked for 0, so similarity is null and question is filtered out
    }
}
