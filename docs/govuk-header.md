# Header

[Header](https://design-system.service.gov.uk/components/header/) on the GOV.UK Design System.

## When to use

Use the GOV.UK header so people can recognise a government service.

## Model

`GovUkHtml.Component("header", options)` renders this component. `options` is a `JsonObject` read as a `ParamBag`. The fields match the GOV.UK Frontend 6.5.1 Nunjucks macro. Plain `text` is escaped. An `html` value must already be trusted HTML from another component or from a fixture. Do not pass user input as HTML.

Pages must not paste `govuk-*` markup. The renderer lives in `src/GovUk.Frontend` and is discovered by `ComponentCatalog`.

## Example

Open `/components/header` to list every fixture, including hidden fixtures, and render the selected one. The raw fragment is `/components/header/raw`.

## Parity

6 official fixtures. The C# renderer must match each fixture `html` exactly. See [testing-components.md](testing-components.md).
