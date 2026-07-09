import { apiGet } from "@/api/client";

export interface DocumentSummary {
  id: string;
  fileName: string;
  status: "Uploaded" | "Chunking" | "Embedding" | "Ready" | "Failed";
}

export function listDocuments() {
  return apiGet<DocumentSummary[]>("/api/documents");
}
