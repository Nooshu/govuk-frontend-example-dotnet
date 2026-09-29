# Details

[Details](https://design-system.service.gov.uk/components/details/) on the GOV.UK Design System.

## When to use

Use details to hide content that only some people need.

## Model

`GovUkHtml.Component("details", options)` renders this component. `options` is a `JsonObject` read as a `ParamBag`. The fields match the GOV.UK Frontend 6.5.1 Nunjucks macro. Plain `text` is escaped. An `html` value must already be trusted HTML from another component or from a fixture. Do not pass user input as HTML.

Pages must not paste `govuk-*` markup. The renderer lives in `src/GovUk.Frontend` and is discovered by `ComponentCatalog`.

## Example

Open `/components/details` to list every fixture, including hidden fixtures, and render the selected one. The raw fragment is `/components/details/raw`.

## Parity

10 official fixtures. The C# renderer must match each fixture `html` exactly. See [testing-components.md](testing-components.md).
