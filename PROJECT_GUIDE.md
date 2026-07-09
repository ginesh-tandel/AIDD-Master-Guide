# AI Document Assistant — Project Guide

A multi-tenant SaaS platform for secure document upload, AI-powered knowledge base
creation, and natural-language Q&A over documents using Retrieval-Augmented
Generation (RAG).

This guide is the single reference for how the project is built, why key decisions
were made, and how to get it running. Start here.

---

## 1. Tech Stack

| Layer | Choice |
|---|---|
| Backend | .NET 8, ASP.NET Core Web API, Clean Architecture |
| Database | SQL Server (document/user metadata) |
| Frontend | Next.js (React, TypeScript) |
| Embeddings + Chat | Gemini API (`gemini-embedding-001` + `gemini-flash-latest`) |
| Repo structure | Monorepo — `backend/` and `frontend/` in one repo |

### Key decisions and why

- **Next.js over Angular**: the core UX is a streaming chat interface with
  citations — React's ecosystem has far more ready-made tooling for streaming/AI
  chat UIs. Angular would work but requires more custom plumbing for this
  specific use case.
- **Monorepo over separate repos**: one `AGENTS.md`/`DESIGN.md` governs both
  sides, easier to keep frontend and backend in sync during early development.
- **No CQRS/MediatR**: the operations here (upload, list, ask) don't have
  complex read/write divergence yet. Plain Application-layer services keep the
  code traceable in one pass. Revisit if the RAG pipeline gets genuinely complex
  (multi-step retrieval, caching layers) or a team fluent in MediatR joins.
- **No shared backend/frontend types package (yet)**: `Domain` already serves as
  the backend's shared layer. Frontend/backend contract sync is manual for now
  (DTOs hand-matched) — worth automating with OpenAPI-generated TypeScript types
  once the API shape stabilizes, not before.
- **Gemini over OpenAI/Azure/Anthropic+Voyage**: genuinely free tier for both
  embeddings and chat (1,500 requests/day, no credit card) — best fit for
  prototyping. Swap later via `IEmbeddingService`/`ICompletionService` — a
  contained change, not a rewrite, once real users need a paid/higher-SLA tier.
- **`dotnet user-secrets` over `.env` for backend secrets**: idiomatic for
  .NET, and secrets live physically outside the repo folder — safer than any
  gitignored file, which can still be accidentally committed.

---

## 2. Architecture

### Backend — Clean Architecture

```
Domain            ← zero dependencies (entities, enums, base types)
   ↑
Application       ← depends on Domain only (interfaces for infra, no EF/SQL/SDK calls)
   ↑
Infrastructure    ← implements Application's interfaces (EF Core, Gemini calls)
   ↑
Api               ← composition root; controllers call Application services only
```

Dependencies point inward, toward `Domain`. `Domain` doesn't know `Application`
exists; `Application` doesn't know `Infrastructure`/`Api` exist.

**Ports and adapters in practice**: `IEmbeddingService`, `ICompletionService`,
`IDocumentRepository` are interfaces defined in `Application`. `Infrastructure`
provides the real implementation (`GeminiEmbeddingService`, etc.). `Api`'s
`Program.cs` is the only place that wires a concrete implementation to an
interface — everywhere else only sees the abstraction.

### Frontend — feature-based

```
src/
  app/            ← Next.js App Router pages
  api/            ← single typed API client — all backend calls go through here
  components/ui/  ← shared UI primitives
  features/
    documents/    ← components + api calls for document upload/list
    chat/         ← components + api calls for the chat/citation UI
    knowledgeBase/
```

No ad-hoc `fetch()` calls scattered through components — everything routes
through `src/api/client.ts`.

### Key domain rules (enforced throughout, not just documented)

- Every request/query is scoped by `WorkspaceId` — no cross-tenant data access,
  ever.
- Every RAG answer carries a source citation back to the originating document/
  chunk — no citation-less answers, by rule (see `AGENTS.md`).
- Ownership chain: `Workspace → Document → Chunk → Embedding`.

---

## 3. Repository Structure

```
AIDocumentAssistant/
├── AGENTS.md              ← AI agent rules (binding for OpenCode/Codex/Claude Code/Cursor)
├── DESIGN.md               ← design tokens (colors, type, spacing) — UI must follow this
├── CLAUDE.md                ← pointer → AGENTS.md
├── opencode.json             ← permission enforcement (build/test free, commit/push ask)
├── PROJECT_GUIDE.md           ← this file
├── README.md                   ← quick setup commands
├── .cursor/rules/main.mdc       ← pointer → AGENTS.md
│
├── backend/
│   ├── AIDocAssistant.sln
│   ├── .gitignore                          (bin/, obj/, appsettings.*.Local.json)
│   ├── src/
│   │   ├── AIDocAssistant.Domain/
│   │   │   ├── Common/BaseEntity.cs
│   │   │   └── Entities/{Document,DocumentChunk}.cs
│   │   ├── AIDocAssistant.Application/
│   │   │   ├── Common/Interfaces/{IEmbeddingService,ICompletionService,IDocumentRepository}.cs
│   │   │   ├── Common/Exceptions/{EmbeddingGenerationException,CompletionStreamException}.cs
│   │   │   └── Documents/UploadDocumentCommand.cs
│   │   ├── AIDocAssistant.Infrastructure/
│   │   │   ├── Configuration/GeminiOptions.cs
│   │   │   ├── Embeddings/GeminiEmbeddingService.cs      ← implements IEmbeddingService
│   │   │   ├── Completions/GeminiCompletionService.cs     ← implements ICompletionService
│   │   │   └── Persistence/AppDbContext.cs                 (commented out — no EF Core yet)
│   │   └── AIDocAssistant.Api/
│   │       ├── Program.cs           ← DI wiring, CORS, Swagger
│   │       ├── appsettings.json      ← structure/defaults only, no secrets
│   │       ├── appsettings.Development.json
│   │       └── Controllers/DocumentsController.cs  (stub — not wired to services yet)
│   └── tests/AIDocAssistant.Application.Tests/
│
└── frontend/
    ├── package.json
    ├── tsconfig.json
    ├── .env.local.example
    └── src/
        ├── app/{layout,page}.tsx
        ├── api/client.ts
        ├── components/ui/Button.tsx
        └── features/
            ├── documents/{api/documentsApi.ts, components/DocumentList.tsx}
            ├── chat/components/ChatWindow.tsx
            └── knowledgeBase/
```

---

## 4. AI-Driven Development Setup

Every AI coding tool used on this repo (OpenCode, Codex, Claude Code, Cursor)
reads the same rules from `AGENTS.md` — no per-tool drift.

- **`AGENTS.md`**: architecture rules, validation rules (build/test must pass
  before commit), commit/push rules, conflict-handling rules, coding
  conventions for both backend and frontend.
- **`CLAUDE.md`** / **`.cursor/rules/main.mdc`**: one-line pointers to
  `AGENTS.md` — OpenCode/Codex read `AGENTS.md` natively.
- **`opencode.json`**: enforces the rules that matter most at the permission
  level — `dotnet build/test` and `npm run build/lint/test` run freely;
  `git commit`/`git push` require approval; force-push is hard-blocked.
- **`DESIGN.md`**: referenced from `AGENTS.md`'s frontend rules — any UI
  component must use these tokens, not invent new colors/spacing ad hoc.

The workflow loop every AI session follows: **Plan → Implement → Validate →
Commit/Push → Conflict-check** (stop and ask on ambiguity, never guess).

---

## 5. Design System

See `DESIGN.md` for the full token set. Summary:

- Warm off-white / near-black palette (not the generic AI-tool cream+terracotta
  or black+neon look) — prioritizes trust and clarity for an enterprise
  document tool.
- Citations render as a highlighted inline chip, not a plain footnote —
  the product's core differentiator, treated as a first-class UI pattern.
- One border-radius value, one 4px-based spacing scale, defined component
  states (hover/active/disabled/focus) — reused everywhere, not redecided
  per screen.
- **Colors/fonts in `DESIGN.md` are still placeholders** — lock these in
  before building real components (a rendered mockup exists to react to;
  ask for it again if needed).

---

## 6. Getting Started

### Backend

```bash
cd backend
dotnet restore
dotnet build

# Get a free Gemini API key: https://aistudio.google.com/app/apikey (no card needed)
cd src/AIDocAssistant.Api
dotnet user-secrets init
dotnet user-secrets set "Gemini:ApiKey" "<your key>"
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "<your local connection string>"

dotnet run
# Swagger UI at https://localhost:<port>/swagger
```

### Frontend

```bash
cd frontend
npm install
cp .env.local.example .env.local
npm run dev
```

---

## 7. Provider Configuration

Currently using **Gemini** (`gemini-embedding-001` for embeddings,
`gemini-flash-latest` for chat) — chosen for its genuinely free tier during
prototyping (1,500 requests/day, no credit card required).

**Before going to production / real users**: free-tier traffic may be used by
Google to improve their models — switch to a paid Gemini key (still cheap) or
another provider before sending real customer documents through this.

**To swap providers**: implement `IEmbeddingService`/`ICompletionService`
(both in `Application/Common/Interfaces/`) against the new provider, then
change only the two `AddHttpClient<...>()` registrations in `Program.cs`.
Nothing in `Application` or `Api` needs to change — that's the point of the
interface boundary.

---

## 8. Current Status

**Done:**
- Clean Architecture backend skeleton, all 5 projects correctly linked
- `IEmbeddingService`/`ICompletionService` implemented against Gemini (real
  HTTP calls, not stubs)
- `appsettings.json` + User Secrets pattern for config/secrets
- Next.js frontend skeleton with feature-folder structure
- Full AIDD config (`AGENTS.md`, `DESIGN.md`, pointer files, `opencode.json`)

**Not done yet — in suggested order:**
1. Lock `DESIGN.md` placeholder colors/fonts
2. Add EF Core (`Microsoft.EntityFrameworkCore.SqlServer`), uncomment
   `AppDbContext`, implement `IDocumentRepository`
3. Wire `DocumentsController` → an Application-layer document service →
   `IDocumentRepository`/`IEmbeddingService` (the actual upload flow, end to end)
4. Build `ChatWindow.tsx` and `DocumentList.tsx` for real (currently empty stubs)
5. Auth — required before real `WorkspaceId` scoping can work correctly
6. Vector storage decision (Azure AI Search / pgvector / Pinecone — not yet chosen)

---

## 9. Contributing / Development Workflow

Any change — human or AI-assisted — follows the loop defined in `AGENTS.md`:

1. **Plan** — state what will change and where, before editing, for anything
   non-trivial (auth, schema, multi-file changes)
2. **Implement** — minimal diff, no unrelated refactors
3. **Validate** — `dotnet build && dotnet test` (backend) and/or
   `npm run build && npm run lint` (frontend), depending on what changed
4. **Commit & push** — only after validation passes; conventional commit
   format `<type>(<scope>): <description>`, e.g. `feat(backend): add document upload endpoint`
5. **Conflict-check** — stop and ask on any ambiguity or contradiction with
   `AGENTS.md`, don't guess

This applies whether you're coding directly or using OpenCode/Claude Code/
Codex/Cursor — the AIDD config in this repo enforces the same loop for all of
them.
