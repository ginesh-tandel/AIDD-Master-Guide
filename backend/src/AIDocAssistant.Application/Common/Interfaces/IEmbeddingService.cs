namespace AIDocAssistant.Application.Common.Interfaces;

/// <summary>
/// Abstraction over the embedding provider (OpenAI, Azure, local model, etc.).
/// Infrastructure implements this — never call a provider SDK directly outside Infrastructure.
/// </summary>
public interface IEmbeddingService
{
    Task<float[]> GenerateEmbeddingAsync(string text, CancellationToken ct = default);
}
