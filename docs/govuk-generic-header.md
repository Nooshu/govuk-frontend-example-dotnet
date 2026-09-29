# Generic header

Generic header does not have its own Design System page. It is markup that other GOV.UK Frontend components include.

## When to use

The generic header is shared header markup used by the GOV.UK header. Pages use the header component.

## Model

`GovUkHtml.Component("generic-header", options)` renders this component. `options` is a `JsonObject` read as a `ParamBag`. The fields match the GOV.UK Frontend 6.5.1 Nunjucks macro. Plain `text` is escaped. An `html` value must already be trusted HTML from another component or from a fixture. Do not pass user input as HTML.

Pages must not paste `govuk-*` markup. The renderer lives in `src/GovUk.Frontend` and is discovered by `ComponentCatalog`.

## Example

Open `/components/generic-header` to list every fixture, including hidden fixtures, and render the selected one. The raw fragment is `/components/generic-header/raw`.

## Parity

7 official fixtures. The C# renderer must match each fixture `html` exactly. See [testing-components.md](testing-components.md).
