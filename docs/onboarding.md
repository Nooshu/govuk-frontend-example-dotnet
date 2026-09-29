# Onboarding

Human-oriented map of this repository. Coding agents should treat [`AGENTS.md`](../AGENTS.md) as the dense entry point; humans should also read [`CONTRIBUTING.md`](../CONTRIBUTING.md). How docs are split for both audiences: [documentation-structure.md](documentation-structure.md).

## What this repo is

A **C# / ASP.NET Core** example of a **GDS-compliant** frontend. **GOV.UK Frontend** is the only UI library. **No** frontend frameworks and **no** Blazor for UI. Exact **HTML parity** against official Frontend fixtures. See [project-purpose.md](project-purpose.md) and [tech-stack.md](tech-stack.md).

The application language is **C# on .NET 10**. New code follows current ASP.NET Core practice: SDK projects, nullable reference types, Razor Pages, and xUnit.

Install `govuk-frontend` from npm. Treat its `template.njk` and `fixtures.json` as the HTML contract, and keep the Node scripts that refresh and verify those fixtures. The running site renders in C#.

**Official guidance:** search the URLs in [guidance-sources.md](guidance-sources.md).

**Priorities:** frontend web performance → frontend security → reduced maintenance → accessibility → inclusive design ([priorities.md](priorities.md)).

**Documentation:** every lasting change is documented for **humans and agents** ([documentation-structure.md](documentation-structure.md)).

**HTML:** C# renderers track `govuk-frontend` templates. Official fixtures prove **100% parity** of that C# output. Do not copy-paste component HTML from each release as the long-term approach. Before Frontend upgrades, always read https://github.com/alphagov/govuk-frontend/releases/latest.

## Priorities

See [priorities.md](priorities.md). Short version: frontend web performance → frontend security → reduced maintenance → accessibility → inclusive design.

## Components vs patterns

| Kind          | What it is                                                               | How we build it                                  | Fixture parity?                                                         |
| ------------- | ------------------------------------------------------------------------ | ------------------------------------------------ | ----------------------------------------------------------------------- |
| **Component** | Design System building block (button, text input, …)                     | Library wrapper that renders exact Frontend HTML | **Yes** — official `fixtures.json`                                      |
| **Pattern**   | Guidance for a journey or page composition (addresses, check answers, …) | Compose shipped components into pages            | **No** — follow Design System guidance; no invented pattern HTML suites |

## Repo map

```text
AGENTS.md                          # Slim agent playbook
docs/                              # All documentation (this folder)
baseline/                          # Performance + OWASP header contract
styles/                            # Sass entry + govuk-overrides
scripts/                           # Node build helpers (styles and fingerprinted assets)
src/GovUk.Frontend/                # C# component renderers
src/GovUk.Frontend.Example/        # Razor Pages host, catalogue, fishing journey
tests/GovUk.Frontend.Tests/        # Fixture parity
tests/GovUk.Frontend.Example.Tests/ # HTTP tests for the journey and catalogue
tests/govuk-fixtures/              # Nunjucks freshness check
```

## Run modes

| Mode    | Command                                                                                                  |
| ------- | -------------------------------------------------------------------------------------------------------- |
| Preview | `npm start` — see [preview-server.md](preview-server.md)                                                 |
| Test    | `dotnet test` with the coverage settings in [tech-stack.md](tech-stack.md), plus `npm test`              |
| Verify  | `npm run verify`, then `dotnet format --verify-no-changes` and the two `dotnet test --settings` commands |
| Upgrade | [upgrading-govuk-frontend.md](upgrading-govuk-frontend.md)                                               |

**Node** is still required to install `govuk-frontend`, compile Sass, and run the Nunjucks freshness check. The running site does not call Node.

## Testing mindset

1. **Parity checks (primary)** compare **backend / library** output to fixture `html` with ordinal string equality — every fixture from the pinned Frontend release.
2. **Nunjucks suite (secondary)** compares Frontend macros to stored fixture `html` to catch **stale fixtures** only.
3. **Never** edit fixture `html` to make tests pass — fix the renderer.
4. **Never** normalise HTML in tests.
5. A green Nunjucks suite alone does **not** prove the C# renderers are correct.

Details: [testing-components.md](testing-components.md).

## Troubleshooting

| Symptom                        | Likely cause                                                |
| ------------------------------ | ----------------------------------------------------------- |
| Parity fails on whitespace     | Renderer ≠ Nunjucks `template.njk` / `{%-` stripping        |
| Encoding differs (`'` vs `'`)  | Used framework HTML encoder instead of Nunjucks `escape`    |
| Attribute order differs        | Built attributes in code property order, not template order |
| Preview/fixture 404 in tests   | Test host not enabling Dev/Testing routes                   |
| Logo unreadable / wrong header | Frontend 5 header classes with Frontend 6+ CSS              |
| Editing fixtures “fixes” tests | Wrong fix — update renderer                                 |

More pitfalls: [creating-components.md](creating-components.md).

## Consistency tooling (today)

Node tooling keeps docs, Sass, and the fixture freshness check consistent:

```sh
npm install
npm run build:styles # Sass → dist/stylesheets/application.css
npm test             # baseline headers + Sass pipeline (100% coverage)
npm run verify:docs  # Prettier + markdownlint
npm run verify       # docs + build:styles + tests
```

See [CONTRIBUTING.md](../CONTRIBUTING.md). Dotfiles: `.editorconfig`, `.prettierrc.json`, `.markdownlint-cli2.jsonc`, `.nvmrc`, `.vscode/`, `.cursor/rules/`, `.github/`.

## Next reads

1. [documentation-structure.md](documentation-structure.md)
2. [tech-stack.md](tech-stack.md)
3. [page-shell.md](page-shell.md) and [layout-chrome.md](layout-chrome.md)
4. [govuk-components.md](govuk-components.md)
5. [service-assessment-readiness.md](service-assessment-readiness.md)
6. [frontend-performance.md](frontend-performance.md) and [frontend-security.md](frontend-security.md)
7. [styles.md](styles.md)
8. [upgrading-govuk-frontend.md](upgrading-govuk-frontend.md)
