# Testing components

How we keep **100% HTML parity** with GOV.UK Frontend — now and after upgrades.

Authoritative upstream: https://frontend.design-system.service.gov.uk/testing-your-html/

## What must be compared

| Compare                                         | Required?              | Purpose                                                              |
| ----------------------------------------------- | ---------------------- | -------------------------------------------------------------------- |
| **C# library HTML → official fixture `html`**   | **Yes — primary gate** | Proves the C# renderer matches the release                           |
| **Nunjucks macro HTML → stored fixture `html`** | Yes — secondary        | Proves fixtures are not stale relative to the pinned Frontend macros |

**Do not** ship a setup that only compares Nunjucks templates to fixture HTML. That never exercises the C# renderer. The parity suite compares C# output with **every** fixture in every shipped component’s `fixtures.json` from the pinned GOV.UK Frontend release.

The Nunjucks suite stays on Node with the pinned package. It checks that the stored fixtures are still fresh.

## Why fixtures exist

Official `fixtures.json` files (one set per component per Frontend release) are the contract for **extensive 100% HTML parity testing** of the C# renderers. Sync them from the same `govuk-frontend` version as CSS/JS. Every shipped component proves byte-for-byte equality, so upgrades catch drift.

Do **not** treat copy-pasted HTML from Design System examples or release notes as the source of truth. Do **not** treat “Nunjucks still matches fixtures” as proof the C# renderer is correct.

## Coverage gate (100% code)

Application and library code under test must maintain **100%** coverage of:

- **functions**
- **branches**
- **statements**

Coverlet fails the build below 100% on all three. Do **not** normalise fixture HTML or skip parity cases to inflate coverage. Exclude only generated Razor views and the exceptions recorded in [tech-stack.md](tech-stack.md).

## Layers

| Layer                          | What it proves                                   | How                                                                                                                              |
| ------------------------------ | ------------------------------------------------ | -------------------------------------------------------------------------------------------------------------------------------- |
| **Parity suite (primary)**     | **C# library HTML** matches fixture `html`       | Map fixture `options` → **C# render** → ordinal string equality vs fixture `html` — cover **all** fixtures extensively           |
| **Nunjucks suite (secondary)** | Stored fixtures still match Frontend macros      | Node scripts render via `govuk-frontend` Nunjucks; compare to fixture `html` — freshness only; does not replace the parity suite |
| **HTTP smoke** (if applicable) | Preview/fixture surfaces wired                   | Hit preview and raw-fixture endpoints in a Testing env                                                                           |
| **Structural**                 | Nav lists components; no demos on index          | Assert homepage entries and absence of embedded demos                                                                            |
| **Fixture sync**               | App fixtures ≡ copies used by the Nunjucks suite | Byte-identical file check                                                                                                        |

## Hard rules

1. **Never** edit fixture `html` to make tests pass.
2. **Never** normalise or pretty-print HTML before compare.
3. **Never** mix Frontend versions between CSS/JS and fixtures.
4. Prefer in-process parity for the bulk of cases (fast, no HTTP); exercise **every** fixture for each shipped component through the **C#** renderer.
5. After upgrade, both the **parity suite** and the **Nunjucks suite** must be green — see [upgrading-govuk-frontend.md](upgrading-govuk-frontend.md). Always read https://github.com/alphagov/govuk-frontend/releases/latest first.
6. **Never** treat a green Nunjucks suite alone as proof the C# renderers are done.

## Encoding

Parity depends on the same escaping as the Frontend templates (`&#39;` for `'`, real newlines in textarea values). The C# helper in [creating-components.md](creating-components.md) emits the encoding the fixtures expect.

## Commands

Document in [tech-stack.md](tech-stack.md):

- **library / parity tests** (C# output vs **all** fixtures) — mandatory
- coverage report enforcing **100%** functions, branches, statements
- Nunjucks fixture verification (Node + `govuk-frontend`) — freshness
- a single “verify” gate for local + CI that includes the parity suite

Node’s test runner (`npm test`) covers the shared baseline, the Sass pipeline, asset fingerprinting, and Nunjucks fixture freshness. The C# parity suite is `dotnet test` with the coverage settings in [tech-stack.md](tech-stack.md). CI runs both.

## Preview as human parity browser

Previews render **one selected fixture**. The banner says the C# HTML matches only when that rendered string is identical to the fixture `html`. Details: [preview-server.md](preview-server.md).

## Adding tests for a new component

Follow steps 8–9 in [creating-components.md](creating-components.md). Cover every fixture name in the C# parity suite and the Nunjucks freshness check.
