namespace AIDocAssistant.Infrastructure.Configuration;

/// <summary>
/// Bound from appsettings.json "Gemini" section. ApiKey comes from User Secrets
/// (dev) or environment variables (prod) — never from appsettings.json directly.
/// See AGENTS.md secrets rules.
/// </summary>
public class GeminiOptions
{
    public const string SectionName = "Gemini";

    public string ApiKey { get; set; } = string.Empty;
    public string EmbeddingModel { get; set; } = "gemini-embedding-001";
    public string ChatModel { get; set; } = "gemini-flash-latest";
    public string BaseUrl { get; set; } = "https://generativelanguage.googleapis.com/v1beta";
}
