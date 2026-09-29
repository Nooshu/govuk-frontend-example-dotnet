# Tech stack

**Status: C# / ASP.NET Core / .NET 10**

This repository is the .NET example. GOV.UK Frontend stays the pinned Node package. Request HTML is native C#. Nunjucks is a behaviour reference and a fixture-freshness check only.

## Two layers

| Layer                          | Stack                                                                                        | Notes                                                                                                                                    |
| ------------------------------ | -------------------------------------------------------------------------------------------- | ---------------------------------------------------------------------------------------------------------------------------------------- |
| **GOV.UK Frontend (upstream)** | **Node** package (`govuk-frontend` **6.5.1**), **Nunjucks** macros, official `fixtures.json` | Fixed by GDS. Use Node for install, Sass, fonts, and the freshness check.                                                                |
| **This service**               | **C# / ASP.NET Core / .NET 10** (SDK `10.0.401`, `latestFeature` roll-forward)               | Razor Pages render HTML by calling the C# component library. **No** Blazor, React, Vue, Angular, or Svelte. **No** Node at request time. |

## How the .NET code is organised

Current SDK-style layout. Nullable reference types, implicit usings, file-scoped namespaces, central package management, analyzers, and warnings as errors.

| Path                                                                            | Role                                                                                            |
| ------------------------------------------------------------------------------- | ----------------------------------------------------------------------------------------------- |
| [`src/GovUk.Frontend/`](../src/GovUk.Frontend/)                                 | Component library. One renderer per fixture-bearing component.                                  |
| [`src/GovUk.Frontend.Example/`](../src/GovUk.Frontend.Example/)                 | Razor Pages host: layout, catalogue, fishing-rod-licence journey, baseline middleware, session. |
| [`tests/GovUk.Frontend.Tests/`](../tests/GovUk.Frontend.Tests/)                 | Data-driven parity for every fixture, plus escape, attributes, and header tests.                |
| [`tests/GovUk.Frontend.Example.Tests/`](../tests/GovUk.Frontend.Example.Tests/) | `WebApplicationFactory` coverage of the journey, catalogue, and headers.                        |
| [`baseline/`](../baseline/)                                                     | Header and cache contract. The app loads [`baseline/policy.json`](../baseline/policy.json).     |
| [`styles/`](../styles/)                                                         | Sass entry. Compiled CSS is fingerprinted into `wwwroot`.                                       |

Solution: [`GovUk.Frontend.Example.slnx`](../GovUk.Frontend.Example.slnx). Packages: [`Directory.Packages.props`](../Directory.Packages.props). SDK pin: [`global.json`](../global.json).

## HTML generation

Renderers return a string. Pages pass options as a `JsonObject`. The library reads them through `ParamBag`, which keeps JSON key order, distinguishes missing values from JSON null, and follows JavaScript truthiness. That is the options model: a typed C# view of the Nunjucks macro options, not a hand-written HTML dump.

`TrustedHtml` is a different type from plain text. `text` options are escaped with the Nunjucks rules (`&#39;` for an apostrophe). `html` options are inserted only when they already come from a trusted fragment (another component, or fixture HTML). Do not pass user input to a raw HTML API.

Fixture tests call `ComponentCatalog.Render` in process. They assert ordinal string equality with the official fixture `html`. On failure, AngleSharp prints a DOM diff. Tests do not normalise HTML and do not edit fixture `html`.

`tests/govuk-fixtures/render-fixtures.mjs` re-renders every fixture with Nunjucks (`trimBlocks`, `lstripBlocks`, then trim) and compares it to stored `html`. That only proves the fixtures are fresh.

## Assets, compression, and headers

`npm run build:styles` compiles [`styles/application.scss`](../styles/application.scss). `node scripts/build-assets-cli.mjs` fingerprints that CSS, the `initAll()` module, and the GOV.UK fonts and images under `/assets/{hash}/`. The app serves those URLs as the `fingerprinted-asset` cache kind. The `js-enabled` snippet is the one-line script in [`baseline/policy.json`](../baseline/policy.json) so the CSP hash stays `sha256-GUQ5ad8JK5KmEWmROf3LZd9ge94daqNvd8xy9YS1iDw=`.

ASP.NET middleware applies the same header map as `baseline/index.mjs`. `secureTransport` is true when the request is HTTPS or `X-Forwarded-Proto` is `https`. Response compression prefers Brotli and falls back to Gzip. Kestrel does not send `Server` or `X-Powered-By`.

Generated files under `wwwroot/assets/` and `wwwroot/asset-manifest.json` are build output. Do not commit them.

## Preview and the example journey

`/components` lists links only. `/components/{name}` lists every fixture, including hidden fixtures, and renders only the selected one with a parity banner. `/components/{name}/raw?fixture=` returns the fragment alone. `/` redirects to `/apply`.

Those routes stay on in every environment because this hosted site is the catalogue. A real service should limit them to Development.

The fictional journey is `/apply`. Personal-data pages use the `sensitive-document` cache kind. See [fishing-rod-licence.md](fishing-rod-licence.md) and [hosting.md](hosting.md).

## Commands

```sh
npm ci
npm run build:styles
npm test                 # baseline, Sass, asset fingerprint, Nunjucks freshness
dotnet test              # fixture parity and the example host; 100% line, branch, and method coverage
dotnet format --verify-no-changes
npm run verify           # docs, Sass, Node tests
```

`dotnet` must be the .NET 10 SDK (`export PATH="$HOME/.dotnet:$PATH"` when it is installed in the user profile). Coverage excludes generated Razor views (`*.cshtml`) and vendor assets. Settings: [`coverlet.library.runsettings`](../coverlet.library.runsettings) and [`coverlet.example.runsettings`](../coverlet.example.runsettings).

Local site:

```sh
dotnet run --project src/GovUk.Frontend.Example
```

## Tests and coverage

xUnit and FluentAssertions. Parity tests load `fixtures.json` with `System.Text.Json` from `node_modules/govuk-frontend/.../fixtures.json`. Set `GOVUK_COMPONENT=input` to run one component’s parity tests.

Coverage gate:

```sh
dotnet test tests/GovUk.Frontend.Tests/GovUk.Frontend.Tests.csproj --settings coverlet.library.runsettings
dotnet test tests/GovUk.Frontend.Example.Tests/GovUk.Frontend.Example.Tests.csproj --settings coverlet.example.runsettings
```

## Hard constraints (always)

- GOV.UK Frontend pins a single version; CSS, JavaScript, and fixtures must match.
- C# renderers track Nunjucks macros. Do not maintain copy-pasted HTML from each release, and do not shell out to Node to render a request.
- Component options mirror Nunjucks macro options and fixture `options`.
- Backend output must match every fixture `html` byte for byte. Nunjucks-vs-fixture checks prove freshness only.
- Compile CSS via Sass ([styles.md](styles.md)); `govuk-overrides.scss` last; never `!important` in service CSS. Do not serve `govuk-frontend.min.css` as the stylesheet.
- Patterns compose components; they are not new low-level components.
- No Blazor and no frontend UI framework for GOV.UK chrome.
- Coverage: **100%** functions, branches, and statements for the library and the example host, excluding generated Razor views. See [testing-components.md](testing-components.md).
- Before every Frontend upgrade: https://github.com/alphagov/govuk-frontend/releases/latest

## Version pin

| Item                          | Value                                                                                                        |
| ----------------------------- | ------------------------------------------------------------------------------------------------------------ |
| Implementation language       | C# / ASP.NET Core / .NET 10                                                                                  |
| Templating                    | Razor Pages call native C# renderers. Fixture options are a `ParamBag`.                                      |
| `govuk-frontend` (Node)       | `6.5.1` — check [latest release](https://github.com/alphagov/govuk-frontend/releases/latest) before upgrades |
| Sass pipeline                 | `styles/application.scss` → fingerprinted `/assets/{hash}.css`                                               |
| Nunjucks fixture verification | `tests/govuk-fixtures/render-fixtures.mjs`                                                                   |
| Page template reference       | https://design-system.service.gov.uk/styles/page-template/                                                   |
| Fixture testing guide         | https://frontend.design-system.service.gov.uk/testing-your-html/                                             |
| Run the example               | `dotnet run --project src/GovUk.Frontend.Example`                                                            |
