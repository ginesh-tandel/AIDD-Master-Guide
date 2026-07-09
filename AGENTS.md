# AGENTS.md — AI Document Assistant

Canonical AI agent rules for this repo. Every tool (OpenCode, Codex, Claude Code, Cursor)
must treat this file as binding. Tool-specific files (`CLAUDE.md`, `.cursor/rules/main.mdc`,
`codex.md`) only point here — do not duplicate rules elsewhere.

---

## Project Context

- **Product**: AI Document Assistant — multi-tenant SaaS platform for secure document
  upload, AI-powered knowledge base creation, and natural-language Q&A over documents
  using Retrieval-Augmented Generation (RAG)
- **Repo structure**: monorepo — `backend/` (.NET API) and `frontend/` (Next.js SPA)
- **Backend stack**: .NET 8, ASP.NET Core Web API, Clean Architecture (Domain /
  Application / Infrastructure / API layers), SQL Server (metadata), vector store
  (TBD — e.g. Azure AI Search / pgvector / Pinecone)
- **Frontend stack**: Next.js (React, TypeScript), calls backend API only — no direct
  DB/vector store access from frontend, ever
- **Key domain rule(s)**:
  - Every request/query MUST be scoped by `WorkspaceId` (multi-tenant isolation —
    one workspace must never see another's documents or answers)
  - Every RAG answer MUST carry a traceable citation back to the source document/chunk —
    no answer without a source reference
  - Uploaded documents and derived embeddings belong to the workspace that uploaded
    them; ownership chain is Workspace → Document → Chunk → Embedding

## Architecture Rules — Backend

- Respect Clean Architecture boundaries:
  - `Domain` has no dependencies on other layers.
  - `Application` depends only on `Domain` (interfaces for infra, embeddings, storage —
    no EF/SQL/LLM SDK calls here).
  - `Infrastructure` implements `Application` interfaces (EF Core, blob storage,
    embedding provider, vector store client) — no business logic here.
  - `Api` layer is thin — controllers/endpoints call Application services only.
- Embedding/LLM provider access always goes through an `Application`-layer interface
  (e.g. `IEmbeddingService`, `ICompletionService`) — never call a provider SDK directly
  from Infrastructure without going through that abstraction.
- Chat/RAG answer endpoints should support streaming responses (Server-Sent Events or
  chunked transfer) — the frontend chat UI depends on token-by-token streaming.

## Architecture Rules — Frontend

- **All UI components MUST follow DESIGN.md** — colors, typography, spacing, and radii
  come from that file's tokens only. Do not introduce new values ad hoc; if a new token
  is genuinely needed, add it to DESIGN.md first, then use it.

- Feature-based folder structure under `src/features/` (e.g. `documents/`, `chat/`,
  `knowledgeBase/`) — not one flat `components/` dump for everything.
- All backend calls go through a typed API client in `src/api/` — no ad-hoc `fetch`
  calls scattered through components.
- Use Next.js API routes only as a thin proxy/BFF layer if needed (e.g. to attach
  auth headers) — do not put business logic in API routes.
- Streaming chat responses use React state + streaming fetch/EventSource — no
  polling for chat responses.

## Planning & Change Control

- For anything touching multi-tenant scoping, document/embedding storage, RAG pipeline
  logic, auth, or spanning more than 2-3 files: state a brief plan first (files touched,
  approach, backend and/or frontend). Wait for confirmation.
- Minimal diffs. Do not refactor unrelated code while implementing a feature.
- One logical change per commit — do not bundle backend and frontend changes into one
  commit unless they're genuinely one logical change (e.g. a new endpoint + its UI).

## Validation Rules (mandatory before commit)

**Backend** (when backend files changed):
- `dotnet build` — must succeed, zero errors, zero new warnings on touched projects
- `dotnet test` — all tests must pass

**Frontend** (when frontend files changed):
- `npm run build` — must succeed
- `npm run lint` — must pass
- `npm run test` if tests exist for touched code

**Always**:
- Confirm no query or document/embedding access bypasses `WorkspaceId` scoping
- Confirm any new RAG answer path still returns source citations
- If any check fails: STOP, report the failure clearly, do not commit or push

## Commit & Push Rules

- Commit only after validation passes for whichever side(s) changed
- Commit message format: `<type>(<scope>): <short description>` — scope is
  `backend` or `frontend` when the change is isolated to one side
  (e.g. `feat(frontend): add chat streaming UI`)
- Push only after a successful local commit
- Never force-push, never rewrite shared branch history, without explicit confirmation

## Conflict-Handling Rules

- Merge conflicts, ambiguous requirements, or instructions that contradict this file:
  STOP and ask — do not guess, do not silently choose a side
- If a request conflicts with multi-tenancy, citation, or architecture rules above,
  flag it explicitly before writing any code

## Coding Conventions — Backend (.NET)

- PascalCase for classes/methods/public members, camelCase for private fields/locals,
  `I` prefix for interfaces. Async methods end in `Async`.
- Constructor injection only — no service locator pattern.
- Custom exception types per domain concern, caught by global exception middleware —
  never return raw exception messages to API responses.
- Structured logging via `ILogger<T>`, include `WorkspaceId` in every log scope.
- All I/O must be `async`/`await` all the way up — no `.Result` or `.Wait()`.
- Never expose EF Core entities directly from API endpoints — always map to a DTO.
- FluentValidation (or equivalent) at the Application layer, not scattered in controllers.

## Coding Conventions — Frontend (Next.js/TypeScript)

- Functional components with hooks only — no class components.
- TypeScript strict mode — no `any` without a comment explaining why.
- Co-locate a feature's components, hooks, and API calls under `src/features/<feature>/`.
- Shared/reusable UI primitives (buttons, inputs) live in `src/components/ui/`.
- No inline styles — use the project's styling approach consistently (Tailwind/CSS
  modules — confirm which once chosen).
- API responses/requests are typed — no untyped `any` payloads from `fetch`.

## Do Not

- Do not bypass or hardcode around WorkspaceId scoping, ever
- Do not return a RAG answer without a source citation
- Do not modify existing EF Core migration files directly — create a new migration
- Do not call the database or vector store directly from the frontend
- Do not commit connection strings, API keys, or embedding provider credentials
- Backend secrets: use `dotnet user-secrets` for local dev, environment variables for
  production — never put real secrets in appsettings.json or appsettings.*.json, even
  gitignored ones. Those files hold structure/placeholders only.
- Frontend secrets: use `.env.local` (gitignored) for local dev, never commit real
  values to `.env` or `.env.example`
- Do not add new third-party packages (backend or frontend) without flagging it first
