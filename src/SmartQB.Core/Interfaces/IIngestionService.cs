using System.Threading.Tasks;

namespace SmartQB.Core.Interfaces;

using System;

public interface IIngestionService
{
    Task ProcessPdfAsync(string filePath, IProgress<string>? progress = null);
}
