# Contributing

Thanks for helping maintain this **GDS-compliant frontend** template. This guide is for **human developers**. Coding agents should follow [`AGENTS.md`](AGENTS.md) and the playbooks under [`docs/`](docs/README.md).

## Before you start

1. Read [`docs/project-purpose.md`](docs/project-purpose.md) and [`docs/onboarding.md`](docs/onboarding.md).
2. Read the .NET conventions in [`docs/tech-stack.md`](docs/tech-stack.md).
3. Prefer official guidance listed in [`docs/guidance-sources.md`](docs/guidance-sources.md).
4. Priorities: frontend web performance → frontend security → reduced maintenance → accessibility → inclusive design.

## Non-negotiables (short)

- GOV.UK Frontend only for UI — **no** React/Vue/Angular/Svelte (etc.).
- C# renderers track Frontend templates. Do not copy-paste HTML from releases.
- Official fixtures for **100% HTML parity** of **C#** output vs every fixture `html`; never edit fixture `html` to pass tests. The Nunjucks check is freshness only.
- **100%** code coverage (functions, branches, statements) when application code exists.
- Before upgrading Frontend, read https://github.com/alphagov/govuk-frontend/releases/latest.
- Document every change for **humans and agents** ([docs/documentation-structure.md](docs/documentation-structure.md)).
- Follow current ASP.NET Core practice in [docs/tech-stack.md](docs/tech-stack.md).

Full list: [`AGENTS.md`](AGENTS.md).

## Consistency tooling

```sh
npm ci
npm run verify       # docs, Sass, Node tests (baseline, assets, Nunjucks freshness)
dotnet format GovUk.Frontend.Example.slnx --verify-no-changes
dotnet test tests/GovUk.Frontend.Tests/GovUk.Frontend.Tests.csproj --settings coverlet.library.runsettings
dotnet test tests/GovUk.Frontend.Example.Tests/GovUk.Frontend.Example.Tests.csproj --settings coverlet.example.runsettings
```

Details and the .NET 10 pin: [`docs/tech-stack.md`](docs/tech-stack.md). CI is [`.github/workflows/docs.yml`](.github/workflows/docs.yml).

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
