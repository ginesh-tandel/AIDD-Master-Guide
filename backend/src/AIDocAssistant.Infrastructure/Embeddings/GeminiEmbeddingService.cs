using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using AIDocAssistant.Application.Common.Interfaces;
using AIDocAssistant.Application.Common.Exceptions;
using AIDocAssistant.Infrastructure.Configuration;
using Microsoft.Extensions.Options;

namespace AIDocAssistant.Infrastructure.Embeddings;

/// <summary>
/// Calls Gemini's embedContent REST endpoint directly via HttpClient — no official
/// Google Gen AI .NET SDK dependency needed for this single call shape.
/// Register with: services.AddHttpClient&lt;IEmbeddingService, GeminiEmbeddingService&gt;();
/// </summary>
public class GeminiEmbeddingService : IEmbeddingService
{
    private readonly HttpClient _http;
    private readonly GeminiOptions _options;

    public GeminiEmbeddingService(HttpClient http, IOptions<GeminiOptions> options)
    {
        _http = http;
        _options = options.Value;
    }

    public async Task<float[]> GenerateEmbeddingAsync(string text, CancellationToken ct = default)
    {
        var url = $"{_options.BaseUrl}/models/{_options.EmbeddingModel}:embedContent";

        var request = new EmbedRequest
        {
            Model = $"models/{_options.EmbeddingModel}",
            Content = new EmbedContent
            {
                Parts = new[] { new EmbedPart { Text = text } }
            }
        };

        using var httpRequest = new HttpRequestMessage(HttpMethod.Post, url)
        {
            Content = JsonContent.Create(request)
        };
        httpRequest.Headers.Add("x-goog-api-key", _options.ApiKey);

        using var response = await _http.SendAsync(httpRequest, ct);

        if (!response.IsSuccessStatusCode)
        {
            var errorBody = await response.Content.ReadAsStringAsync(ct);
            throw new EmbeddingGenerationException(
                $"Gemini embedContent failed ({(int)response.StatusCode}): {errorBody}");
        }

        var result = await response.Content.ReadFromJsonAsync<EmbedResponse>(cancellationToken: ct)
            ?? throw new EmbeddingGenerationException("Gemini embedContent returned an empty response.");

        return result.Embedding?.Values ?? throw new EmbeddingGenerationException(
            "Gemini embedContent response missing embedding values.");
    }

    // --- Request/response DTOs, scoped to this file since they're wire-format only ---

    private class EmbedRequest
    {
        [JsonPropertyName("model")]
        public string Model { get; set; } = string.Empty;

        [JsonPropertyName("content")]
        public EmbedContent Content { get; set; } = default!;
    }

    private class EmbedContent
    {
        [JsonPropertyName("parts")]
        public EmbedPart[] Parts { get; set; } = Array.Empty<EmbedPart>();
    }

    private class EmbedPart
    {
        [JsonPropertyName("text")]
        public string Text { get; set; } = string.Empty;
    }

    private class EmbedResponse
    {
        [JsonPropertyName("embedding")]
        public EmbeddingValues? Embedding { get; set; }
    }

    private class EmbeddingValues
    {
        [JsonPropertyName("values")]
        public float[]? Values { get; set; }
    }
}
