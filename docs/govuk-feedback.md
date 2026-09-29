# Feedback

[Feedback](https://design-system.service.gov.uk/components/feedback/) on the GOV.UK Design System.

## When to use

Use feedback when a page can ask whether it was useful. This is a trial component in GOV.UK Frontend 6.5.

## Model

`GovUkHtml.Component("feedback", options)` renders this component. `options` is a `JsonObject` read as a `ParamBag`. The fields match the GOV.UK Frontend 6.5.1 Nunjucks macro. Plain `text` is escaped. An `html` value must already be trusted HTML from another component or from a fixture. Do not pass user input as HTML.

Pages must not paste `govuk-*` markup. The renderer lives in `src/GovUk.Frontend` and is discovered by `ComponentCatalog`.

## Example

Open `/components/feedback` to list every fixture, including hidden fixtures, and render the selected one. The raw fragment is `/components/feedback/raw`.

## Parity

7 official fixtures. The C# renderer must match each fixture `html` exactly. See [testing-components.md](testing-components.md).
