# Tech stack

**Status: TBD** (implementation language)

The _example implementation_ language and its 2026 best-practice templating / component approach are not chosen yet. This file is the **single place** to record them when decided.

## Two layers

| Layer                          | Stack                                                                                               | Notes                                                                                                                                      |
| ------------------------------ | --------------------------------------------------------------------------------------------------- | ------------------------------------------------------------------------------------------------------------------------------------------ |
| **GOV.UK Frontend (upstream)** | **Node** package (`govuk-frontend`), **Nunjucks** macros (`template.njk`), official `fixtures.json` | Fixed by GDS. Always name Node/Nunjucks when discussing install, fixtures, macro options, escape behaviour, and verifying stored fixtures. |
| **This template (wrapper)**    | TBD — e.g. TypeScript, Go, Python; **prefer Nunjucks** for GOV.UK HTML when viable                  | Server-side HTML from Frontend **macros**, not pasted release HTML. **No** React/Vue/Angular/Svelte for UI.                                |

## Rule for agents and humans

Until an implementation language is recorded here: do not invent wrapper-specific paths, package managers, or framework idioms.

Once recorded: **every** feature request and code change must follow **that language’s latest best practices** for project layout, typing, modules, testing, packaging, and CI — while honouring Frontend’s Nunjucks/fixture contract in [`AGENTS.md`](../AGENTS.md). Prefer current stable idioms for the recorded major version over outdated tutorials. **Prefer Nunjucks macros** for component HTML where the stack allows (e.g. Node/TypeScript calling `govuk-frontend` macros directly). If the language cannot call Nunjucks, implement thin renderers that stay byte-for-byte with fixtures — still do **not** maintain hand-copied HTML dumps from each release.

Shared Node tooling in this repo (Sass pipeline, `baseline/`, docs scripts) already uses current ESM / Node 22+ practice; keep it that way.

Document stack decisions and “how we write X here” notes in this file when the language is chosen, so humans and agents share one source of truth.

## Consistency tooling (today)

While the wrapper language is TBD, Node tooling keeps docs and shared config consistent:

```sh
npm install
npm test          # baseline/ headers and cache policy; Sass pipeline
npm run build:styles
npm run verify    # docs + build:styles + tests
```

See [CONTRIBUTING.md](../CONTRIBUTING.md). Dotfiles: `.editorconfig`, `.prettierrc.json`, `.markdownlint-cli2.jsonc`, `.nvmrc`, `.vscode/`, `.cursor/rules/`, `.github/`. Record language-specific formatters in this file when chosen.

## Shared baseline (language-agnostic)

[`baseline/`](../baseline/) is part of this template’s contract. Language lines sync that directory with this repo.

| Piece                                             | Role                                                                                               |
| ------------------------------------------------- | -------------------------------------------------------------------------------------------------- |
| [`baseline/policy.json`](../baseline/policy.json) | OWASP header values, CSP directives, cache kinds, performance budgets                              |
| [`baseline/index.mjs`](../baseline/index.mjs)     | Node helpers: `buildResponseHeaders`, `applyResponseHeaders`, `buildSetCookie`, preload and `ETag` |
| [`styles/`](../styles/)                           | Sass entry compiling Frontend via `@use`, then `govuk-overrides.scss` ([styles.md](styles.md))     |

Node and TypeScript services call the helpers. Other languages implement the same `kind` values and header map, and test against the Node output. Production HTTPS passes `secureTransport: true`. Details: [frontend-performance.md](frontend-performance.md), [frontend-security.md](frontend-security.md).

Expect a **Node** dependency (and often small Node scripts) even when the wrapper is another language — that is how you install `govuk-frontend`, refresh fixtures, re-render Nunjucks for stale-fixture checks, run the baseline tests, and run shared docs hygiene.

## When implementation language is confirmed, document

- Language, runtime, and version policy
- Templating approach: **Nunjucks macros preferred**; document any non-Nunjucks renderer and how fixture parity is proven
- Package manager, lockfile, and how dependencies are pinned (including `govuk-frontend` via npm/Node)
- How Frontend CSS/JS (and fonts) are installed and served: Sass compile of `styles/application.scss`, fingerprinted URL, `buildResponseHeaders` as `fingerprinted-asset`
- How every HTTP response applies [`baseline/`](../baseline/) (`secureTransport: true` in production)
- Shared HTML escape + attribute helpers matching **Nunjucks `escape`** when not invoking Nunjucks directly (see [creating-components.md](creating-components.md))
- Fixture loader and preview / raw-fixture route conventions (extensive parity coverage)
- Layout chrome helpers (skip link, header, service navigation, footer)
- Test runner commands, parity suite over **all** fixtures, **100%** coverage gate (functions / branches / statements), and **Nunjucks fixture-verification** scripts (Node)
- Upgrade entrypoint — always review https://github.com/alphagov/govuk-frontend/releases/latest first; see [upgrading-govuk-frontend.md](upgrading-govuk-frontend.md)
- Confirmation that no frontend UI framework is in the dependency tree for rendering

## Hard constraints (always)

- GOV.UK Frontend pins a single version; CSS/JS and fixtures must match.
- Prefer **Nunjucks macros** for component HTML; do not maintain copy-pasted HTML from each release.
- Component options mirror Nunjucks macro options (`macro-options.json` / fixture `options`).
- Backend output must pass extensive **100% HTML fixture parity** (byte-for-byte with Nunjucks / fixture `html`).
- Compile CSS via Sass ([styles.md](styles.md)); `govuk-overrides.scss` last; never `!important` in service CSS.
- Patterns compose components; they are not new low-level components.
- Wrapper structure/tooling follow the **chosen language’s best practices**; Frontend tooling stays Node/Nunjucks.
- No frontend UI frameworks for GOV.UK chrome — see [project-purpose.md](project-purpose.md).
- Coverage: **100%** functions, branches, statements — see [testing-components.md](testing-components.md).
- Before every Frontend upgrade: https://github.com/alphagov/govuk-frontend/releases/latest

See [`AGENTS.md`](../AGENTS.md), [guidance-sources.md](guidance-sources.md), and [creating-components.md](creating-components.md).

## Placeholder version pin

| Item                              | Value                                                                                                        |
| --------------------------------- | ------------------------------------------------------------------------------------------------------------ |
| Implementation language           | _TBD_                                                                                                        |
| Templating / component approach   | _TBD — prefer Nunjucks macros when viable_                                                                   |
| `govuk-frontend` (Node)           | `6.5.1` — check [latest release](https://github.com/alphagov/govuk-frontend/releases/latest) before upgrades |
| Sass pipeline                     | `styles/application.scss` → `npm run build:styles` → `dist/stylesheets/application.css`                      |
| Nunjucks fixture verification     | _TBD — Node scripts under tests/_                                                                            |
| Page template reference           | https://design-system.service.gov.uk/styles/page-template/                                                   |
| Fixture testing guide             | https://frontend.design-system.service.gov.uk/testing-your-html/                                             |
| Upgrade / test / preview commands | _TBD — list here when wired_                                                                                 |
