namespace AIDocAssistant.Application.Common.Exceptions;

public class CompletionStreamException : Exception
{
    public CompletionStreamException(string message) : base(message) { }
    public CompletionStreamException(string message, Exception inner) : base(message, inner) { }
}
