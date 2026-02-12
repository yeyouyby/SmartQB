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
    private readonly ChatClient _chatClient;
    private readonly EmbeddingClient _embeddingClient;

    public LLMService(string apiKey, string modelId = "gpt-4o", string embeddingModelId = "text-embedding-3-small")
    {
         var credential = new ApiKeyCredential(apiKey);
         _chatClient = new ChatClient(modelId, credential);
         _embeddingClient = new EmbeddingClient(embeddingModelId, credential);
    }

    public async Task<string> ChatAsync(string prompt, string? systemPrompt = null)
    {
        var messages = new List<ChatMessage>();
        if (!string.IsNullOrEmpty(systemPrompt))
        {
            messages.Add(new SystemChatMessage(systemPrompt));
        }
        messages.Add(new UserChatMessage(prompt));

        ClientResult<ChatCompletion> result = await _chatClient.CompleteChatAsync(messages);

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

        ClientResult<ChatCompletion> result = await _chatClient.CompleteChatAsync(messages);

        if (result?.Value?.Content == null || result.Value.Content.Count == 0)
        {
             throw new InvalidOperationException("Received empty or invalid response from LLM.");
        }

        return result.Value.Content[0].Text ?? string.Empty;
    }

    public async Task<float[]> GetEmbeddingAsync(string text)
    {
        if (string.IsNullOrWhiteSpace(text)) return Array.Empty<float>();

        ClientResult<OpenAIEmbedding> result = await _embeddingClient.GenerateEmbeddingAsync(text);
        return result.Value.ToFloats().ToArray();
    }
}
