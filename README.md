# AI-Driven Development (AIDD) Guide

Personal reference for setting up AI-Driven Development on any project —
one canonical rules file per project, read the same way by OpenCode, Codex,
Claude Code, and Cursor, with risky actions (commit, push, migrations)
enforced rather than just requested.

**Start here → [AIDD-Master-Guide.md](./AIDD-Master-Guide.md)**

## What's inside the guide

- The AIDD workflow loop (Plan → Implement → Validate → Commit/Push → Conflict-check)
- Copy-paste templates: `AGENTS.md`, `CLAUDE.md`, `.cursor/rules/main.mdc`, `opencode.json`
- Global `~/.claude/CLAUDE.md` template (personal preferences, set up once, applies everywhere)
- Monorepo variant (frontend + backend split)
- A new-project checklist (10-15 min per project)

## How to use this

For any new project:

1. Open `AIDD-Master-Guide.md`
2. Jump to the checklist (Section 8)
3. Copy the templates, fill in the blanks for that project
4. Commit the resulting files to that project's own repo

This repo itself doesn't get copied into a project — only the templates inside
the guide do.
