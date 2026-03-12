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
        // Global exception handlers
        AppDomain.CurrentDomain.UnhandledException += (s, e) => LogCrash(e.ExceptionObject as Exception, "AppDomain");
        DispatcherUnhandledException += (s, e) => { LogCrash(e.Exception, "Dispatcher"); e.Handled = true; };
        System.Threading.Tasks.TaskScheduler.UnobservedTaskException += (s, e) => { LogCrash(e.Exception, "TaskScheduler"); e.SetObserved(); };

        try
        {
            AppHost = Host.CreateDefaultBuilder()
                        .ConfigureAppConfiguration((context, config) =>
                        {
                            config.SetBasePath(AppContext.BaseDirectory);
                            config.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);
                        })
                        .ConfigureServices((context, services) =>
                        {
                            // Core & Infrastructure Services
                            services.AddLogging(configure => configure.AddDebug());

                            services.AddSingleton<ISettingsService, SettingsService>();

                            services.AddSingleton<ITaggingService, TaggingService>();
                            services.AddSingleton<IQuestionService, QuestionService>();

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
                                var settings = sp.GetRequiredService<ISettingsService>();
                                var config = sp.GetRequiredService<IConfiguration>();
                                // Ensure settings are loaded on startup

                                return new LLMService(settings, config);
                            });

                            // Database
                            services.AddDbContext<SmartQBDbContext>(options =>
                                options.UseSqlite(context.Configuration.GetConnectionString("DefaultConnection")));

                            // UI Services
                            services.AddSingleton<ImportViewModel>();
                            services.AddSingleton<LibraryViewModel>();
                            services.AddSingleton<ExportViewModel>();

                            services.AddSingleton<MainWindow>();
                            services.AddSingleton<MainViewModel>();
                        }).Build();
        }
        catch (Exception ex)
        {
            LogCrash(ex, "Constructor");
        }
    }

    private void LogCrash(Exception? ex, string source)
    {
        var msg = $"[{DateTime.Now}] CRASH in {source}: {ex?.Message}\n{ex?.StackTrace}\n";
        if (ex?.InnerException != null)
        {
            msg += $"Inner: {ex?.InnerException?.Message}\n{ex?.InnerException?.StackTrace}\n";
        }
        try { File.AppendAllText("crash.log", msg); } catch { }
        MessageBox.Show($"Application crashed ({source}). See crash.log for details.\n\n{ex?.Message}", "Fatal Error", MessageBoxButton.OK, MessageBoxImage.Error);
        Environment.Exit(1);
    }



    protected override async void OnStartup(StartupEventArgs e)
    {
        try
        {
            await AppHost!.StartAsync();

                    // Load settings asynchronously before doing any other UI initialization
                    var settings = AppHost.Services.GetRequiredService<ISettingsService>();
                    await settings.LoadAsync();

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
        catch (Exception ex)
        {
            LogCrash(ex, "OnStartup");
        }
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
