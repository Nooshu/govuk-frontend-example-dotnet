# GOV.UK components — architecture

## Source of truth

[GOV.UK Frontend](https://frontend.design-system.service.gov.uk/) **Nunjucks** macros and fixtures (Node package `govuk-frontend` 6.5.1). This repo re-implements that HTML contract in C#. Pages call `GovUkHtml.Component`. See [tech-stack.md](tech-stack.md).

## Architecture (intended)

```text
Page / pattern
  → library API (one entry per component)
    → options model (aligned with Nunjucks macro options)
      → renderer → exact HTML string
        → Frontend CSS/JS in the page shell
```

Supporting pieces:

- **Shared HTML helpers** — Nunjucks-compatible escape + attribute serialization.
- **Fixture loader** — cached `fixtures.json` for Previews / Fixtures.
- **Options mapper** — fixture `options` → model (including edge cases).
- **Layout chrome** — shared skip link / header / footer / service nav / pattern back link.
- **Nunjucks suite (Node)** — proves stored fixtures still match Frontend macros.

See [layout-chrome.md](layout-chrome.md), [creating-components.md](creating-components.md), [testing-components.md](testing-components.md).

## Components vs patterns

|                | Components                  | Patterns               |
| -------------- | --------------------------- | ---------------------- |
| Design System  | `/components/`              | `/patterns/` and Pages |
| Implementation | Library wrappers + fixtures | Composed pages         |
| Parity suite   | Required                    | Not applicable         |

## Shipped component set

Every component directory in govuk-frontend 6.5.1 that contains `fixtures.json` has a renderer, a catalogue page, and a short doc:

| Component           | Doc                                                          |
| ------------------- | ------------------------------------------------------------ |
| Accordion           | [govuk-accordion.md](govuk-accordion.md)                     |
| Back link           | [govuk-back-link.md](govuk-back-link.md)                     |
| Breadcrumbs         | [govuk-breadcrumbs.md](govuk-breadcrumbs.md)                 |
| Button              | [govuk-button.md](govuk-button.md)                           |
| Character count     | [govuk-character-count.md](govuk-character-count.md)         |
| Checkboxes          | [govuk-checkboxes.md](govuk-checkboxes.md)                   |
| Cookie banner       | [govuk-cookie-banner.md](govuk-cookie-banner.md)             |
| Date input          | [govuk-date-input.md](govuk-date-input.md)                   |
| Details             | [govuk-details.md](govuk-details.md)                         |
| Error message       | [govuk-error-message.md](govuk-error-message.md)             |
| Error summary       | [govuk-error-summary.md](govuk-error-summary.md)             |
| Exit this page      | [govuk-exit-this-page.md](govuk-exit-this-page.md)           |
| Feedback            | [govuk-feedback.md](govuk-feedback.md)                       |
| Fieldset            | [govuk-fieldset.md](govuk-fieldset.md)                       |
| File upload         | [govuk-file-upload.md](govuk-file-upload.md)                 |
| Footer              | [govuk-footer.md](govuk-footer.md)                           |
| Generic header      | [govuk-generic-header.md](govuk-generic-header.md)           |
| Header              | [govuk-header.md](govuk-header.md)                           |
| Hint                | [govuk-hint.md](govuk-hint.md)                               |
| Inset text          | [govuk-inset-text.md](govuk-inset-text.md)                   |
| Label               | [govuk-label.md](govuk-label.md)                             |
| Language navigation | [govuk-language-navigation.md](govuk-language-navigation.md) |
| Notification banner | [govuk-notification-banner.md](govuk-notification-banner.md) |
| Pagination          | [govuk-pagination.md](govuk-pagination.md)                   |
| Panel               | [govuk-panel.md](govuk-panel.md)                             |
| Password input      | [govuk-password-input.md](govuk-password-input.md)           |
| Phase banner        | [govuk-phase-banner.md](govuk-phase-banner.md)               |
| Radios              | [govuk-radios.md](govuk-radios.md)                           |
| Select              | [govuk-select.md](govuk-select.md)                           |
| Service navigation  | [govuk-service-navigation.md](govuk-service-navigation.md)   |
| Skip link           | [govuk-skip-link.md](govuk-skip-link.md)                     |
| Summary list        | [govuk-summary-list.md](govuk-summary-list.md)               |
| Table               | [govuk-table.md](govuk-table.md)                             |
| Tabs                | [govuk-tabs.md](govuk-tabs.md)                               |
| Tag                 | [govuk-tag.md](govuk-tag.md)                                 |
| Task list           | [govuk-task-list.md](govuk-task-list.md)                     |
| Text input          | [govuk-input.md](govuk-input.md)                             |
| Textarea            | [govuk-textarea.md](govuk-textarea.md)                       |
| Warning text        | [govuk-warning-text.md](govuk-warning-text.md)               |

## Previews

Each component has a catalogue page listing every fixture, including hidden fixtures, and rendering only the selected fixture with a parity banner. The home page lists links only — **no live demos on the homepage**. Routes stay available in every environment for this demonstration. A real service should limit them to Development. See [preview-server.md](preview-server.md).

## Do not

- Custom CSS for Design System appearance.
- Unofficial step-nav or invented `govuk-*` chrome.
- Embed demos on the index.
- Rebuild skip link / header / footer models per request when shared chrome helpers exist.
- Force another ecosystem’s folder layout once a language is chosen — follow that language’s best practices.
