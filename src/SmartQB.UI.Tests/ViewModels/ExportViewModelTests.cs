using System.Collections.Generic;
using System.Threading.Tasks;
using Moq;
using SmartQB.Core.Entities;
using SmartQB.Core.Interfaces;
using SmartQB.UI.ViewModels;
using Xunit;

namespace SmartQB.UI.Tests.ViewModels;

public class ExportViewModelTests
{
    private readonly Mock<IQuestionService> _mockQuestionService;
    private readonly ExportViewModel _viewModel;

    public ExportViewModelTests()
    {
        _mockQuestionService = new Mock<IQuestionService>();
        _viewModel = new ExportViewModel(_mockQuestionService.Object);
    }

    [Fact]
    public async Task GenerateHtmlAsync_WithoutAnswers_GeneratesCorrectHtml()
    {
        // Arrange
        var questions = new List<Question>
        {
            new Question { Content = "Test Content 1", Difficulty = 1.0, LogicDescriptor = "Logic 1" }
        };
        _mockQuestionService.Setup(s => s.GetAllQuestionsAsync(null)).ReturnsAsync(questions);

        // Act
        var html = await _viewModel.GenerateHtmlAsync(includeAnswers: false);

        // Assert
        Assert.Contains("Test Content 1", html);
        Assert.DoesNotContain("Logic 1", html);
        Assert.DoesNotContain("Logic/Answer", html);
        Assert.Contains("MathJax-script", html);
    }

    [Fact]
    public async Task GenerateHtmlAsync_WithAnswers_IncludesLogicDescriptors()
    {
        // Arrange
        var questions = new List<Question>
        {
            new Question { Content = "Test Content 2", Difficulty = 2.0, LogicDescriptor = "Secret Logic 2" }
        };
        _mockQuestionService.Setup(s => s.GetAllQuestionsAsync(null)).ReturnsAsync(questions);

        // Act
        var html = await _viewModel.GenerateHtmlAsync(includeAnswers: true);

        // Assert
        Assert.Contains("Test Content 2", html);
        Assert.Contains("Secret Logic 2", html);
        Assert.Contains("Logic/Answer", html);
    }
}
