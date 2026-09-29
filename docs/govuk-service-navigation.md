# Service navigation

[Service navigation](https://design-system.service.gov.uk/components/service-navigation/) on the GOV.UK Design System.

## When to use

Use service navigation for the links that belong to this service.

## Model

`GovUkHtml.Component("service-navigation", options)` renders this component. `options` is a `JsonObject` read as a `ParamBag`. The fields match the GOV.UK Frontend 6.5.1 Nunjucks macro. Plain `text` is escaped. An `html` value must already be trusted HTML from another component or from a fixture. Do not pass user input as HTML.

Pages must not paste `govuk-*` markup. The renderer lives in `src/GovUk.Frontend` and is discovered by `ComponentCatalog`.

## Example

Open `/components/service-navigation` to list every fixture, including hidden fixtures, and render the selected one. The raw fragment is `/components/service-navigation/raw`.

## Parity

33 official fixtures. The C# renderer must match each fixture `html` exactly. See [testing-components.md](testing-components.md).
