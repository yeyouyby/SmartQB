using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using SmartQB.Core.Entities;
using SmartQB.Core.Interfaces;
using SmartQB.Infrastructure.Data;

namespace SmartQB.UI.ViewModels;

public partial class IngestionViewModel : ObservableObject
{
    private readonly IPdfService _pdfService;
    private readonly ILLMService _llmService;
    private readonly IServiceScopeFactory _scopeFactory;

    [ObservableProperty]
    private bool _isProcessing;

    [ObservableProperty]
    private double _progress;

    [ObservableProperty]
    private string _statusMessage = "Ready to ingest PDF files.";

    public ObservableCollection<Question> ProcessedQuestions { get; } = new();

    public IngestionViewModel(IPdfService pdfService, ILLMService llmService, IServiceScopeFactory scopeFactory)
    {
        _pdfService = pdfService;
        _llmService = llmService;
        _scopeFactory = scopeFactory;
    }

    [RelayCommand]
    private async Task ProcessFilesAsync(string[] filePaths)
    {
        if (IsProcessing) return;

        IsProcessing = true;
        Progress = 0;
        StatusMessage = "Starting ingestion...";

        try
        {
            await Task.Run(async () =>
            {
                foreach (var filePath in filePaths)
                {
                    await ProcessSingleFileAsync(filePath);
                }
            });

            StatusMessage = "Ingestion complete.";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error: {ex.Message}";
        }
        finally
        {
            IsProcessing = false;
            Progress = 100;
        }
    }

    private async Task ProcessSingleFileAsync(string filePath)
    {
        var fileName = Path.GetFileName(filePath);
        UpdateStatus($"Processing {fileName}...");

        int pageCount = _pdfService.GetPageCount(filePath);

        for (int i = 0; i < pageCount; i++)
        {
            UpdateStatus($"Processing {fileName} - Page {i + 1}/{pageCount}...");

            // 1. Render Page
            var imageBytes = await _pdfService.RenderPageAsync(filePath, i);

            // 2. Analyze with LLM
            var prompt = @"
You are a smart assistant that extracts exam questions from images.
Identify the question stem, options (if any), answer, and parsing.
Also estimate difficulty (0.0 to 1.0) and generate a logic descriptor.
Extract tags relevant to the question.
Output strictly valid JSON in the following format:
[
  {
    ""Content"": ""The full content of the question in LaTeX/Markdown format"",
    ""LogicDescriptor"": ""Short description of the logic"",
    ""Difficulty"": 0.5,
    ""Tags"": [""Tag1"", ""Tag2""]
  }
]
If there are multiple questions, return a JSON array of objects.
Do not include any markdown formatting (like ```json) in the response, just the raw JSON string.
";
            var jsonResponse = await _llmService.AnalyzeImageAsync(imageBytes, prompt);

            // 3. Parse JSON
            var questions = ParseLlmResponse(jsonResponse);

            // 4. Save to DB
            if (questions.Count > 0)
            {
                using (var scope = _scopeFactory.CreateScope())
                {
                    var db = scope.ServiceProvider.GetRequiredService<SmartQBDbContext>();

                    foreach (var qDto in questions)
                    {
                        var tags = new List<Tag>();
                        if (qDto.Tags != null)
                        {
                            foreach (var tagName in qDto.Tags)
                            {
                                var tag = db.Tags.FirstOrDefault(t => t.Name == tagName);
                                if (tag == null)
                                {
                                    tag = new Tag { Name = tagName };
                                    db.Tags.Add(tag);
                                }
                                tags.Add(tag);
                            }
                        }

                        var question = new Question
                        {
                            Content = qDto.Content,
                            LogicDescriptor = qDto.LogicDescriptor,
                            Difficulty = qDto.Difficulty,
                            Tags = tags
                        };

                        db.Questions.Add(question);
                        await db.SaveChangesAsync();

                        // Sync to UI
                        App.Current.Dispatcher.Invoke(() => ProcessedQuestions.Add(question));
                    }
                }
            }

            // Update Progress
            UpdateProgress((double)(i + 1) / pageCount * 100);
        }
    }

    private List<QuestionDto> ParseLlmResponse(string json)
    {
        try
        {
            // Try to clean up markdown if present
            json = json.Replace("```json", "").Replace("```", "").Trim();

            if (json.StartsWith("["))
            {
                return JsonSerializer.Deserialize<List<QuestionDto>>(json) ?? new List<QuestionDto>();
            }
            else if (json.StartsWith("{"))
            {
                var single = JsonSerializer.Deserialize<QuestionDto>(json);
                return single != null ? new List<QuestionDto> { single } : new List<QuestionDto>();
            }
            return new List<QuestionDto>();
        }
        catch
        {
            // If parsing fails, maybe return raw content as a fallback or log error
            return new List<QuestionDto>();
        }
    }

    private void UpdateStatus(string message)
    {
        App.Current.Dispatcher.Invoke(() => StatusMessage = message);
    }

    private void UpdateProgress(double value)
    {
        App.Current.Dispatcher.Invoke(() => Progress = value);
    }

    private record QuestionDto(string Content, string LogicDescriptor, double Difficulty, List<string> Tags);
}
