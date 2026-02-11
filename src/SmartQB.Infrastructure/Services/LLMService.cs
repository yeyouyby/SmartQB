using OpenAI.Chat;
using SmartQB.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.ClientModel;
using System.Threading.Tasks;

namespace SmartQB.Infrastructure.Services;

public class LLMService : ILLMService
{
    private readonly ChatClient _chatClient;

    public LLMService(string apiKey, string modelId = "gpt-4o")
    {
         _chatClient = new ChatClient(modelId, new ApiKeyCredential(apiKey));
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
        return result.Value.Content[0].Text;
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
        return result.Value.Content[0].Text;
    }
}
