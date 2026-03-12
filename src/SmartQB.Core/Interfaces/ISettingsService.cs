using System.Threading.Tasks;

namespace SmartQB.Core.Interfaces;

public interface ISettingsService
{
    string ApiKey { get; set; }
    string BaseUrl { get; set; }
    string ModelId { get; set; }

    Task LoadAsync();
    Task SaveAsync();
}
