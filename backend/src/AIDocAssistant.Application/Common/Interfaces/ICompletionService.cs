namespace AIDocAssistant.Application.Common.Interfaces;

public interface ICompletionService
{
    IAsyncEnumerable<string> StreamCompletionAsync(
        string prompt,
        IReadOnlyList<string> contextChunks,
        CancellationToken ct = default);
}
