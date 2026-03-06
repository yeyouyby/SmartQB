using Moq;
using SmartQB.Core.Entities;
using SmartQB.Core.Interfaces;
using SmartQB.UI.ViewModels;
using System.Diagnostics;

namespace SmartQB.UI.Tests;

public class LibraryViewModelPerformanceTests
{
    [Fact]
    public async Task LoadTagsAsync_Performance_Baseline()
    {
        // Arrange
        var questionServiceMock = new Mock<IQuestionService>();
        var vectorServiceMock = new Mock<IVectorService>();
        var taggingServiceMock = new Mock<ITaggingService>();

        var mockTags = Enumerable.Range(1, 10000).Select(i => new Tag { Id = i, Name = $"Tag{i}" }).ToList();
        questionServiceMock.Setup(qs => qs.GetAllTagsAsync()).ReturnsAsync(mockTags);

        var vm = new LibraryViewModel(questionServiceMock.Object, vectorServiceMock.Object, taggingServiceMock.Object);

        // Warmup
        await vm.LoadTagsAsync();

        // Act
        var sw = Stopwatch.StartNew();
        await vm.LoadTagsAsync();
        sw.Stop();

        // Assert
        Console.WriteLine($"LoadTagsAsync with 10000 items took: {sw.ElapsedMilliseconds} ms");
        Assert.True(sw.ElapsedMilliseconds < 5000); // Sanity check
    }

    [Fact]
    public async Task LoadQuestionsAsync_Performance_Baseline()
    {
        // Arrange
        var questionServiceMock = new Mock<IQuestionService>();
        var vectorServiceMock = new Mock<IVectorService>();
        var taggingServiceMock = new Mock<ITaggingService>();

        var mockQuestions = Enumerable.Range(1, 10000).Select(i => new Question { Id = i, Content = $"Question{i}", Tags = new List<Tag>() }).ToList();
        questionServiceMock.Setup(qs => qs.GetAllQuestionsAsync()).ReturnsAsync(mockQuestions);

        var vm = new LibraryViewModel(questionServiceMock.Object, vectorServiceMock.Object, taggingServiceMock.Object);

        // Warmup
        await vm.LoadQuestionsAsync();

        // Act
        var sw = Stopwatch.StartNew();
        await vm.LoadQuestionsAsync();
        sw.Stop();

        // Assert
        Console.WriteLine($"LoadQuestionsAsync with 10000 items took: {sw.ElapsedMilliseconds} ms");
        Assert.True(sw.ElapsedMilliseconds < 5000); // Sanity check
    }
}
