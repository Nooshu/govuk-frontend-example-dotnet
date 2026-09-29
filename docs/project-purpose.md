# Project purpose

This repository is a **C# / ASP.NET Core** example of a **GDS-compliant** government frontend.

## Intent

The example shows a service that:

1. Meets [Service Standard](https://www.gov.uk/service-manual/service-standard) and [Technology Code of Practice](https://www.gov.uk/guidance/the-technology-code-of-practice) expectations for common components, accessibility, and open standards — as far as the UI layer can.
2. Uses **C# on .NET 10** for HTML generation and application logic.
3. Uses **[GOV.UK Frontend](https://frontend.design-system.service.gov.uk/)** (latest pinned release) as the **only** frontend component library — styles, progressive-enhancement JS, and the HTML contract in `template.njk`.
4. Does **not** introduce SPA or component **frontend frameworks** (React, Vue, Angular, Svelte, Next.js UI layers, Blazor, etc.) for rendering GOV.UK UI.
5. Derives component HTML from the C# renderers that track **GOV.UK Frontend** `template.njk`, and wires **official test fixtures** for extensive **100% HTML parity** testing of that C# output.

## Priorities

1. Frontend web performance
2. Frontend security
3. Reduced maintenance
4. Accessibility
5. Inclusive design

See [priorities.md](priorities.md).

The `govuk-frontend` package is installed with npm. Request HTML is C#. See [tech-stack.md](tech-stack.md).

## What “GDS compliant” means here

Follow official GDS guidance and the Design System / Frontend contracts rather than inventing parallel UI systems. Canonical places to search: [guidance-sources.md](guidance-sources.md).

Using this template does **not** by itself make a live service assessment-ready — see [service-assessment-readiness.md](service-assessment-readiness.md) and [assisted digital](https://www.gov.uk/service-manual/helping-people-to-use-your-service/assisted-digital-support-introduction).

## Agent skill

Coding agents should apply [`.cursor/skills/gds-compliant-frontend/SKILL.md`](../.cursor/skills/gds-compliant-frontend/SKILL.md) when scaffolding or reviewing work in this repo.
