using Moq;
using Microsoft.Extensions.Logging;
using SmartQB.Core.Interfaces;
using SmartQB.UI.ViewModels;
using System.Threading.Tasks;
using Xunit;
using System;

namespace SmartQB.UI.Tests;

public class ImportViewModelTests
{
    [Fact]
    public async Task ProcessFileAsync_Success_UpdatesStatusAndCallsService()
    {
        // Arrange
        var ingestionServiceMock = new Mock<IIngestionService>();
        var loggerMock = new Mock<ILogger<ImportViewModel>>();

        var vm = new ImportViewModel(ingestionServiceMock.Object, loggerMock.Object);
        var testFilePath = "C:\\test\\document.pdf";

        // Act
        await vm.ProcessFileCommand.ExecuteAsync(testFilePath);

        // Assert
        ingestionServiceMock.Verify(s => s.ProcessPdfAsync(testFilePath), Times.Once);
        Assert.Equal("Import completed successfully!", vm.StatusMessage);
        Assert.False(vm.IsBusy);
    }

    [Fact]
    public async Task ProcessFileAsync_Exception_LogsErrorAndUpdatesStatus()
    {
        // Arrange
        var ingestionServiceMock = new Mock<IIngestionService>();
        ingestionServiceMock.Setup(s => s.ProcessPdfAsync(It.IsAny<string>()))
                            .ThrowsAsync(new Exception("Test failure"));

        var loggerMock = new Mock<ILogger<ImportViewModel>>();

        var vm = new ImportViewModel(ingestionServiceMock.Object, loggerMock.Object);
        var testFilePath = "C:\\test\\document.pdf";

        // Act
        await vm.ProcessFileCommand.ExecuteAsync(testFilePath);

        // Assert
        ingestionServiceMock.Verify(s => s.ProcessPdfAsync(testFilePath), Times.Once);
        Assert.Equal("An error occurred during import.", vm.StatusMessage);
        Assert.False(vm.IsBusy);

        // Ensure logger was called for an error
        loggerMock.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => true),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception?, string>>((v, t) => true)),
            Times.Once);
    }

    [Fact]
    public async Task ProcessFileAsync_Busy_DoesNotCallServiceAgain()
    {
        // Arrange
        var ingestionServiceMock = new Mock<IIngestionService>();

        // Make the service call block indefinitely so IsBusy stays true
        var tcs = new TaskCompletionSource();
        ingestionServiceMock.Setup(s => s.ProcessPdfAsync(It.IsAny<string>()))
                            .Returns(tcs.Task);

        var loggerMock = new Mock<ILogger<ImportViewModel>>();
        var vm = new ImportViewModel(ingestionServiceMock.Object, loggerMock.Object);
        var testFilePath = "C:\\test\\document.pdf";

        // Act
        // Start first call without awaiting so IsBusy is set
        var task1 = vm.ProcessFileCommand.ExecuteAsync(testFilePath);

        // Wait briefly to ensure IsBusy is set (synchronous part of async method runs)
        await Task.Yield();

        Assert.True(vm.IsBusy);

        // Try second call while busy
        await vm.ProcessFileCommand.ExecuteAsync(testFilePath);

        // Assert
        // Service should only be called once because the second call returned early
        ingestionServiceMock.Verify(s => s.ProcessPdfAsync(testFilePath), Times.Once);

        // Cleanup: allow first task to finish to avoid unobserved exceptions
        tcs.SetResult();
        await task1;
    }
}
