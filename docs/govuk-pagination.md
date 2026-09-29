# Pagination

[Pagination](https://design-system.service.gov.uk/components/pagination/) on the GOV.UK Design System.

## When to use

Use pagination when a long list is split across pages.

## Model

`GovUkHtml.Component("pagination", options)` renders this component. `options` is a `JsonObject` read as a `ParamBag`. The fields match the GOV.UK Frontend 6.5.1 Nunjucks macro. Plain `text` is escaped. An `html` value must already be trusted HTML from another component or from a fixture. Do not pass user input as HTML.

Pages must not paste `govuk-*` markup. The renderer lives in `src/GovUk.Frontend` and is discovered by `ComponentCatalog`.

## Example

Open `/components/pagination` to list every fixture, including hidden fixtures, and render the selected one. The raw fragment is `/components/pagination/raw`.

## Parity

15 official fixtures. The C# renderer must match each fixture `html` exactly. See [testing-components.md](testing-components.md).
