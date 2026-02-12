using System;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SmartQB.Core.Entities;
using SmartQB.Core.Interfaces;
using SmartQB.Infrastructure.Data;

namespace SmartQB.Infrastructure.Services;

public class IngestionService : IIngestionService
{
    private readonly IPdfService _pdfService;
    private readonly ILLMService _llmService;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<IngestionService> _logger;

    public IngestionService(IPdfService pdfService, ILLMService llmService, IServiceScopeFactory scopeFactory, ILogger<IngestionService> logger)
    {
        _pdfService = pdfService;
        _llmService = llmService;
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    public async Task ProcessPdfAsync(string filePath)
    {
        _logger.LogInformation("Starting ingestion for file: {FilePath}", filePath);
        int pageCount = _pdfService.GetPageCount(filePath);

        for (int i = 0; i < pageCount; i++)
        {
            try
            {
                // Render page to image
                byte[] imageBytes = await _pdfService.RenderPageAsync(filePath, i);

                // Prompt for LLM
                string prompt = @"
Analyze the provided image of a math question.
Extract the following information and return it as a JSON object:
- ""Content"": The question text in LaTeX/Markdown format.
- ""LogicDescriptor"": A brief explanation of the logic or steps required to solve it.
- ""Difficulty"": A number between 0.0 and 5.0 representing the difficulty level.

Format:
{
  ""Content"": ""..."",
  ""LogicDescriptor"": ""..."",
  ""Difficulty"": 3.5
}
Ensure the output is valid JSON and contains no markdown code blocks.";

                // Call LLM
                string response = await _llmService.AnalyzeImageAsync(imageBytes, prompt);

                // Clean up response (remove markdown code blocks if present)
                string json = CleanJson(response);

                // Deserialize
                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var data = JsonSerializer.Deserialize<QuestionDto>(json, options);

                if (data != null && !string.IsNullOrWhiteSpace(data.Content))
                {
                    // Use a scope to get DbContext
                    using (var scope = _scopeFactory.CreateScope())
                    {
                        var dbContext = scope.ServiceProvider.GetRequiredService<SmartQBDbContext>();

                        // Create Question Entity
                        var question = new Question
                        {
                            Content = data.Content,
                            LogicDescriptor = data.LogicDescriptor,
                            Difficulty = data.Difficulty
                        };

                        // Generate Embedding
                        string textToEmbed = !string.IsNullOrWhiteSpace(data.LogicDescriptor) ? data.LogicDescriptor : data.Content;
                        var embedding = await _llmService.GetEmbeddingAsync(textToEmbed);
                        if (embedding.Length > 0)
                        {
                            question.EmbeddingJson = JsonSerializer.Serialize(embedding);
                        }

                        dbContext.Questions.Add(question);
                        await dbContext.SaveChangesAsync();
                        _logger.LogInformation("Successfully ingested page {PageNumber} of {FilePath}", i + 1, filePath);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing page {PageNumber} of {FilePath}", i + 1, filePath);
                // Continue to next page
            }
        }
        _logger.LogInformation("Finished ingestion for file: {FilePath}", filePath);
    }

    private string CleanJson(string response)
    {
        if (string.IsNullOrWhiteSpace(response)) return "{}";

        var trimmed = response.Trim();
        if (trimmed.StartsWith("```json"))
        {
            trimmed = trimmed.Substring(7);
        }
        else if (trimmed.StartsWith("```"))
        {
            trimmed = trimmed.Substring(3);
        }

        if (trimmed.EndsWith("```"))
        {
            trimmed = trimmed.Substring(0, trimmed.Length - 3);
        }

        return trimmed.Trim();
    }

    // Helper class for deserialization
    private class QuestionDto
    {
        public string? Content { get; set; }
        public string? LogicDescriptor { get; set; }
        public double Difficulty { get; set; }
    }
}
