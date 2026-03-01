using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SmartQB.Core.Entities;
using SmartQB.Core.Interfaces;
using SmartQB.UI.ViewModels;
using Xunit;

namespace SmartQB.UI.Tests;

public class ExportViewModelTests
{
    private class FakeQuestionService : IQuestionService
    {
        private readonly List<Question> _questions;

        public FakeQuestionService(List<Question> questions)
        {
            _questions = questions;
        }

        public Task<List<Question>> GetAllQuestionsAsync() => Task.FromResult(_questions);
        public Task<Question?> GetQuestionAsync(int id) => Task.FromResult(_questions.FirstOrDefault(q => q.Id == id));
    }

    [Fact]
    public async Task GenerateHtmlAsync_IncludesQuestionsAndAnswers()
    {
        // Arrange
        var fakeQuestions = new List<Question>
        {
            new Question { Id = 1, Content = "Find the derivative of $x^2$.", LogicDescriptor = "Use power rule: $2x$." }
        };

        var questionService = new FakeQuestionService(fakeQuestions);
        var viewModel = new ExportViewModel(questionService);

        // Act
        string html = await viewModel.GenerateHtmlAsync(includeAnswers: true);

        // Assert
        Assert.Contains("Find the derivative of $x^2$.", html);
        Assert.Contains("Logic/Answer:", html);
        Assert.Contains("Use power rule: $2x$.", html);
    }

    [Fact]
    public async Task GenerateHtmlAsync_ExcludesAnswers_WhenFlagIsFalse()
    {
        // Arrange
        var fakeQuestions = new List<Question>
        {
            new Question { Id = 1, Content = "Find the integral of $2x$.", LogicDescriptor = "Use power rule: $x^2$." }
        };

        var questionService = new FakeQuestionService(fakeQuestions);
        var viewModel = new ExportViewModel(questionService);

        // Act
        string html = await viewModel.GenerateHtmlAsync(includeAnswers: false);

        // Assert
        Assert.Contains("Find the integral of $2x$.", html);
        Assert.DoesNotContain("Logic/Answer:", html);
        Assert.DoesNotContain("Use power rule: $x^2$.", html);
    }
}
