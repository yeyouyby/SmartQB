using Microsoft.Extensions.Configuration;
using OpenAI;
using OpenAI.Chat;
using OpenAI.Embeddings;
using SmartQB.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.ClientModel;
using System.Threading.Tasks;

namespace SmartQB.Infrastructure.Services;

public class LLMService : ILLMService
{
    private readonly ISettingsService _settings;
    private readonly IConfiguration _config;
    private ChatClient? _chatClient;
    private string _currentApiKey;
    private string _currentBaseUrl;
    private string _currentModelId;
    private EmbeddingClient? _embeddingClient;

    public LLMService(ISettingsService settings, IConfiguration config)
    {
        _settings = settings;
        _config = config;
        RefreshClients();
    }

    private void RefreshClients()
    {
        var apiKey = string.IsNullOrEmpty(_settings.ApiKey) ? _config["AI:ApiKey"] : _settings.ApiKey;
        var modelId = string.IsNullOrEmpty(_settings.ModelId) ? (_config["AI:ModelId"] ?? "gpt-4o") : _settings.ModelId;
        var embeddingModelId = "text-embedding-3-small";
        var baseUrl = _settings.BaseUrl;

        // Only recreate if settings have actually changed
        if (apiKey == _currentApiKey && modelId == _currentModelId && baseUrl == _currentBaseUrl && _chatClient != null)
        {
            return;
        }

        _currentApiKey = apiKey;
        _currentModelId = modelId;
        _currentBaseUrl = baseUrl;

        if (string.IsNullOrEmpty(apiKey))
        {
             // We allow clients to be null if API key is not configured, to let the UI load and user enter it in Settings.
             // Calls to the API will fail until configured.
             _chatClient = null;
             _embeddingClient = null;
             return;
        }

        var credential = new ApiKeyCredential(apiKey);

        OpenAIClientOptions? options = null;
        if (!string.IsNullOrEmpty(baseUrl))
        {
             options = new OpenAIClientOptions { Endpoint = new Uri(baseUrl) };
        }

        var openAiClient = options != null
            ? new OpenAIClient(credential, options)
            : new OpenAIClient(credential);

        _chatClient = openAiClient.GetChatClient(modelId);
        _embeddingClient = openAiClient.GetEmbeddingClient(embeddingModelId);
    }

    private ChatClient GetChatClient()
    {
        RefreshClients(); // Refresh dynamically before use to pick up changes
        if (_chatClient == null)
            throw new InvalidOperationException("API Key is not configured. Please configure it in Settings.");
        return _chatClient;
    }

    private EmbeddingClient GetEmbeddingClient()
    {
        RefreshClients(); // Refresh dynamically before use to pick up changes
        if (_embeddingClient == null)
            throw new InvalidOperationException("API Key is not configured. Please configure it in Settings.");
        return _embeddingClient;
    }

    public async Task<string> ChatAsync(string prompt, string? systemPrompt = null)
    {
        var messages = new List<ChatMessage>();
        if (!string.IsNullOrEmpty(systemPrompt))
        {
            messages.Add(new SystemChatMessage(systemPrompt));
        }
        messages.Add(new UserChatMessage(prompt));

        ClientResult<ChatCompletion> result = await GetChatClient().CompleteChatAsync(messages);

        if (result?.Value?.Content == null || result.Value.Content.Count == 0)
        {
             throw new InvalidOperationException("Received empty or invalid response from LLM.");
        }

        return result.Value.Content[0].Text ?? string.Empty;
    }

    public async Task<string> AnalyzeImageAsync(byte[] imageBytes, string prompt)
    {
        var messages = new List<ChatMessage>
        {
            new UserChatMessage(
                ChatMessageContentPart.CreateTextPart(prompt),
                ChatMessageContentPart.CreateImagePart(new BinaryData(imageBytes), "image/png")
            )
        };

        ClientResult<ChatCompletion> result = await GetChatClient().CompleteChatAsync(messages);

        if (result?.Value?.Content == null || result.Value.Content.Count == 0)
        {
             throw new InvalidOperationException("Received empty or invalid response from LLM.");
        }

        return result.Value.Content[0].Text ?? string.Empty;
    }

    public async Task<float[]> GetEmbeddingAsync(string text)
    {
        if (string.IsNullOrWhiteSpace(text)) return Array.Empty<float>();

        ClientResult<OpenAIEmbedding> result = await GetEmbeddingClient().GenerateEmbeddingAsync(text);
        return result.Value.ToFloats().ToArray();
    }
}
