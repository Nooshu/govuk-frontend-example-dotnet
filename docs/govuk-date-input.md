# Date input

[Date input](https://design-system.service.gov.uk/components/date-input/) on the GOV.UK Design System.

## When to use

Use a date input for a day, month, and year, such as a date of birth.

## Model

`GovUkHtml.Component("date-input", options)` renders this component. `options` is a `JsonObject` read as a `ParamBag`. The fields match the GOV.UK Frontend 6.5.1 Nunjucks macro. Plain `text` is escaped. An `html` value must already be trusted HTML from another component or from a fixture. Do not pass user input as HTML.

Pages must not paste `govuk-*` markup. The renderer lives in `src/GovUk.Frontend` and is discovered by `ComponentCatalog`.

## Example

Open `/components/date-input` to list every fixture, including hidden fixtures, and render the selected one. The raw fragment is `/components/date-input/raw`.

## Parity

50 official fixtures. The C# renderer must match each fixture `html` exactly. See [testing-components.md](testing-components.md).
