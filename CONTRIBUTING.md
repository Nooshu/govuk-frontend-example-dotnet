# Contributing

Thanks for helping maintain this **GDS-compliant frontend** template. This guide is for **human developers**. Coding agents should follow [`AGENTS.md`](AGENTS.md) and the playbooks under [`docs/`](docs/README.md).

## Before you start

1. Read [`docs/project-purpose.md`](docs/project-purpose.md) and [`docs/onboarding.md`](docs/onboarding.md).
2. Confirm the wrapper language status in [`docs/tech-stack.md`](docs/tech-stack.md).
3. Prefer official guidance listed in [`docs/guidance-sources.md`](docs/guidance-sources.md).
4. Priorities: frontend web performance → frontend security → reduced maintenance → accessibility → inclusive design.

## Non-negotiables (short)

- GOV.UK Frontend only for UI — **no** React/Vue/Angular/Svelte (etc.).
- Prefer **Nunjucks macros** over copy-pasted HTML from releases.
- Official fixtures for **100% HTML parity**; never edit fixture `html` to pass tests.
- **100%** code coverage (functions, branches, statements) when application code exists.
- Before upgrading Frontend, read https://github.com/alphagov/govuk-frontend/releases/latest.
- Document every change for **humans and agents** ([docs/documentation-structure.md](docs/documentation-structure.md)).
- Follow the **latest** best practices for the language in [docs/tech-stack.md](docs/tech-stack.md).

Full list: [`AGENTS.md`](AGENTS.md).

## Consistency tooling (today)

While the wrapper language is TBD, Node tooling keeps docs and the shared performance/security baseline consistent:

```sh
npm install
npm run build:styles # Sass → dist/stylesheets/application.css
npm test             # baseline/ + styles pipeline — 100% lines, branches, functions
npm run verify:docs  # format:check + lint:md
npm run verify       # verify:docs + build:styles + test
```

When the wrapper language is chosen, add its format/lint/test/coverage commands to [`docs/tech-stack.md`](docs/tech-stack.md) and wire them into CI.

### Dotfiles (do not bypass)

| File                                        | Role                                                   |
| ------------------------------------------- | ------------------------------------------------------ |
| `.editorconfig`                             | Indentation, charset, newlines across editors          |
| `.gitignore` / `.gitattributes`             | Ignore hygiene + line endings                          |
| `.nvmrc` / `.npmrc`                         | Node version + npm behaviour for Frontend/docs tooling |
| `.prettierrc` / `.prettierignore`           | Shared formatting                                      |
| `.markdownlint-cli2.jsonc`                  | Markdown consistency                                   |
| `.vscode/settings.json` / `extensions.json` | Shared editor defaults                                 |
| `.cursor/rules/`                            | Agent consistency rules                                |
| `.github/`                                  | PR template, Dependabot                                |

## Pull requests

- Keep changes focused; update `/docs` (and `AGENTS.md` links) when behaviour or process changes — dual audience, same PR.
- Follow the recorded language’s latest best practices; do not introduce outdated stack idioms.
- Do not commit unless maintainers ask in agent sessions; humans use normal git workflow.
- Use the PR template checklist.
- For Frontend bumps: follow [`docs/upgrading-govuk-frontend.md`](docs/upgrading-govuk-frontend.md).
- Split finished work into focused commits with comprehensive messages when landing multiple concerns.

## Licence and security

- [MIT License](LICENSE)
- [SECURITY.md](SECURITY.md) — private vulnerability reporting; Frontend upgrade and encoding expectations

## Documentation for both audiences

See [`docs/documentation-structure.md`](docs/documentation-structure.md). Put detail in `/docs`; keep `AGENTS.md` as the agent index.
