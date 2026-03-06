using Moq;
using SmartQB.Core.Entities;
using SmartQB.Core.Interfaces;
using SmartQB.UI.ViewModels;

namespace SmartQB.UI.Tests;

public class ExportViewModelTests
{
    [Fact]
    public async Task GenerateHtmlAsync_GeneratesHtmlWithQuestions()
    {
        // Arrange
        var questionServiceMock = new Mock<IQuestionService>();
        var mockQuestions = new List<Question>
        {
            new Question { Id = 1, Content = "Solve $x^2 = 4$", Difficulty = 2.5, LogicDescriptor = "$x = \\pm 2$" }
        };
        questionServiceMock.Setup(qs => qs.GetQuestionsAsync(It.IsAny<int?>())).ReturnsAsync(mockQuestions);

        var vm = new ExportViewModel(questionServiceMock.Object);

        // Act
        string htmlWithAnswers = await vm.GenerateHtmlAsync(true);
        string htmlWithoutAnswers = await vm.GenerateHtmlAsync(false);

        // Assert
        Assert.Contains("Solve $x^2 = 4$", htmlWithAnswers);
        Assert.Contains("Difficulty: 2.5", htmlWithAnswers);
        Assert.Contains("$x = \\pm 2$", htmlWithAnswers);

        Assert.Contains("Solve $x^2 = 4$", htmlWithoutAnswers);
        Assert.Contains("Difficulty: 2.5", htmlWithoutAnswers);
        Assert.DoesNotContain("$x = \\pm 2$", htmlWithoutAnswers);
    }
}