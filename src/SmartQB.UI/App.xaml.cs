using System;
using System.IO;
using System.Windows;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SmartQB.Core.Interfaces;
using SmartQB.Infrastructure.Data;
using SmartQB.Infrastructure.Services;
using SmartQB.UI.ViewModels;
using SmartQB.UI.Views;

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
                services.AddSingleton<IVersionService, VersionService>();
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

                // ViewModels
                services.AddSingleton<MainViewModel>();
                services.AddSingleton<IngestionViewModel>();
                services.AddSingleton<QuestionBankViewModel>();
                services.AddSingleton<PaperCompositionViewModel>();

                // Windows & Views
                services.AddSingleton<MainWindow>();
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
