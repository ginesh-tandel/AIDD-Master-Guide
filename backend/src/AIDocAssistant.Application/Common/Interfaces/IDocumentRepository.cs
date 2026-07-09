using AIDocAssistant.Domain.Entities;

namespace AIDocAssistant.Application.Common.Interfaces;

/// <summary>
/// Every method here MUST be scoped by workspaceId — never return
/// documents across workspace boundaries.
/// </summary>
public interface IDocumentRepository
{
    Task<Document?> GetByIdAsync(Guid workspaceId, Guid documentId, CancellationToken ct = default);
    Task<IReadOnlyList<Document>> ListAsync(Guid workspaceId, CancellationToken ct = default);
    Task AddAsync(Document document, CancellationToken ct = default);
}
