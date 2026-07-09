# AI-Driven Development (AIDD) Framework
### Universal process/rules for LogiqCube Consulting projects — works across OpenCode, Claude Code, Codex, Cursor

---

## 1. Core Principle

Every AI coding tool reads its own file name (`CLAUDE.md`, `AGENTS.md`, `.cursor/rules`, `codex.md`), but they should **never contain different rules**. Maintain **one canonical file** and point every tool at it.

```
/project-root
  AGENTS.md          ← canonical rules (source of truth)
  CLAUDE.md           → "See AGENTS.md. Follow it exactly."
  .cursor/rules/main.mdc → "See AGENTS.md. Follow it exactly."
  codex.md            → "See AGENTS.md. Follow it exactly."
```

`AGENTS.md` is the emerging cross-tool standard (Codex, OpenCode, and several others read it natively). Claude Code and Cursor need a one-line pointer file since they don't auto-discover `AGENTS.md` by default.

Keep `AGENTS.md` under ~150 lines. It's a behavioral contract, not documentation — every line should change what the AI does. Deep or path-specific rules go in `/rules/*.md` and get referenced, not pasted in.

---

## 2. The AIDD Workflow Loop

Every task — regardless of tool — follows the same five stages. This is the loop your `AGENTS.md` should encode.

| Stage | What happens | Who confirms |
|---|---|---|
| **1. Plan** | AI states what it will change, in which files, before touching code | You approve or redirect |
| **2. Implement** | AI makes the change — minimal diff, no unrelated refactors | — |
| **3. Validate** | Build/tests/lint run automatically before anything is considered "done" | AI reports pass/fail |
| **4. Commit & Push** | Only runs if validation passed. One logical change per commit. | Conditional — never on failure |
| **5. Conflict Check** | If merge conflicts or ambiguous instructions appear, AI stops and asks — never guesses | You resolve, then AI continues |

This mirrors what you already run in OpenCode Prompt Generator — the difference here is making it the **default for every project**, not something re-specified each time.

---

## 3. Canonical `AGENTS.md` Template

```markdown
# AGENTS.md — LogiqCube Consulting Standard Rules

## Project Context
- Stack: .NET 8 / ASP.NET Core MVC / SQL Server (Clean Architecture)
- Multi-tenant: every request must carry UserId + WorkspaceId
- [Add project-specific stack lines here]

## Planning & Change Control
- For any non-trivial change (new feature, multi-file edit, architecture change),
  state a brief plan BEFORE writing code. Wait for confirmation on anything
  touching auth, data ownership, or schema.
- Minimal diffs only. Do not refactor unrelated code.
- One logical change per commit.

## Validation Rules (mandatory before commit)
- Run: `dotnet build` — must succeed with zero errors
- Run: `dotnet test` — all tests must pass
- Run linter/formatter if configured
- If validation fails: STOP. Report the failure. Do not commit. Do not push.

## Commit & Push Rules
- Only commit after validation passes.
- Commit message format: `<type>: <short description>` (feat/fix/refactor/docs/chore)
- Push only after commit succeeds locally.
- Never force-push without explicit confirmation.

## Conflict-Handling Rules
- If a merge conflict, ambiguous requirement, or contradicting instruction appears:
  STOP and ask. Do not guess intent, do not silently pick a side.
- If a requested change conflicts with an existing architectural rule in this file,
  flag the conflict explicitly before proceeding.

## Coding Conventions
- [Naming conventions, folder structure, DI patterns, etc.]
- [Error handling / logging standards]

## Do Not
- Do not modify migration history files directly
- Do not bypass WorkspaceId scoping on any query
- Do not commit secrets, connection strings, or API keys
```

---

## 4. Tool-Specific Setup Notes

| Tool | Reads by default | Setup needed |
|---|---|---|
| **OpenCode** | `AGENTS.md` | None — works natively |
| **Codex** | `AGENTS.md` (walks up directory tree) | None — works natively |
| **Claude Code** | `CLAUDE.md` (project + `~/.claude/CLAUDE.md` global + nested dirs) | Add `CLAUDE.md` with one line: `See AGENTS.md — treat every rule in it as binding.` Claude Code also has **auto memory**: it writes its own notes from your corrections over time (build commands, style fixes) — separate from `AGENTS.md`, review it occasionally with `/memory`. |
| **Cursor** | `.cursor/rules/*.mdc` | Add `.cursor/rules/main.mdc` with the same pointer line, or import `AGENTS.md` content directly since Cursor rule files support frontmatter scoping (`alwaysApply: true`) |

Keep the pointer files literally one line each — duplicating the rules invites drift between tools.

---

## 5. Prompting Pattern (use for every session start)

```
Read AGENTS.md fully before starting. Confirm you understand the
Validation and Conflict-Handling rules. Then:

TASK: <describe the task>
SCOPE: <files/modules expected to change>
CONSTRAINTS: <anything project-specific for this task>

Plan first. Wait for my go-ahead before implementing.
```

This forces stage 1 (Plan) explicitly rather than relying on the tool to remember.

---

## 6. Maintenance Rule

Review `AGENTS.md` every 4–6 weeks or after a recurring correction happens twice. If you find yourself explaining the same thing to the AI more than once, that's the signal to add a line — not before. A shrinking, high-signal file beats a growing one.

---

## Next Steps

1. Drop this `AGENTS.md` into LogiqLead's repo root and adapt the stack/convention sections.
2. Add the 3 one-line pointer files for whichever tools you actually use.
3. Reuse the same `AGENTS.md` skeleton for every new project — only Section 1 (Project Context) and Coding Conventions change.
