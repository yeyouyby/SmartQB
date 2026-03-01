using System;
using System.Threading.Tasks;

namespace SmartQB.Core.Interfaces;

public interface IIngestionService
{
    Task ProcessPdfAsync(string filePath);
    event EventHandler<int> QuestionIngested;
}
