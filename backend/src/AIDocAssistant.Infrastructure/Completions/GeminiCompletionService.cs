using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using AIDocAssistant.Application.Common.Interfaces;
using AIDocAssistant.Application.Common.Exceptions;
using AIDocAssistant.Infrastructure.Configuration;
using Microsoft.Extensions.Options;

namespace AIDocAssistant.Infrastructure.Completions;

/// <summary>
/// Streams chat completions from Gemini's streamGenerateContent REST endpoint
/// (Server-Sent Events). Context chunks are injected into the prompt as retrieved
/// passages — the caller is responsible for attaching citations from those chunks
/// to the final answer (see AGENTS.md — no answer without a source citation).
/// Register with: services.AddHttpClient&lt;ICompletionService, GeminiCompletionService&gt;();
/// </summary>
public class GeminiCompletionService : ICompletionService
{
    private readonly HttpClient _http;
    private readonly GeminiOptions _options;

    public GeminiCompletionService(HttpClient http, IOptions<GeminiOptions> options)
    {
        _http = http;
        _options = options.Value;
    }

    public async IAsyncEnumerable<string> StreamCompletionAsync(
        string prompt,
        IReadOnlyList<string> contextChunks,
        [EnumeratorCancellation] CancellationToken ct = default)
    {
        var url = $"{_options.BaseUrl}/models/{_options.ChatModel}:streamGenerateContent?alt=sse";

        var fullPrompt = BuildPromptWithContext(prompt, contextChunks);

        var requestBody = new
        {
            contents = new[]
            {
                new
                {
                    role = "user",
                    parts = new[] { new { text = fullPrompt } }
                }
            }
        };

        using var httpRequest = new HttpRequestMessage(HttpMethod.Post, url)
        {
            Content = JsonContent(requestBody)
        };
        httpRequest.Headers.Add("x-goog-api-key", _options.ApiKey);

        using var response = await _http.SendAsync(
            httpRequest, HttpCompletionOption.ResponseHeadersRead, ct);

        if (!response.IsSuccessStatusCode)
        {
            var errorBody = await response.Content.ReadAsStringAsync(ct);
            throw new CompletionStreamException(
                $"Gemini streamGenerateContent failed ({(int)response.StatusCode}): {errorBody}");
        }

        await using var stream = await response.Content.ReadAsStreamAsync(ct);
        using var reader = new StreamReader(stream);

        while (!reader.EndOfStream)
        {
            ct.ThrowIfCancellationRequested();
            var line = await reader.ReadLineAsync(ct);

            if (string.IsNullOrWhiteSpace(line) || !line.StartsWith("data: "))
                continue;

            var json = line["data: ".Length..];
            if (json == "[DONE]")
                yield break;

            string? token = null;
            try
            {
                using var doc = JsonDocument.Parse(json);
                token = doc.RootElement
                    .GetProperty("candidates")[0]
                    .GetProperty("content")
                    .GetProperty("parts")[0]
                    .GetProperty("text")
                    .GetString();
            }
            catch (Exception ex) when (ex is KeyNotFoundException or JsonException or IndexOutOfRangeException)
            {
                // Malformed or unexpected SSE chunk shape — skip rather than break the stream.
                continue;
            }

            if (!string.IsNullOrEmpty(token))
                yield return token;
        }
    }

    private static string BuildPromptWithContext(string question, IReadOnlyList<string> contextChunks)
    {
        if (contextChunks.Count == 0)
            return question;

        var sb = new StringBuilder();
        sb.AppendLine("Answer the question using only the context passages below. " +
                      "If the answer isn't in the context, say so — do not guess.");
        sb.AppendLine();
        for (var i = 0; i < contextChunks.Count; i++)
        {
            sb.AppendLine($"[Passage {i + 1}]");
            sb.AppendLine(contextChunks[i]);
            sb.AppendLine();
        }
        sb.AppendLine($"Question: {question}");
        return sb.ToString();
    }

    private static System.Net.Http.Json.JsonContent JsonContent(object body) =>
        System.Net.Http.Json.JsonContent.Create(body);
}
