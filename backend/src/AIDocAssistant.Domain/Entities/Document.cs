using AIDocAssistant.Domain.Common;

namespace AIDocAssistant.Domain.Entities;

public class Document : BaseEntity
{
    public string FileName { get; set; } = default!;
    public string StorageUri { get; set; } = default!;
    public DocumentStatus Status { get; set; } = DocumentStatus.Uploaded;
}

public enum DocumentStatus
{
    Uploaded,
    Chunking,
    Embedding,
    Ready,
    Failed
}
