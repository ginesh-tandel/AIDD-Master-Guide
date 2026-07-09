namespace AIDocAssistant.Application.Common.Exceptions;

public class EmbeddingGenerationException : Exception
{
    public EmbeddingGenerationException(string message) : base(message) { }
    public EmbeddingGenerationException(string message, Exception inner) : base(message, inner) { }
}
