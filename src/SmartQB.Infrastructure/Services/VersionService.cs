using Microsoft.Extensions.Configuration;
using SmartQB.Core.Interfaces;

namespace SmartQB.Infrastructure.Services;

public class VersionService : IVersionService
{
    private readonly IConfiguration _configuration;

    public VersionService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public string GetVersion()
    {
        return _configuration["Version"] ?? "Unknown Version";
    }
}
