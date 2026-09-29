# Upgrading GOV.UK Frontend

Solid, simple update plan for keeping this example on a current, fixture-matched GOV.UK Frontend release.

**Goals:** mechanical sync where possible, exact HTML parity after every bump, no invented fixtures, C# renderers that track the Frontend templates, human visual QA at the end.

## Principles

1. **One pinned version** — CSS, JS, and every `fixtures.json` come from the same `govuk-frontend` release.
2. **Always read the latest release notes first** — https://github.com/alphagov/govuk-frontend/releases/latest (required before every upgrade).
3. **Fixtures are sacred** — never edit fixture `html` to pass tests; never loosen string equality.
4. **Prefer the C# renderers over pasted HTML** — update those renderers from `template.njk`; do not refresh by copy-pasting HTML from release notes into pages.
5. **Automate the boring parts** — download package, copy fixtures, refresh static assets, run parity suites.
6. **Fix the C# renderers, not the tests** — when HTML drifts, update the renderer so it still matches the template.
7. **Docs must track the pin** — search/replace the old version string across `/docs` and `AGENTS.md` after a successful upgrade.
8. **Do not commit unless asked.**

## When to upgrade

- Security or accessibility fixes in Frontend
- New components you need that have **shipped** with official fixtures
- Aligning with a service that already moved to a newer Frontend

Do **not** upgrade mid-feature unless required. **Always** open https://github.com/alphagov/govuk-frontend/releases/latest first, then check [govuk-frontend-roadmap.md](govuk-frontend-roadmap.md) and [all releases](https://github.com/alphagov/govuk-frontend/releases).

## Update plan (checklist)

### 0. Pre-flight

- [ ] **Mandatory:** read https://github.com/alphagov/govuk-frontend/releases/latest and summarise the target version’s notes (fixes, new components, breaking changes).
- [ ] For majors/minors, also skim prior notes on the [releases index](https://github.com/alphagov/govuk-frontend/releases) as needed.
- [ ] Confirm CI is green on the current pin (parity + 100% code coverage).
- [ ] Note major-version risk for humans (e.g. header / service navigation migrations).

### 1. Mechanical sync

C# renderers live in this repo. Installing `govuk-frontend`, refreshing fixtures, and the Nunjucks freshness check use Node. The commands are in [tech-stack.md](tech-stack.md).

An upgrade must:

1. **Pin** `govuk-frontend@X.Y.Z` (npm/Node) in the project’s dependency / lock mechanism.
2. **Publish Frontend assets** the app actually serves — CSS, JS, and fonts — so runtime assets match `X.Y.Z`.
3. **Refresh official fixtures** for every shipped component from that same release (`dist/govuk/components/<kebab-name>/fixtures.json`).
4. **Keep a manifest of shipped components** in sync so the sync step knows which fixture sets to update.
5. **Map upstream package names** to this repo’s names where they differ (e.g. Design System “Text input” ↔ upstream `input`).
6. **Prefer a single entrypoint** that can dry-run, sync-only, or sync-then-verify — exact flags are stack-specific; Node scripts are the usual way to talk to `govuk-frontend`.
7. **Run verification** after sync (C# parity vs fixtures + the Nunjucks freshness suite).

### 2. Renderer / mapper fixes

If parity fails:

- [ ] Diff failing fixture `html` vs renderer output (whitespace, attributes, encoding).
- [ ] Re-read Nunjucks `template.njk` (+ imported macros) for the new version.
- [ ] Update `ParamBag` handling only when template options changed.
- [ ] Fix encoding via the shared helper that matches **Nunjucks** `escape` — not the framework default encoder if it differs.

### 3. Page template / layout

- [ ] Apply page-template and chrome changes from release notes to the shared layout.
- [ ] Confirm skip link, header, service navigation, footer still match Frontend 6+ rules ([layout-chrome.md](layout-chrome.md)).
- [ ] If the `js-enabled` snippet changed, update `jsEnabledSnippet` and `jsEnabledScriptHash` in [`baseline/policy.json`](../baseline/policy.json) to the hash published in [import JavaScript](https://frontend.design-system.service.gov.uk/import-javascript/). Re-measure CSS and JS budgets in [frontend-performance.md](frontend-performance.md).
- [ ] Run `npm run build:styles` and fix Sass breaks; keep `govuk-overrides.scss` last and free of `!important` ([styles.md](styles.md)).

### 4. New upstream components

If the release adds a component you will ship:

- [ ] Follow [creating-components.md](creating-components.md) end-to-end.
- [ ] Add the kebab name to the shipped-components manifest used by mechanical sync.
- [ ] Add docs + homepage/preview nav entries.

### 5. Docs sweep

- [ ] `rg` (or equivalent) for the previous version string; update leftovers.
- [ ] Update the pin table in [tech-stack.md](tech-stack.md).
- [ ] Refresh [govuk-frontend-roadmap.md](govuk-frontend-roadmap.md) “already covered” line.
- [ ] Note any assessment-facing behaviour changes in [service-assessment-readiness.md](service-assessment-readiness.md) if relevant.

### 6. Verify and stop conditions

Report **done** only when:

- [ ] Fixture copy / sync checks are green
- [ ] **C# parity** tests are green (renderer output ≡ every fixture `html`)
- [ ] Nunjucks fixture verification (Node) is green (freshness only — not a substitute for the C# comparison)
- [ ] Config / CI verify gate is green

Then list what still needs **human visual QA** (preview server — [preview-server.md](preview-server.md)): header/footer, focus states, a sample form with errors, and any components touched by the release notes.

## Hard rules

| Do                                                                               | Don’t                                                                     |
| -------------------------------------------------------------------------------- | ------------------------------------------------------------------------- |
| Read https://github.com/alphagov/govuk-frontend/releases/latest before upgrading | Skip the latest release notes                                             |
| C# renderers that track `template.njk`                                           | Copy-paste HTML from release notes; shell out to Node to render a request |
| Summarise release notes before editing                                           | Edit fixture `html` to pass tests                                         |
| Fix renderers/mappers/macro usage                                                | Add HTML normalisation in tests                                           |
| Keep CSS/JS/fixtures on one version                                              | Mix Frontend versions                                                     |
| Add new components only with official fixtures                                   | Hand-build unreleased GOV.UK chrome                                       |
| Use the C# escape and attribute helpers                                          | Invent ad-hoc CSS or `!important` “fixes”                                 |
| Compile Frontend through Sass (`styles/` + `govuk-overrides.scss`)               | Ship `govuk-frontend.min.css` as the long-term CSS                        |

## Cadence (recommended)

| Trigger                                   | Action                                                                                                     |
| ----------------------------------------- | ---------------------------------------------------------------------------------------------------------- |
| Monthly                                   | Open https://github.com/alphagov/govuk-frontend/releases/latest + roadmap; open an upgrade issue if behind |
| After each successful upgrade             | Tag or note the pin in changelog / tech-stack                                                              |
| Before public beta of a consuming service | Re-verify parity + visual QA on the pin they will ship                                                     |

## Ownership

- **Agents** may run the mechanical upgrade and fix parity failures when asked.
- **Humans** approve major upgrades, perform visual QA, and decide whether to adopt new optional components.
