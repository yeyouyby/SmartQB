using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using SmartQB.Core.Entities;
using SmartQB.Core.Interfaces;
using SmartQB.UI.ViewModels;
using Xunit;

namespace SmartQB.UI.Tests.ViewModels;

public class LibraryViewModelTests
{
    private readonly Mock<IQuestionService> _mockQuestionService;
    private readonly Mock<IVectorService> _mockVectorService;
    private readonly Mock<ITaggingService> _mockTaggingService;
    private readonly Mock<ILogger<LibraryViewModel>> _mockLogger;
    private readonly LibraryViewModel _viewModel;

    public LibraryViewModelTests()
    {
        _mockQuestionService = new Mock<IQuestionService>();
        _mockVectorService = new Mock<IVectorService>();
        _mockTaggingService = new Mock<ITaggingService>();
        _mockLogger = new Mock<ILogger<LibraryViewModel>>();

        _viewModel = new LibraryViewModel(
            _mockQuestionService.Object,
            _mockVectorService.Object,
            _mockTaggingService.Object,
            _mockLogger.Object);
    }

    [Fact]
    public async Task LoadTagsAsync_PopulatesTagsCollection()
    {
        // Arrange
        var tags = new List<Tag> { new Tag { Id = 1, Name = "Algebra" }, new Tag { Id = 2, Name = "Calculus" } };
        _mockQuestionService.Setup(s => s.GetAllTagsAsync()).ReturnsAsync(tags);

        // Act
        await _viewModel.LoadTagsAsync();

        // Assert
        Assert.Equal(2, _viewModel.Tags.Count);
        Assert.Equal("Algebra", _viewModel.Tags[0].Name);
    }

    [Fact]
    public async Task LoadQuestionsAsync_PopulatesQuestionsCollection()
    {
        // Arrange
        var questions = new List<Question> { new Question { Content = "Q1" } };
        _mockQuestionService.Setup(s => s.GetAllQuestionsAsync(null)).ReturnsAsync(questions);

        // Act
        await _viewModel.LoadQuestionsAsync();

        // Assert
        Assert.Single(_viewModel.Questions);
        Assert.Equal("Q1", _viewModel.Questions[0].Content);
    }

    [Fact]
    public async Task SearchAsync_WithValidQuery_PopulatesFromVectorService()
    {
        // Arrange
        _viewModel.SearchQuery = "find math";
        var searchResults = new List<Question> { new Question { Content = "Math Q1" } };
        _mockVectorService.Setup(s => s.SearchSimilarAsync("find math", 10, null)).ReturnsAsync(searchResults);

        // Act
        await _viewModel.SearchCommand.ExecuteAsync(null);

        // Assert
        Assert.Single(_viewModel.Questions);
        Assert.Equal("Math Q1", _viewModel.Questions[0].Content);
        _mockVectorService.Verify(v => v.SearchSimilarAsync("find math", 10, null), Times.Once);
        Assert.False(_viewModel.IsSearching);
    }

    [Fact]
    public async Task SearchAsync_WithEmptyQuery_LoadsAllQuestions()
    {
        // Arrange
        _viewModel.SearchQuery = "   ";
        var allQuestions = new List<Question> { new Question { Content = "All Q1" } };
        _mockQuestionService.Setup(s => s.GetAllQuestionsAsync(null)).ReturnsAsync(allQuestions);

        // Act
        await _viewModel.SearchCommand.ExecuteAsync(null);

        // Assert
        Assert.Single(_viewModel.Questions);
        Assert.Equal("All Q1", _viewModel.Questions[0].Content);
        _mockVectorService.Verify(v => v.SearchSimilarAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int?>()), Times.Never);
    }

    [Fact]
    public async Task ClearFilter_SetsSelectedTagToNull()
    {
        // Arrange
        _viewModel.SelectedTag = new Tag { Id = 1, Name = "Temp" };

        // Act
        _viewModel.ClearFilterCommand.Execute(null);

        // Assert
        Assert.Null(_viewModel.SelectedTag);
    }
}
