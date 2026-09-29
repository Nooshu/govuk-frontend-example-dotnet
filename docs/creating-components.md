# Creating a new GOV.UK component

Use this playbook when implementing a Design System **component** as a library wrapper. For **patterns**, use [creating-patterns.md](creating-patterns.md) instead.

Official fixture guidance: [https://frontend.design-system.service.gov.uk/testing-your-html/#using-the-html-test-files](https://frontend.design-system.service.gov.uk/testing-your-html/#using-the-html-test-files)

Related docs: [govuk-components.md](govuk-components.md), [testing-components.md](testing-components.md), [upgrading-govuk-frontend.md](upgrading-govuk-frontend.md), [preview-server.md](preview-server.md).

**Stack note:** Structure the renderer in C# ([tech-stack.md](tech-stack.md)). Track the GOV.UK Frontend template rather than copy-pasting HTML from each release. Official `fixtures.json` is the **100% parity** contract for that C# output.

## Goals (non-negotiable)

1. **GOV.UK Frontend is the source of truth** — official CSS/JS and `fixtures.json` from the **same** pinned `govuk-frontend` Node package.
2. **Options mirror Nunjucks macros** — names/shapes align with `macro-options.json` / fixture `options`.
3. **Exact HTML parity (C# vs fixtures)** — C# output equals each fixture’s `html` byte-for-byte (ordinal equality). Cover every fixture. A Nunjucks-vs-fixture check is freshness only; it does not replace the C# comparison. See [testing-components.md](testing-components.md).
4. **Never hand-write component markup in pages** — pages invoke the library API.
5. **No ad-hoc custom CSS** — Sass pipeline + `govuk-overrides.scss` only; no `!important` ([styles.md](styles.md)).
6. **Register in navigation** — every shipped component appears in the home/components list with a preview link.

## Which renderer to copy

| If the component is…                       | Start from                                         |
| ------------------------------------------ | -------------------------------------------------- |
| Single element / simple attributes         | Back link or Button                                |
| List of items                              | Breadcrumbs or Accordion                           |
| Form control that **composes** other parts | Character count (read every nested `template.njk`) |

## Target layout (conceptual)

Exact filenames and folders follow [tech-stack.md](tech-stack.md). Conceptually each component needs:

```text
src/GovUk.Frontend/Components/
  renderer                         # builds the exact HTML string
  ParamBag options                 # fixtures.json options

preview page                       # selected fixture
raw fixture route                  # fragment only

tests/GovUk.Frontend.Tests/        # C# parity
tests/govuk-fixtures/              # Nunjucks freshness check (Node)
```

## Recommended work order

1. Copy official fixtures beside the component and into `tests/govuk-fixtures/` (or equivalent) for the Nunjucks suite.
2. Dump every fixture’s `html` + `options`; read Nunjucks `template.njk` (+ imports). Note attribute order and `{%-` / `-%}` whitespace stripping.
3. Implement models + mapper + renderer together — do not free-hand the component body in a way that drifts from the template.
4. Follow the existing preview page, parity tests, and `tests/govuk-fixtures/render-fixtures.mjs`.
5. Register homepage/nav entry (nav only — no live demos on the index).
6. Run parity tests → fix renderer/mapper only → Nunjucks suite → docs.
7. Visual check via preview ([preview-server.md](preview-server.md)).

## Step checklist

### 1. Obtain official fixtures

1. Confirm Design System “when to use / when not to use”.
2. From pinned `govuk-frontend` (npm/Node), copy `dist/govuk/components/<kebab-name>/fixtures.json`.
3. Skim `template.njk` and `macro-options.json`.
4. Keep a byte-identical copy for the Nunjucks suite.
5. **Never edit fixture** `html`**.** Refresh only when upgrading Frontend.

### 2. Models (Nunjucks-aligned)

- Options mirror the template parameters (`id`, `classes`, `attributes`, …) through `ParamBag`.
- Use a text/html pair: prefer `text` (encoded); `html` only for **trusted** markup; if both set, `html` wins; sanitise untrusted input.
- Map JSON carefully (`headingLevel` and the other template names stay as the fixture spells them).
- Handle edge cases: falsy array entries, **tri-state booleans** (unset vs `false`), nested `label` / `hint` / `errorMessage` / `formGroup` / i18n maps.
- JSON numbers that Nunjucks stringifies (`maxlength`, `rows`, …) must round-trip as strings in HTML attributes where fixtures do.

### 3. Renderer

Prefer a dedicated renderer that returns a string, invoked by `ComponentCatalog.Render`.

**Read Nunjucks** `template.njk` **as the contract.** Attribute order and newlines follow the template (including `{%-` stripping).

**Encoding — match Nunjucks** `escape`, not typical framework encoders:

| Character | Entity        |
| --------- | ------------- |
| `&`       | `&`           |
| `<`       | `<`           |
| `>`       | `>`           |
| `"`       | `"`           |
| `'`       | `'`           |
| Newlines  | leave as `\n` |

Use one shared escape + attributes helper for every renderer.

**Composed macros:** If `template.njk` calls other components, implement the **composed output** in this renderer (inline helpers OK). Refactor to shared helpers later without breaking parity.

Other rules:

- Match indentation/newlines from fixture `html`.
- Preserve attribute and i18n map key order (JSON object order).
- Optional attributes: omit when falsy.
- Hard-code exact SVG / static fragments from the template when present.

### 4. Public API

The public entry accepts a `ParamBag` and returns an HTML string. Pages call `GovUkHtml.Component` — they do not paste markup.

### 5. Options mapper

Map fixture `options` → model without losing edge cases: nested text/html, attribute dictionaries, number→string, tri-state booleans, omit empty error wrappers.

### 6. Fixtures endpoint (raw HTML)

- Expose a Dev/Testing-only way to fetch one fixture as a **fragment** (no layout).
- Route naming is stack-specific; document it in [tech-stack.md](tech-stack.md).

### 7. Previews surface

- List fixture names; **render only the selected** fixture.
- Show the parity banner only from an ordinal comparison of the rendered C# HTML with the fixture `html`. The success text is “The C# HTML is the same as the official fixture HTML.”
- Back link to home — never use fixture hrefs for chrome.
- Syntax highlighting assets on Previews only — not global layout.
- Development / Testing only.

### 8. Tests

1. Structural/smoke: index lists preview; no embedded demo on index.
2. Parity (**primary**): one case per fixture name; in-process `ParamBag` + C# renderer; ordinal string equality to fixture `html`. This is the C# renderer under test.
3. HTTP checks for the catalogue and raw fragment live in the example test project.
4. Use xUnit. Registering the component in `ComponentCatalog` picks the fixture up in the data-driven parity test.

### 9. Nunjucks fixture verification (Node)

- Depend on the same `govuk-frontend` pin via npm/Node.
- Script renders each fixture through Frontend’s Nunjucks macros and compares to stored `html` (trim trailing newline only if needed).
- Purpose: catch **stale fixtures** only. The C# parity tests catch **renderer drift**.
- A green Nunjucks suite without a green C# parity suite is **not** done.
- Typical shape: `tests/govuk-fixtures/render-<kebab-name>-fixtures.mjs` — document the exact runner in [tech-stack.md](tech-stack.md).

### 10. Navigation + docs

- Add nav item (display name, one-line description, preview path, Design System URL).
- Add `docs/govuk-<kebab-name>.md` and link from [README.md](README.md), [govuk-components.md](govuk-components.md), [testing-components.md](testing-components.md), [upgrading-govuk-frontend.md](upgrading-govuk-frontend.md).
- Add an agent-usage section (when to use, model table, minimal example, do/don’t).

### 11. Visual verify

Use the preview server; hard-refresh after rebuilds. Confirm the component is listed and default fixture shows parity success.

## Common pitfalls

| Symptom                        | Likely cause                                         |
| ------------------------------ | ---------------------------------------------------- |
| Whitespace-only parity failure | Indentation ≠ Nunjucks; `{%-` strips spaces/newlines |
| `'` or `                       |                                                      |
| `                              | Framework encoder instead of Nunjucks `escape`       |
| Attribute order differs        | Code property order ≠ `template.njk` order           |
| Numbers wrong/missing          | JSON numbers not mapped like Nunjucks                |
| Tri-state bool wrong           | Coerced missing → `false`                            |
| i18n key order wrong           | Plural map order not preserved                       |
| Composed control fails         | Nested macros not implemented                        |
| IDs off by one                 | Skipped falsy items; must preserve positions         |
| Fixture route is a full page   | Must return fragment only                            |
| Preview back goes to `/home`   | Used fixture href for chrome                         |
| Editing fixtures “fixes” tests | Update renderer instead                              |

## Do / don’t

**Do:** start from fixtures and `template.njk`; keep one renderer per component; prove C# HTML matches every fixture before calling done; keep the Node freshness check.

**Don’t:** hand-paste `govuk-*` into pages; ship without a catalogue link; embed demos on the index; add ad-hoc CSS or `!important`; normalise HTML in tests; invent fixture HTML; ship the prebuilt minified Frontend CSS instead of the Sass pipeline; claim parity from the Nunjucks check alone.
