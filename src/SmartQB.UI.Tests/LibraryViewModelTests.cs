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
        questionServiceMock.Setup(qs => qs.GetAllQuestionsAsync()).ReturnsAsync(mockQuestions);

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
        vectorServiceMock.Setup(vs => vs.SearchSimilarAsync("test query", 10)).ReturnsAsync(searchResults);

        var vm = new LibraryViewModel(questionServiceMock.Object, vectorServiceMock.Object, taggingServiceMock.Object);
        vm.SearchQuery = "test query";

        // Act
        await vm.SearchCommand.ExecuteAsync(null);

        // Assert
        vectorServiceMock.Verify(vs => vs.SearchSimilarAsync("test query", 10), Times.Once);
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
        questionServiceMock.Setup(qs => qs.GetAllQuestionsAsync()).ReturnsAsync(mockQuestions);

        var vm = new LibraryViewModel(questionServiceMock.Object, vectorServiceMock.Object, taggingServiceMock.Object);
        vm.SearchQuery = "";

        // Act
        await vm.SearchCommand.ExecuteAsync(null);

        // Assert
        questionServiceMock.Verify(qs => qs.GetAllQuestionsAsync(), Times.Once);
        Assert.Single(vm.Questions);
    }
}