using Moq;
using SmartQB.Core.Entities;
using SmartQB.Core.Interfaces;
using SmartQB.UI.ViewModels;

namespace SmartQB.UI.Tests;

public class LibraryViewModelTests
{
    [Fact]
    public async Task LoadQuestionsAsync_LoadsAllQuestions()
    {
        // Arrange
        var questionServiceMock = new Mock<IQuestionService>();
        var vectorServiceMock = new Mock<IVectorService>();
        var taggingServiceMock = new Mock<ITaggingService>();

        var mockQuestions = new List<Question>
        {
            new Question { Id = 1, Content = "Q1" },
            new Question { Id = 2, Content = "Q2" }
        };
        questionServiceMock.Setup(qs => qs.GetAllQuestionsAsync(null)).ReturnsAsync(mockQuestions);

        var vm = new LibraryViewModel(questionServiceMock.Object, vectorServiceMock.Object, taggingServiceMock.Object);

        // Act
        await vm.LoadQuestionsAsync();

        // Assert
        Assert.Equal(2, vm.Questions.Count);
        Assert.Equal("Q1", vm.Questions[0].Content);
        Assert.Equal("Q2", vm.Questions[1].Content);
    }

    [Fact]
    public async Task SearchCommand_CallsSearchSimilarAsync()
    {
        // Arrange
        var questionServiceMock = new Mock<IQuestionService>();
        var vectorServiceMock = new Mock<IVectorService>();
        var taggingServiceMock = new Mock<ITaggingService>();

        var searchResults = new List<Question>
        {
            new Question { Id = 3, Content = "Search Result Q3" }
        };
        vectorServiceMock.Setup(vs => vs.SearchSimilarAsync("test query", 10, null)).ReturnsAsync(searchResults);

        var vm = new LibraryViewModel(questionServiceMock.Object, vectorServiceMock.Object, taggingServiceMock.Object);
        vm.SearchQuery = "test query";

        // Act
        await vm.SearchCommand.ExecuteAsync(null);

        // Assert
        vectorServiceMock.Verify(vs => vs.SearchSimilarAsync("test query", 10, null), Times.Once);
        Assert.Single(vm.Questions);
        Assert.Equal("Search Result Q3", vm.Questions[0].Content);
    }

    [Fact]
    public async Task SearchCommand_EmptyQuery_LoadsAllQuestions()
    {
        // Arrange
        var questionServiceMock = new Mock<IQuestionService>();
        var vectorServiceMock = new Mock<IVectorService>();
        var taggingServiceMock = new Mock<ITaggingService>();

        var mockQuestions = new List<Question>
        {
            new Question { Id = 1, Content = "Q1" }
        };
        questionServiceMock.Setup(qs => qs.GetAllQuestionsAsync(null)).ReturnsAsync(mockQuestions);

        var vm = new LibraryViewModel(questionServiceMock.Object, vectorServiceMock.Object, taggingServiceMock.Object);
        vm.SearchQuery = "";

        // Act
        await vm.SearchCommand.ExecuteAsync(null);

        // Assert
        questionServiceMock.Verify(qs => qs.GetAllQuestionsAsync(null), Times.Once);
        Assert.Single(vm.Questions);
    }

    [Fact]
    public async Task LoadQuestionsAsync_FiltersByTag()
    {
        // Arrange
        var questionServiceMock = new Mock<IQuestionService>();
        var vectorServiceMock = new Mock<IVectorService>();
        var taggingServiceMock = new Mock<ITaggingService>();

        var tag1 = new Tag { Id = 1, Name = "Math" };

        var mockQuestions = new List<Question>
        {
            new Question { Id = 1, Content = "Q1", Tags = new List<Tag> { tag1 } }
        };
        questionServiceMock.Setup(qs => qs.GetAllQuestionsAsync(1)).ReturnsAsync(mockQuestions);

        var vm = new LibraryViewModel(questionServiceMock.Object, vectorServiceMock.Object, taggingServiceMock.Object);

        // Act - Set SelectedTag to trigger filtering
        vm.SelectedTag = tag1;

        // Assert
        questionServiceMock.Verify(qs => qs.GetAllQuestionsAsync(1), Times.Once);
        Assert.Single(vm.Questions);
    }

    [Fact]
    public async Task SearchCommand_FiltersByTag()
    {
        // Arrange
        var questionServiceMock = new Mock<IQuestionService>();
        var vectorServiceMock = new Mock<IVectorService>();
        var taggingServiceMock = new Mock<ITaggingService>();

        var tag1 = new Tag { Id = 1, Name = "Math" };

        var searchResults = new List<Question>
        {
            new Question { Id = 1, Content = "Result 1", Tags = new List<Tag> { tag1 } }
        };
        vectorServiceMock.Setup(vs => vs.SearchSimilarAsync("test query", 10, 1)).ReturnsAsync(searchResults);

        var vm = new LibraryViewModel(questionServiceMock.Object, vectorServiceMock.Object, taggingServiceMock.Object);
        vm.SearchQuery = "test query";
        vm.SelectedTag = tag1;

        // Act
        await vm.SearchCommand.ExecuteAsync(null);

        // Assert
        vectorServiceMock.Verify(vs => vs.SearchSimilarAsync("test query", 10, 1), Times.Once);
        Assert.Single(vm.Questions);
        Assert.Equal("Result 1", vm.Questions[0].Content);
    }

    [Fact]
    public void ClearFilterCommand_ClearsSelectedTag()
    {
        // Arrange
        var questionServiceMock = new Mock<IQuestionService>();
        var vectorServiceMock = new Mock<IVectorService>();
        var taggingServiceMock = new Mock<ITaggingService>();

        var vm = new LibraryViewModel(questionServiceMock.Object, vectorServiceMock.Object, taggingServiceMock.Object);
        vm.SelectedTag = new Tag { Id = 1, Name = "Math" };

        // Act
        vm.ClearFilterCommand.Execute(null);

        // Assert
        Assert.Null(vm.SelectedTag);
    }
}