# File upload

[File upload](https://design-system.service.gov.uk/components/file-upload/) on the GOV.UK Design System.

## When to use

Use a file upload when people need to choose a file from their device.

## Model

`GovUkHtml.Component("file-upload", options)` renders this component. `options` is a `JsonObject` read as a `ParamBag`. The fields match the GOV.UK Frontend 6.5.1 Nunjucks macro. Plain `text` is escaped. An `html` value must already be trusted HTML from another component or from a fixture. Do not pass user input as HTML.

Pages must not paste `govuk-*` markup. The renderer lives in `src/GovUk.Frontend` and is discovered by `ComponentCatalog`.

## Example

Open `/components/file-upload` to list every fixture, including hidden fixtures, and render the selected one. The raw fragment is `/components/file-upload/raw`.

## Parity

22 official fixtures. The C# renderer must match each fixture `html` exactly. See [testing-components.md](testing-components.md).
