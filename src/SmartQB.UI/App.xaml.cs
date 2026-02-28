using System;
using System.IO;
using System.Windows;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SmartQB.Core.Interfaces;
using SmartQB.Infrastructure.Data;
using SmartQB.Infrastructure.Services;
using SmartQB.UI.ViewModels;

namespace SmartQB.UI;

public partial class App : Application
{
    public static IHost? AppHost { get; private set; }

    public App()
    {
        AppHost = Host.CreateDefaultBuilder()
            .ConfigureAppConfiguration((context, config) =>
            {
                config.SetBasePath(AppContext.BaseDirectory);
                config.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
            })
            .ConfigureServices((context, services) =>
            {
                // Core & Infrastructure Services
                services.AddLogging(configure => configure.AddDebug());

                services.AddSingleton<ITaggingService, TaggingService>();

                services.AddSingleton<IVectorService, VectorService>();

                services.AddSingleton<IVersionService, VersionService>();

                // Configuration
                services.Configure<SmartQB.Core.Configuration.PdfExtractionOptions>(context.Configuration.GetSection("PdfExtractionOptions"));

                // PDF Service
                services.AddSingleton<IIngestionService, IngestionService>();

                services.AddSingleton<IPdfService, PdfService>();

                // LLM Service
                services.AddSingleton<ILLMService>(sp =>
                {
                    var config = sp.GetRequiredService<IConfiguration>();
                    var apiKey = config["AI:ApiKey"] ?? throw new InvalidOperationException("AI:ApiKey is missing");
                    var modelId = config["AI:ModelId"] ?? "gpt-4o";
                    return new LLMService(apiKey, modelId);
                });

                // Database
                services.AddDbContext<SmartQBDbContext>(options =>
                    options.UseSqlite(context.Configuration.GetConnectionString("DefaultConnection")));

                // UI Services
                services.AddSingleton<ImportViewModel>();

                services.AddSingleton<MainWindow>();
                services.AddSingleton<MainViewModel>();
            })
            .Build();
    }

    protected override async void OnStartup(StartupEventArgs e)
    {
        await AppHost!.StartAsync();

        // Ensure Database is Created
        using (var scope = AppHost.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<SmartQBDbContext>();
            // EnsureCreated works well for prototyping. For production, Migrations are better.
            // But adhering to the task list, this is "Configure EF Core + SQLite".
            dbContext.Database.EnsureCreated();
        }

        var startupForm = AppHost.Services.GetRequiredService<MainWindow>();
        startupForm.Show();

        base.OnStartup(e);
    }

    protected override async void OnExit(ExitEventArgs e)
    {
        using (AppHost)
        {
            await AppHost!.StopAsync();
        }
        base.OnExit(e);
    }
}
