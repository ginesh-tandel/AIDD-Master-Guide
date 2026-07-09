# DESIGN.md — AI Document Assistant Design System

Referenced by AGENTS.md. Any UI component built by an AI tool or a human must use
these tokens — do not introduce new colors, fonts, spacing values, or radii ad hoc.
If a new value is genuinely needed, add it here first, then use it.

---

## Color Palette

| Token | Hex | Use |
|---|---|---|
| `--color-primary` | `#TBD` | Primary actions, links, active states |
| `--color-neutral-900` | `#TBD` | Primary text |
| `--color-neutral-600` | `#TBD` | Secondary text |
| `--color-neutral-200` | `#TBD` | Borders, dividers |
| `--color-neutral-50` | `#TBD` | Page/card backgrounds |
| `--color-success` | `#TBD` | "Ready" status, success toasts |
| `--color-warning` | `#TBD` | "Processing" status |
| `--color-error` | `#TBD` | Errors, "Failed" status |

*(Fill in real hex values once chosen — see "Next Step" at the bottom.)*

## Typography

| Role | Font | Notes |
|---|---|---|
| Body / UI text | [TBD — pick a highly legible sans-serif] | Optimize for reading long AI answers, not personality |
| Headings | [TBD — can match body or be a slightly heavier weight of the same family] | Don't introduce a second decorative typeface for an enterprise tool |
| Monospace / data | [TBD — e.g. a system mono stack] | Only if showing code, IDs, or raw data |

## Spacing Scale

Use Tailwind's default 4px-based scale — do not introduce arbitrary spacing values:
`4px, 8px, 12px, 16px, 24px, 32px, 48px, 64px`

## Border Radius

One value, used everywhere: `8px` (`rounded-lg` in Tailwind). Do not mix radii across components.

## Component States

Define once, reuse everywhere — do not redecide per screen:

- **Button**: default / hover (slightly darker primary) / active (darker still) / disabled (neutral-200 bg, neutral-600 text) / focus (visible outline, 2px, primary color)
- **Input**: default / focus (primary border + subtle ring) / error (error border + error text below) / disabled
- **Card**: default (neutral-50 bg, neutral-200 border) / hover (subtle shadow lift) if interactive

## Key UI Pattern — Citations

This is the product's core differentiator — treat it as a first-class component, not a generic tooltip:

- Inline citation marker: small superscript chip (e.g. `[¹]`) immediately after the claim it supports
- On hover/click: expands to show source document name + the specific passage, with a link to open the full document viewer
- Never render an AI answer without at least one citation chip attached

## Empty States

- **Empty document library**: explain what to do next ("Upload your first document to start asking questions"), not just "No documents"
- **Empty chat**: invite action ("Ask a question about your documents"), don't leave a blank message list

## Do Not

- Do not introduce new colors/fonts/spacing outside this file without updating it first
- Do not use a generic AI-tool look (cream + terracotta, or black + neon accent) — this is an enterprise document tool, prioritize trust/clarity over novelty
- Do not ship a citation-less AI answer, ever — this is a design rule as much as a backend rule (see AGENTS.md)

---

## Next Step

The hex values and font choices above are placeholders. Before building real components,
lock these in — either:
1. Ask for a rendered mockup (color palette + typography applied to the chat screen) to react to, or
2. Provide brand colors/fonts if you already have them (e.g. from LogiqCube Consulting branding)

Once locked, replace every `[TBD]` above with the real value.
