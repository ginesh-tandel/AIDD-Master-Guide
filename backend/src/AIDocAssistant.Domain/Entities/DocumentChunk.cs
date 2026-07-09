using AIDocAssistant.Domain.Common;

namespace AIDocAssistant.Domain.Entities;

public class DocumentChunk : BaseEntity
{
    public Guid DocumentId { get; set; }
    public int ChunkIndex { get; set; }
    public string Content { get; set; } = default!;
    // Vector embedding is stored in the vector store, referenced by this chunk's Id.
}
