using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SmartQB.Core.Entities;
using SmartQB.Core.Interfaces;
using SmartQB.UI.ViewModels;
using Xunit;

namespace SmartQB.UI.Tests;

public class MainViewModelTests
{
    private class FakeVersionService : IVersionService
    {
        public string GetVersion() => "1.0.0-test";
    }

    private class FakeIngestionService : IIngestionService
    {
        public event EventHandler<int>? QuestionIngested;
        public Task ProcessPdfAsync(string filePath) => Task.CompletedTask;
    }

    private class FakeQuestionService : IQuestionService
    {
        public Task<List<Question>> GetAllQuestionsAsync() => Task.FromResult(new List<Question>());
        public Task<Question?> GetQuestionAsync(int id) => Task.FromResult<Question?>(null);
    }

    private class FakeVectorService : IVectorService
    {
        public Task AddVectorAsync(int questionId, float[] vector) => Task.CompletedTask;
        public Task<List<Question>> SearchSimilarAsync(string query, int limit = 10) => Task.FromResult(new List<Question>());
    }

    [Fact]
    public void NavigateCommand_ChangesViewModel_Successfully()
    {
        // Arrange
        var ingestionService = new FakeIngestionService();
        var questionService = new FakeQuestionService();
        var vectorService = new FakeVectorService();
        var versionService = new FakeVersionService();

        var importVM = new ImportViewModel(ingestionService);
        var libraryVM = new LibraryViewModel(questionService, ingestionService, vectorService);
        var exportVM = new ExportViewModel(questionService);

        var mainVM = new MainViewModel(importVM, libraryVM, exportVM, versionService);

        // Assert initial state
        Assert.Same(importVM, mainVM.CurrentViewModel);

        // Act & Assert Library
        mainVM.NavigateCommand.Execute("Library");
        Assert.Same(libraryVM, mainVM.CurrentViewModel);

        // Act & Assert Export
        mainVM.NavigateCommand.Execute("Export");
        Assert.Same(exportVM, mainVM.CurrentViewModel);

        // Act & Assert Import
        mainVM.NavigateCommand.Execute("Import");
        Assert.Same(importVM, mainVM.CurrentViewModel);
    }
}
