using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SmartQB.Core.Entities;
using SmartQB.Core.Interfaces;
using SmartQB.UI.ViewModels;
using Xunit;

namespace SmartQB.UI.Tests;

public class LibraryViewModelTests
{
    private class FakeIngestionService : IIngestionService
    {
#pragma warning disable CS0067 // The event is never used
        public event EventHandler<int>? QuestionIngested;
#pragma warning restore CS0067
        public Task ProcessPdfAsync(string filePath) => Task.CompletedTask;
    }

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

    private class FakeVectorService : IVectorService
    {
        private readonly List<Question> _searchResults;

        public FakeVectorService(List<Question> searchResults)
        {
            _searchResults = searchResults;
        }

        public Task AddVectorAsync(int questionId, float[] vector) => Task.CompletedTask;
        public Task<List<Question>> SearchSimilarAsync(string query, int limit = 10) => Task.FromResult(_searchResults);
    }

    [Fact]
    public async Task LoadQuestionsAsync_PopulatesCollection()
    {
        // Arrange
        var fakeQuestions = new List<Question>
        {
            new Question { Id = 1, Content = "Q1" },
            new Question { Id = 2, Content = "Q2" }
        };

        var questionService = new FakeQuestionService(fakeQuestions);
        var ingestionService = new FakeIngestionService();
        var vectorService = new FakeVectorService(new List<Question>());

        var viewModel = new LibraryViewModel(questionService, ingestionService, vectorService);

        // Act
        await viewModel.LoadQuestionsAsync();

        // Assert
        Assert.Equal(2, viewModel.Questions.Count);
        Assert.Equal("Q1", viewModel.Questions[0].Content);
        Assert.Equal("Q2", viewModel.Questions[1].Content);
    }

    [Fact]
    public async Task SearchCommand_CallsVectorService_UpdatesCollection()
    {
        // Arrange
        var allQuestions = new List<Question>
        {
            new Question { Id = 1, Content = "Math Q1" },
            new Question { Id = 2, Content = "Physics Q1" },
            new Question { Id = 3, Content = "Math Q2" }
        };

        var searchResults = new List<Question>
        {
            new Question { Id = 1, Content = "Math Q1" },
            new Question { Id = 3, Content = "Math Q2" }
        };

        var questionService = new FakeQuestionService(allQuestions);
        var ingestionService = new FakeIngestionService();
        var vectorService = new FakeVectorService(searchResults);

        var viewModel = new LibraryViewModel(questionService, ingestionService, vectorService);

        // Initial Load
        await viewModel.LoadQuestionsAsync();
        Assert.Equal(3, viewModel.Questions.Count);

        // Act
        viewModel.SearchQuery = "Math";
        await viewModel.SearchCommand.ExecuteAsync(null);

        // Assert
        Assert.Equal(2, viewModel.Questions.Count);
        Assert.Equal("Math Q1", viewModel.Questions[0].Content);
        Assert.Equal("Math Q2", viewModel.Questions[1].Content);
    }

    [Fact]
    public async Task ClearSearchCommand_ResetsQueryAndReloads()
    {
        // Arrange
        var allQuestions = new List<Question>
        {
            new Question { Id = 1, Content = "Math Q1" }
        };

        var questionService = new FakeQuestionService(allQuestions);
        var ingestionService = new FakeIngestionService();
        var vectorService = new FakeVectorService(new List<Question>());

        var viewModel = new LibraryViewModel(questionService, ingestionService, vectorService);
        viewModel.SearchQuery = "Old Search";

        // Act
        await viewModel.ClearSearchCommand.ExecuteAsync(null);

        // Assert
        Assert.Equal(string.Empty, viewModel.SearchQuery);
        Assert.Single(viewModel.Questions);
    }
}
