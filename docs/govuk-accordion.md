# Accordion

[Accordion](https://design-system.service.gov.uk/components/accordion/) on the GOV.UK Design System.

## When to use

Use an accordion to let people show and hide sections of related content.

## Model

`GovUkHtml.Component("accordion", options)` renders this component. `options` is a `JsonObject` read as a `ParamBag`. The fields match the GOV.UK Frontend 6.5.1 Nunjucks macro. Plain `text` is escaped. An `html` value must already be trusted HTML from another component or from a fixture. Do not pass user input as HTML.

Pages must not paste `govuk-*` markup. The renderer lives in `src/GovUk.Frontend` and is discovered by `ComponentCatalog`.

## Example

Open `/components/accordion` to list every fixture, including hidden fixtures, and render the selected one. The raw fragment is `/components/accordion/raw`.

## Parity

13 official fixtures. The C# renderer must match each fixture `html` exactly. See [testing-components.md](testing-components.md).
