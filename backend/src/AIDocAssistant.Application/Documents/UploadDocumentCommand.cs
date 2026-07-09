namespace AIDocAssistant.Application.Documents;

public record UploadDocumentCommand(Guid WorkspaceId, string FileName, Stream Content);
