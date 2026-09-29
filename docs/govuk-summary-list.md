# Summary list

[Summary list](https://design-system.service.gov.uk/components/summary-list/) on the GOV.UK Design System.

## When to use

Use a summary list to show the answers people need to check.

## Model

`GovUkHtml.Component("summary-list", options)` renders this component. `options` is a `JsonObject` read as a `ParamBag`. The fields match the GOV.UK Frontend 6.5.1 Nunjucks macro. Plain `text` is escaped. An `html` value must already be trusted HTML from another component or from a fixture. Do not pass user input as HTML.

Pages must not paste `govuk-*` markup. The renderer lives in `src/GovUk.Frontend` and is discovered by `ComponentCatalog`.

## Example

Open `/components/summary-list` to list every fixture, including hidden fixtures, and render the selected one. The raw fragment is `/components/summary-list/raw`.

## Parity

30 official fixtures. The C# renderer must match each fixture `html` exactly. See [testing-components.md](testing-components.md).
