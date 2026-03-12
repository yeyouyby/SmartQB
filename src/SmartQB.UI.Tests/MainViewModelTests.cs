using Moq;
using SmartQB.Core.Interfaces;
using SmartQB.UI.ViewModels;

namespace SmartQB.UI.Tests;

public class MainViewModelTests
{
    [Fact]
    public void Navigate_UpdatesCurrentViewModel()
    {
        // Arrange
        var versionServiceMock = new Mock<IVersionService>();
        versionServiceMock.Setup(vs => vs.GetVersion()).Returns("1.0.0");

        var ingestionServiceMock = new Mock<IIngestionService>();
        var importVM = new ImportViewModel(ingestionServiceMock.Object, new Mock<Microsoft.Extensions.Logging.ILogger<ImportViewModel>>().Object);

        var questionServiceMock = new Mock<IQuestionService>();
        var vectorServiceMock = new Mock<IVectorService>();
        var taggingServiceMock = new Mock<ITaggingService>();
        var libraryVM = new LibraryViewModel(questionServiceMock.Object, vectorServiceMock.Object, taggingServiceMock.Object, new Mock<Microsoft.Extensions.Logging.ILogger<LibraryViewModel>>().Object);

        var exportVM = new ExportViewModel(questionServiceMock.Object);
        var settingsServiceMock = new Mock<ISettingsService>();
        var settingsVM = new SettingsViewModel(settingsServiceMock.Object);

        var mainVM = new MainViewModel(importVM, libraryVM, exportVM, settingsVM, versionServiceMock.Object);

        // Act & Assert
        mainVM.NavigateCommand.Execute("Library");
        Assert.Equal(libraryVM, mainVM.CurrentViewModel);

        mainVM.NavigateCommand.Execute("Export");
        Assert.Equal(exportVM, mainVM.CurrentViewModel);

        mainVM.NavigateCommand.Execute("Import");
        Assert.Equal(importVM, mainVM.CurrentViewModel);

        mainVM.NavigateCommand.Execute("Settings");
        Assert.Equal(settingsVM, mainVM.CurrentViewModel);
    }
}
