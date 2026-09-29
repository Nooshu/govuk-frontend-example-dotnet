# Label

Label does not have its own Design System page. It is markup that other GOV.UK Frontend components include.

## When to use

Use a label for every form field. Other form components render the label for you.

## Model

`GovUkHtml.Component("label", options)` renders this component. `options` is a `JsonObject` read as a `ParamBag`. The fields match the GOV.UK Frontend 6.5.1 Nunjucks macro. Plain `text` is escaped. An `html` value must already be trusted HTML from another component or from a fixture. Do not pass user input as HTML.

Pages must not paste `govuk-*` markup. The renderer lives in `src/GovUk.Frontend` and is discovered by `ComponentCatalog`.

## Example

Open `/components/label` to list every fixture, including hidden fixtures, and render the selected one. The raw fragment is `/components/label/raw`.

## Parity

17 official fixtures. The C# renderer must match each fixture `html` exactly. See [testing-components.md](testing-components.md).
