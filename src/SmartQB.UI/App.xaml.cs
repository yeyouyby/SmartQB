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

namespace SmartQB.UI;

public partial class App : Application
{
    public static IHost? AppHost { get; private set; }

    public App()
    {
        AppHost = Host.CreateDefaultBuilder()
            .ConfigureAppConfiguration((context, config) =>
            {
                config.SetBasePath(Directory.GetCurrentDirectory());
                config.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
            })
            .ConfigureServices((context, services) =>
            {
                // Core & Infrastructure Services
                services.AddSingleton<IVersionService, VersionService>();

                // Database
                services.AddDbContext<SmartQBDbContext>(options =>
                    options.UseSqlite(context.Configuration.GetConnectionString("DefaultConnection")));

                // UI Services
                services.AddSingleton<MainWindow>();
                services.AddSingleton<MainViewModel>();
            })
            .Build();
    }

    protected override async void OnStartup(StartupEventArgs e)
    {
        await AppHost!.StartAsync();

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
